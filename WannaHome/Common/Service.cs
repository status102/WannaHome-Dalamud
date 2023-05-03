using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace WannaHome.Common
{
    public class Service
    {
        public static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();

        [PluginService][RequiredVersion("1.0")] internal static ObjectTable ObjectTable { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static CommandManager Commands { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static DataManager DataManager { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static ChatGui ChatGui { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static ClientState ClientState { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static CommandManager CommandManager { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static GameGui GameGui { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static GameNetwork GameNetwork { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] internal static SigScanner SigScanner { get; private set; } = null!;

    }
}
