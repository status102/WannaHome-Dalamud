using Dalamud.Game.Network;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WannaHome.Common;
using WannaHome.Data;
using WannaHome.Structure;

namespace WannaHome
{
	public class Calculate : IDisposable
	{
		private StreamWriter? networkMessageWriter;
		private const long startUnix = 1659970800, interval = 9 * 24 * 60 * 60, showDelay = 5 * 24 * 60 * 60;
		private readonly WannaHome WannaHome;
		private readonly HouseWard HouseWard;
		private readonly HouseVote HouseVote;
		private Configuration Config => WannaHome.Configuration;
		public bool captureOpcode { private set; get; } = false;
		private bool voteInfoCapture = false, clientTriggerCapture = false;
		private Dictionary<ushort, long> initOp = new(), houseOp = new();
		private string GameVerson => Service.DataManager.GameData.Repositories.First(repo => repo.Key == "ffxiv").Value.Version;
		#region init
		public Calculate(WannaHome wannaHome) {
			this.WannaHome = wannaHome;
			HouseWard = new(wannaHome);
			HouseVote = new(wannaHome);
			Service.GameNetwork.NetworkMessage += CaptureOpcodeDelegate;
			Service.GameNetwork.NetworkMessage += NetworkMessageDelegate;
			Service.ClientState.TerritoryChanged += TerritoryChange;
			/*
			try {
				FileStream stream = File.Open(Path.Join(WannaHome.PluginInterface.ConfigDirectory.FullName, String.Format("网络包{0:}.log", DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss"))), FileMode.OpenOrCreate);
				if (stream != null && stream.CanWrite)
					networkMessageWriter = new StreamWriter(stream);
			} catch (IOException e) {
				PluginLog.Error(e.ToString());
				networkMessageWriter = null;
				//DalamudDll.ChatGui.PrintError("网络包初始化：" + e.ToString());
			}*/

		}

		public void Dispose() {
			Service.GameNetwork.NetworkMessage -= NetworkMessageDelegate;
			Service.GameNetwork.NetworkMessage -= CaptureOpcodeDelegate;

			if (networkMessageWriter != null) {
				networkMessageWriter.Flush();
				networkMessageWriter.Close();
			}
		}
		#endregion

		private void TerritoryChange(object? sender, ushort territoryId) {
			if (Territory.TerritoriesMap.ContainsKey(territoryId) && Config.GameVersion != GameVerson)
				CaptureOpcode();
			else
				CaptureCancel();
		}
		private void NetworkMessageDelegate(IntPtr dataPtr, ushort opcode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {
			if (captureOpcode)
				return;
			var direc = direction switch {
				NetworkMessageDirection.ZoneUp => "↑Up: ",
				NetworkMessageDirection.ZoneDown => "↓Down: ",
				_ => "Unknown: "
			};
			if (Config.Debug) { PluginLog.Debug($"{direc}{opcode}"); }
			
			if (Service.DataManager.IsDataReady && opcode == Service.DataManager.ServerOpCodes["HousingWardInfo"])
				HouseWard.onHousingWardInfo(Marshal.PtrToStructure<HousingWardInfo>(dataPtr));
			else if (opcode == Config.ClientTriggerOpcode) {
				//ClientTrigger
				HouseVote.onClientTrigger(Marshal.PtrToStructure<ClientTrigger>(dataPtr));
			} else if (opcode == Config.VoteInfoOpcode) {
				HouseVote.onVoteInfo(Marshal.PtrToStructure<VoteInfo>(dataPtr));
			}
			if (Config.Debug && opcode == Config.DebugOpcode) {
				byte[] data = new byte[0x20];
				Marshal.Copy(dataPtr, data, 0, 0x20);
				Service.ChatGui.Print(BitConverter.ToString(data));
			}
		}
		private void CaptureOpcodeDelegate(IntPtr dataPtr, ushort opcode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {
			if (!captureOpcode)
				return;
			var direc = direction switch {
				NetworkMessageDirection.ZoneUp => "↑Up: ",
				NetworkMessageDirection.ZoneDown => "↓Down: ",
				_ => "Unknown: "
			};
			var int32_0 = (uint)Marshal.ReadInt32(dataPtr);
			var byte_0 = Marshal.ReadByte(dataPtr);
			var byte_1 = Marshal.ReadByte(dataPtr, 1);
			var byte_2 = Marshal.ReadByte(dataPtr, 2);

			PluginLog.Debug($"获取OpCode中：{direc}Opcode：{opcode}，int0：{int32_0}，Byte0：{byte_0}，Byte1：{byte_1}，Byte2：{byte_2}\n{Enum.IsDefined(typeof(PurchaseType), byte_0)}，{Enum.IsDefined(typeof(TenantType), byte_1)}，{Enum.IsDefined(typeof(AvailableType), byte_2)}");
			if (Enum.IsDefined(typeof(PurchaseType), byte_0) && Enum.IsDefined(typeof(TenantType), byte_1) && Enum.IsDefined(typeof(AvailableType), byte_2)) {
				var int32_1 = (uint)Marshal.ReadInt32(dataPtr, 4);
				var diff = (int32_1 - startUnix) % interval;
				PluginLog.Debug($"尝试获取VoteInfoOpCode：{direc}Opcode：{opcode}，Byte0：{byte_0}，Byte1：{byte_1}，Byte2：{byte_2}，Int：{int32_1}，diff：{diff}");
				if (diff == 0 || diff == showDelay) {
					Config.VoteInfoOpcode = opcode;
					voteInfoCapture = true;
					PluginLog.Information($"VoteInfoOpCode获取成功：{Config.VoteInfoOpcode}");
					Config.Save();
				}
			}
			if (int32_0 == 0x0003)
				initOp[opcode] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
			if (int32_0 == 0x0452 || int32_0 == 0x0451)
				houseOp[opcode] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

			if ((int32_0 == 3 || int32_0 == 0x0452 || int32_0 == 0x0451) && initOp.ContainsKey(opcode) && houseOp.ContainsKey(opcode)) {
				var diff = houseOp[opcode] - initOp[opcode];
				if (diff >= 0 && diff < 5) {
					Config.ClientTriggerOpcode = opcode;
					clientTriggerCapture = true;
					PluginLog.Information($"ClientTriggerOpcode获取成功：{Config.ClientTriggerOpcode}");
					Config.Save();
				}
			}
			if (voteInfoCapture && clientTriggerCapture) {
				captureOpcode = false;
				Config.GameVersion = GameVerson;
				PluginLog.Information($"Opcode刷新成功，GameVersion：{Config.GameVersion}，ClientTrigger：{Config.ClientTriggerOpcode}，VoteInfo：{Config.VoteInfoOpcode}");
				Service.ChatGui.Print($"[{WannaHome.Plugin_Name}]Opcode刷新成功\nGameVersion：{Config.GameVersion}\nClientTrigger：{Config.ClientTriggerOpcode}\nVoteInfo：{Config.VoteInfoOpcode}");
				Config.Save();
			}
		}
		public void CaptureOpcode() {
			captureOpcode = true;
			voteInfoCapture = clientTriggerCapture = false;
			initOp = new();
			houseOp = new();
			PluginLog.Information($"开始刷新Opcode");
		}
		public void CaptureCancel() {
			captureOpcode = false;
			initOp.Clear();
			houseOp.Clear();
		}
	}
}
