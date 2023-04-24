﻿using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WannaHome.Common;
using WannaHome.Model;

namespace WannaHome
{
	public sealed class WannaHome : IDalamudPlugin
	{
		public static WannaHome? Instance { get; private set; }
		public const string Plugin_Name = "WannaHome";
		public string Name => "Wanna Home";
		public static string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

		private const string commandName = "/wh";

		public DalamudPluginInterface PluginInterface { get; init; }
		public PluginUI PluginUi { get; init; }
		public Configuration Configuration { get; init; }
		public Calculate Calculate { get; init; }

		/// <summary>
		/// 服务器ID-房区ID-房区序号-List
		/// </summary>
		public Dictionary<ushort, Dictionary<ushort, Dictionary<ushort, List<LandInfo>>>> landMap = new();

		public ushort serverId = 0, territoryId = 0, wardId = 0;
		public ushort sendServerId, sendTerritoryId, sendWardId;

		public WannaHome(
			[RequiredVersion("1.0")] DalamudPluginInterface pluginInterface
		) {
			Instance = this;
			PluginInterface = pluginInterface;
			Service.Initialize(pluginInterface);


			Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
			Configuration.Initialize(this.PluginInterface);

			PluginUi = new PluginUI(this, Configuration);
			Calculate = new Calculate(this);

			Service.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
				HelpMessage = "打开主界面；\n/wh cfg打开设置界面"
			});

			PluginInterface.UiBuilder.Draw += DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
			Service.ClientState.Login += Login;
			LoadLandMap();
		}

		public void Dispose() {
			SaveLandMap().Wait();
			Calculate.Dispose();
			PluginUi.Dispose();
			Service.CommandManager.RemoveHandler(commandName);
			Service.ClientState.Login -= Login;
			PluginInterface.UiBuilder.Draw -= DrawUI;
			PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
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
			Task.Run(async () => {
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
			Task.Run(() => {
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
			Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => {

			});
		}
	}
}
