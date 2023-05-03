using ImGuiNET;
using System;
using System.Numerics;
using WannaHome.Window;

namespace WannaHome
{
	public class PluginUI : IDisposable
	{
		private Configuration configuration;

		private bool visible = false;
		public bool Visible {
			get { return this.visible; }
			set { this.visible = value; }
		}

		private bool settingsVisible = false;
		public bool SettingsVisible {
			get { return this.settingsVisible; }
			set { this.settingsVisible = value; }
		}

		private WannaHome WannaHome { get; init; }
		public Setting Setting { get; init; }
		private LandView LandView { get; init; }

		public PluginUI(WannaHome wannaHome, Configuration configuration) {
			this.configuration = configuration;
			this.WannaHome = wannaHome;
			Setting = new(wannaHome);
			LandView = new(wannaHome);
		}

		public void Dispose() {
			Setting.Dispose();
			LandView.Dispose();
		}

		public void Draw() {
			LandView.Draw(ref visible);
			Setting.Draw(ref settingsVisible);
		}
	}
}
