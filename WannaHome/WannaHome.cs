using Dalamud.Game.Command;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WannaHome.Common;
using WannaHome.Data;
using WannaHome.Model;
using WannaHome.Structure;

namespace WannaHome
{
	public sealed class WannaHome : IDalamudPlugin
	{
		public static WannaHome Instance { get; private set; }
		public const string Plugin_Name = "WannaHome";
		public string Name => "Wanna Home";
		public static string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

		private const string commandName = "/wh";

		public DalamudPluginInterface PluginInterface { get; init; }
		public PluginUI PluginUi { get; init; }
		public Configuration Config { get; init; }

		/// <summary>
		/// 服务器ID-房区ID-房区序号-List
		/// </summary>
		public Dictionary<ushort, Dictionary<ushort, Dictionary<ushort, List<LandInfo>>>> landMap = new();

		public ushort ServerId = 0, TerritoryId = 0, WardId = 0;

		public WannaHome(
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface
		) {
			Instance = this;
			PluginInterface = pluginInterface;
			Service.Initialize(pluginInterface);


			Config = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
			Config.Initialize(this.PluginInterface);

			PluginUi = new PluginUI(this, Config);

			Service.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
				HelpMessage = "打开主界面；\n/wh cfg打开设置界面"
			});

			PluginInterface.UiBuilder.Draw += DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
			Service.ClientState.Login += Login;
			Service.ClientState.TerritoryChanged += TerritoryChange;
			Service.GameNetwork.NetworkMessage += NetworkMessageDelegate;
			LoadLandMap();
		}

		public void Dispose() {
			PluginUi.Dispose();
			Service.CommandManager.RemoveHandler(commandName);
			PluginInterface.UiBuilder.Draw -= DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
			Service.ClientState.Login -= Login;
			Service.ClientState.TerritoryChanged -= TerritoryChange;
			Service.GameNetwork.NetworkMessage -= NetworkMessageDelegate;
			SaveLandMap().Wait();
		}

		private void OnCommand(string command, string args) {
			// in response to the slash command, just display our main ui
			string arg = args.Trim().Replace("\"", string.Empty);
			if (string.IsNullOrEmpty(arg)) {
				this.PluginUi.Visible = !this.PluginUi.Visible;
			} else if (arg == "cfg") {
				this.PluginUi.SettingsVisible = !this.PluginUi.SettingsVisible;
			}
		}

		private void DrawUI() {
			this.PluginUi.Draw();
		}

		private void DrawConfigUI() {
			this.PluginUi.SettingsVisible = true;
		}

		public void SaveLandList(ushort serverId, ushort territoryId, ushort wardId, List<LandInfo> landList) {
			ServerId = serverId;
			TerritoryId = territoryId;
			WardId = wardId;

			if (landMap.ContainsKey(serverId)) {
				if (landMap[serverId].ContainsKey(territoryId)) {
					landMap[serverId][territoryId][wardId] = landList;
				} else {
					landMap[serverId][territoryId] = new() { { wardId, landList } };
				}
			} else {
				landMap[serverId] = new() { { territoryId, new() { { wardId, landList } } } };
			}
		}
		public Task LoadLandMap() =>
			Task.Run(async () => {
				var dataPath = Path.Join(this.PluginInterface.ConfigDirectory.FullName, $"LandInfo.txt");
				if (File.Exists(dataPath)) {
					try {
						using (var stream = new StreamReader(File.Open(dataPath, FileMode.OpenOrCreate), Encoding.UTF8)) {
							var str = await stream.ReadToEndAsync();
							var data = JsonSerializer.Deserialize<PluginData>(str);
							if (data != null) {
								ServerId = data.ServerId;
								TerritoryId = data.TerritoryId;
								WardId = data.WardId;
								landMap = data.LandInfo;
								PluginLog.Debug($"LandMap导入成功");
							}
						}

					} catch (Exception e) {
						PluginLog.Error($"LandMap导入：{e}");
					}
				}
			});

		public Task SaveLandMap() =>
			Task.Run(() => {
				var dataPath = Path.Join(this.PluginInterface.ConfigDirectory.FullName, $"LandInfo.txt");
				try {
					using (var stream = new StreamWriter(File.Open(dataPath, FileMode.Create), Encoding.UTF8)) {
						PluginData data = new() { ServerId = ServerId, TerritoryId = TerritoryId, WardId = WardId, LandInfo = landMap };
						stream.Write(JsonSerializer.Serialize(data));
						stream.Flush();
						PluginLog.Debug($"LandMap保存成功");
					}
				} catch (Exception e) {
					PluginLog.Error($"LandMap保存：{e}");
				}
			});

		private void Login(object? sender, EventArgs e) {
			Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => {

			});
		}
		private void TerritoryChange(object? sender, ushort territoryId) {

			var gameVersion = Service.DataManager.GameData.Repositories.First(repo => repo.Key == "ffxiv").Value.Version;
			if (Territory.TerritoriesMap.ContainsKey(territoryId) && Config.GameVersion != gameVersion) {
				PluginUi.Setting.ScanOpcode();
			}
		}
		private void NetworkMessageDelegate(IntPtr dataPtr, ushort opcode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction) {

			if (Service.DataManager.IsDataReady && opcode == Service.DataManager.ServerOpCodes["HousingWardInfo"]) {
				HouseWard.onHousingWardInfo(Marshal.PtrToStructure<HousingWardInfo>(dataPtr));
			} else if (opcode == Config.ClientTriggerOpcode) {
				HouseVote.onClientTrigger(Marshal.PtrToStructure<ClientTrigger>(dataPtr));
			} else if (opcode == Config.VoteInfoOpcode) {
				HouseVote.onVoteInfo(Marshal.PtrToStructure<VoteInfo>(dataPtr));
			}

			if (Config.Debug) {
				var direc = direction switch {
					NetworkMessageDirection.ZoneUp => "↑Up: ",
					NetworkMessageDirection.ZoneDown => "↓Down: ",
					_ => "Unknown: "
				};
				PluginLog.Debug($"{direc}{opcode}");

				if (opcode == Config.DebugOpcode) {
					byte[] data = new byte[0x20];
					Marshal.Copy(dataPtr, data, 0, 0x20);
					Service.ChatGui.Print(BitConverter.ToString(data));
				}
			}
		}
	}
}
