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
		private Setting Setting { get; init; }
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
			// This is our only draw handler attached to UIBuilder, so it needs to be
			// able to draw any windows we might have open.
			// Each method checks its own visibility/state to ensure it only draws when
			// it actually makes sense.
			// There are other ways to do this, but it is generally best to keep the number of
			// draw delegates as low as possible.

			LandView.Draw(ref visible);
			Setting.Draw(ref settingsVisible);
		}
	}
}
