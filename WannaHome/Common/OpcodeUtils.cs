using Dalamud.Game.Network;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WannaHome.Structure;

namespace WannaHome.Common
{
	public class OpcodeUtils
	{
		private const long startUnix = 1659970800, interval = 9 * 24 * 60 * 60, showDelay = 5 * 24 * 60 * 60;
		private static ushort ClientTriggerOpcode = 0, VoteInfoOpcode = 0;


		private static Dictionary<ushort, long> initOp = new(), houseOp = new();
		private static Action<ushort, ushort>? onFinish = null;

		public static void Scan(Action<ushort, ushort> func) {
			ClientTriggerOpcode = VoteInfoOpcode = 0;
			onFinish = func;
			Service.GameNetwork.NetworkMessage += ScanOpcodeDelegate;
		}

		public static void Cancel() {
			if (onFinish == null) { return; }
			Service.GameNetwork.NetworkMessage -= ScanOpcodeDelegate;
			onFinish?.Invoke(0, 0);
			onFinish = null;
		}

		private static void ScanOpcodeDelegate(IntPtr dataPtr, ushort opcode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {

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
					VoteInfoOpcode = opcode;
					PluginLog.Debug($"VoteInfoOpCode获取成功：{VoteInfoOpcode}");
				}
			}
			if (int32_0 == 0x0003)
				initOp[opcode] = DateTimeOffset.Now.ToUnixTimeSeconds();
			if (int32_0 == 0x0452 || int32_0 == 0x0451)
				houseOp[opcode] = DateTimeOffset.Now.ToUnixTimeSeconds();

			if ((int32_0 == 3 || int32_0 == 0x0452 || int32_0 == 0x0451) && initOp.ContainsKey(opcode) && houseOp.ContainsKey(opcode)) {
				var diff = houseOp[opcode] - initOp[opcode];
				if (diff >= 0 && diff < 5) {
					ClientTriggerOpcode = opcode;
					PluginLog.Debug($"ClientTriggerOpcode获取成功：{ClientTriggerOpcode}");
				}
			}
			Check();
		}
		private static void Check() {
			if (ClientTriggerOpcode != 0 && VoteInfoOpcode != 0) {
				Service.GameNetwork.NetworkMessage -= ScanOpcodeDelegate;
				onFinish?.Invoke(ClientTriggerOpcode, VoteInfoOpcode);
				onFinish = null;
			}
		}
	}
}
