using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WannaHome.Model;

namespace WannaHome
{
	public sealed class WannaHome : IDalamudPlugin
	{
		public static WannaHome? Instance { get; private set; }
		public string Name => "Wanna Home";
		public static string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

		private const string commandName = "/wh";

		public DalamudPluginInterface PluginInterface { get; init; }
		public PluginUI PluginUi { get; init; }
		public CommandManager CommandManager { get; init; }
		public Configuration Configuration { get; init; }
		public ClientState ClientState { get; init; }
		public ChatGui ChatGui { get; init; }
		public GameNetwork GameNetwork { get; init; }
		public DataManager DataManager { get; init; }
		public Calculate Calculate { get; init; }

		/// <summary>
		/// 服务器ID-房区ID-房区序号-List
		/// </summary>
		public Dictionary<ushort, Dictionary<ushort, Dictionary<ushort, List<LandInfo>>>> landMap = new();

		public ushort serverId = 0, territoryId = 0, wardId = 0;
		public ushort sendServerId, sendTerritoryId, sendWardId;

		public WannaHome(
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
			[RequiredVersion("1.0")] CommandManager commandManager,
			[RequiredVersion("1.0")] ClientState clientState,
			[RequiredVersion("1.0")] ChatGui chatGui,
			[RequiredVersion("1.0")] GameNetwork gameNetwork,
			[RequiredVersion("1.0")] DataManager dataManager
		) {
			Instance = this;
			this.PluginInterface = pluginInterface;
			this.CommandManager = commandManager;
			this.ClientState = clientState;
			this.ChatGui = chatGui;
			this.GameNetwork = gameNetwork;
			this.DataManager = dataManager;

			this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
			this.Configuration.Initialize(this.PluginInterface);

			// you might normally want to embed resources and load them from the manifest stream
			//var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
			// var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
			this.PluginUi = new PluginUI(this, this.Configuration);
			this.Calculate = new Calculate(this);

			this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
			{
				HelpMessage = "打开主界面；\n/wh cfg打开设置界面"
			});

			this.PluginInterface.UiBuilder.Draw += DrawUI;
			this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
			this.ClientState.Login += Login;
			LoadLandMap();
		}

		public void Dispose() {
			SaveLandMap().Wait();
			this.Calculate.Dispose();
			this.PluginUi.Dispose();
			this.CommandManager.RemoveHandler(commandName);
			this.ClientState.Login -= Login;
			this.PluginInterface.UiBuilder.Draw -= DrawUI;
			this.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
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

		public Task LoadLandMap() =>
			Task.Run(async () =>
			{
				var dataPath = Path.Join(this.PluginInterface.ConfigDirectory.FullName, $"LandInfo.txt");
				if (File.Exists(dataPath)) {
					try {
						using (var stream = new StreamReader(File.Open(dataPath, FileMode.OpenOrCreate), Encoding.UTF8)) {
							var str = await stream.ReadToEndAsync();
							var data = JsonSerializer.Deserialize<PluginData>(str);
							if (data != null) {
								serverId = data.serverId;
								sendServerId = data.serverId;
								territoryId = data.territoryId;
								sendTerritoryId = data.territoryId;
								wardId = data.wardId;
								sendWardId = data.wardId;
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
			Task.Run(() =>
			{
				var dataPath = Path.Join(this.PluginInterface.ConfigDirectory.FullName, $"LandInfo.txt");
				try {
					using (var stream = new StreamWriter(File.Open(dataPath, FileMode.Create), Encoding.UTF8)) {
						PluginData data = new() { serverId = serverId, territoryId = territoryId, wardId = wardId, LandInfo = landMap };
						stream.Write(JsonSerializer.Serialize(data));
						stream.Flush();
						PluginLog.Debug($"LandMap保存成功");
					}
				} catch (Exception e) {
					PluginLog.Error($"LandMap保存：{e}");
				}
			});

		public void Login(object? sender, EventArgs e) {
			Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ =>
			{

			});
		}
	}
}
