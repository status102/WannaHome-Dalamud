using Dalamud.Game.Network;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WannaHome.Data;
using WannaHome.Model.Structure;

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
		public bool captureOpCode { private set; get; } = false;
		private bool voteInfoCapture = false, clientTriggerCapture = false;
		private Dictionary<ushort, long> initOp = new(), houseOp = new();
		private string GameVerson => WannaHome.DataManager.GameData.Repositories.First(repo => repo.Key == "ffxiv").Value.Version;
		#region init
		public Calculate(WannaHome wannaHome) {
			this.WannaHome = wannaHome;
			HouseWard = new(wannaHome);
			HouseVote = new(wannaHome);
			WannaHome.GameNetwork.NetworkMessage += CaptureOpCodeDelegate;
			WannaHome.GameNetwork.NetworkMessage += NetworkMessageDelegate;
			wannaHome.ClientState.TerritoryChanged += TerritoryChange;
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
			WannaHome.GameNetwork.NetworkMessage -= NetworkMessageDelegate;
			WannaHome.GameNetwork.NetworkMessage -= CaptureOpCodeDelegate;

			if (networkMessageWriter != null) {
				networkMessageWriter.Flush();
				networkMessageWriter.Close();
			}
		}
		#endregion

		private void TerritoryChange(object? sender, ushort territoryId) {
			if (Territory.TerritoriesMap.ContainsKey(territoryId) && Config.GameVersion != GameVerson)
				CaptureOpCode();
			else
				CaptureCancel();
		}
		private void NetworkMessageDelegate(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {
			if (captureOpCode)
				return;
			var direc = direction switch
			{
				NetworkMessageDirection.ZoneUp => "↑Up: ",
				NetworkMessageDirection.ZoneDown => "↓Down: ",
				_ => "Unknown: "
			};
			if (Config.Debug) {
				PluginLog.Debug($"{direc}{opCode}");
			}
			if (Config.SavePackage) {
				var savePtr = dataPtr - 0x20;
				byte[] data = new byte[2408 + 32];
				Marshal.Copy(savePtr, data, 0, 2408 + 32);
				networkMessageWriter?.WriteLine($"[{direc}{opCode}]{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}：{BitConverter.ToString(data)}");
				//data.ToList().ForEach(b =>Convert.ToHexString());

				networkMessageWriter?.Flush();
			}
			if (opCode == WannaHome.DataManager.ServerOpCodes["HousingWardInfo"])
				HouseWard.onHousingWardInfo((HousingWardInfo?)Marshal.PtrToStructure(dataPtr, typeof(HousingWardInfo)));
			else if (opCode == Config.ClientTriggerOpCode) {
				//ClientTrigger
				HouseVote.onClientTrigger((ClientTrigger?)Marshal.PtrToStructure(dataPtr, typeof(ClientTrigger)));
			} else if (opCode == Config.VoteInfoOpCode) {
				HouseVote.onVoteInfo((VoteInfo?)Marshal.PtrToStructure(dataPtr, typeof(VoteInfo)));
			}
			if (Config.Debug && opCode == Config.DebugOpCode) {
				byte[] data = new byte[0x20];
				Marshal.Copy(dataPtr, data, 0, 0x20);
				WannaHome.ChatGui.Print(BitConverter.ToString(data));
			}
		}
		private void CaptureOpCodeDelegate(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {
			if (!captureOpCode)
				return;
			var direc = direction switch
			{
				NetworkMessageDirection.ZoneUp => "↑Up: ",
				NetworkMessageDirection.ZoneDown => "↓Down: ",
				_ => "Unknown: "
			};
			var int32_0 = (uint)Marshal.ReadInt32(dataPtr);
			var byte_0 = Marshal.ReadByte(dataPtr);
			var byte_1 = Marshal.ReadByte(dataPtr, 1);
			var byte_2 = Marshal.ReadByte(dataPtr, 2);

			PluginLog.Debug($"获取OpCode中：{direc}Opcode：{opCode}，int0：{int32_0}，Byte0：{byte_0}，Byte1：{byte_1}，Byte2：{byte_2}\n{Enum.IsDefined(typeof(PurchaseType), byte_0)}，{Enum.IsDefined(typeof(TenantType), byte_1)}，{Enum.IsDefined(typeof(AvailableType), byte_2)}");
			if (Enum.IsDefined(typeof(PurchaseType), byte_0) && Enum.IsDefined(typeof(TenantType), byte_1) && Enum.IsDefined(typeof(AvailableType), byte_2)) {
				var int32_1 = (uint)Marshal.ReadInt32(dataPtr, 4);
				var diff = (int32_1 - startUnix) % interval;
				PluginLog.Debug($"尝试获取VoteInfoOpCode：{direc}Opcode：{opCode}，Byte0：{byte_0}，Byte1：{byte_1}，Byte2：{byte_2}，Int：{int32_1}，diff：{diff}");
				if ( diff == 0 || diff == showDelay) {
					Config.VoteInfoOpCode = opCode;
					voteInfoCapture = true;
					PluginLog.Information($"VoteInfoOpCode获取成功：{Config.VoteInfoOpCode}");
					Config.Save();
				}
			}
			if (int32_0 == 0x0003)
				initOp[opCode] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
			if (int32_0 == 0x0452 || int32_0 == 0x0451)
				houseOp[opCode] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

			if ((int32_0 == 3 || int32_0 == 0x0452 || int32_0 == 0x0451) && initOp.ContainsKey(opCode) && houseOp.ContainsKey(opCode)) {
				var diff = houseOp[opCode] - initOp[opCode];
				if (diff >= 0 && diff < 5) {
					Config.ClientTriggerOpCode = opCode;
					clientTriggerCapture = true;
					PluginLog.Information($"ClientTriggerOpCode获取成功：{Config.ClientTriggerOpCode}");
					Config.Save();
				}
			}
			if (voteInfoCapture && clientTriggerCapture) {
				captureOpCode = false;
				Config.GameVersion = GameVerson;
				PluginLog.Information($"OpCode刷新成功，GameVersion：${Config.GameVersion}，ClientTrigger：{Config.ClientTriggerOpCode}，VoteInfo：{Config.VoteInfoOpCode}");
				WannaHome.ChatGui.Print($"[{WannaHome.Name}]OpCode刷新成功\nGameVersion：${Config.GameVersion}\nClientTrigger：{Config.ClientTriggerOpCode}\nVoteInfo：{Config.VoteInfoOpCode}");
				Config.Save();
			}
		}
		public void CaptureOpCode() {
			captureOpCode = true;
			voteInfoCapture = clientTriggerCapture = false;
			initOp = new();
			houseOp = new();
			PluginLog.Information($"开始刷新OpCode");
		}
		public void CaptureCancel() {
			captureOpCode = false;
			initOp.Clear();
			houseOp.Clear();
		}
	}
}
