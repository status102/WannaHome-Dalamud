using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using WannaHome.Model;

namespace WannaHome
{
	[Serializable]
	public class Configuration : IPluginConfiguration
	{
		public int Version { get; set; } = 0;

		public bool Debug = false;
		public bool SavePackage = false;
		public string GameVersion = "";
		public ushort ClientTriggerOpCode = 0x0192;
		public ushort VoteInfoOpCode = 429;
		public ushort DebugOpCode = 429;
		public byte alertSize = 1;
		// the below exist just to make saving less cumbersome

		public List<UploadToken> Token = new();

		[NonSerialized]
		private DalamudPluginInterface? pluginInterface;
		[NonSerialized]
		public readonly static Dictionary<ushort, string> defaultToken = new()
		{
			{ 1043, "cdtuK99IlknlnMgPJ9VeFix7nCm8YK6M" },
			{ 1045, "B8FI5ALivhIXwCSNHC25WuY1BRf0VRZl" },
			{ 1106, "4gUfgUUVC1kGg0OOTyOBqR7HEceupMpa" },
			{ 1169, "ppb021EBlfkQRtfPzTJItQ9EcsANd8sc" },
			{ 1177, "tHz81ovPRdsggUPcCwTD04lOoN9DrbhL" },
			{ 1178, "Nv9aLwKyt4bFz2ucPitjfJwrk6XGm6TP" },
			{ 1179, "kUoOABsK3Gqw4NLxumbW6abPBrvyIhNP" },
			{ 1183, "d43f195502aed43f195502aed43f1955" },
			{ 1192, "aeca889f8415aeca889f8415aeca889f" },
			{ 1180, "b36dd5c6d5e24e0ba9c2f6499e7fbf80" },
			{ 1186, "2719346835c34b158470146e61c4ec6b" },
			{ 1201, "772d9f36f771424582e3eb550963ad16" }
		};

		#region init
		public void Initialize(DalamudPluginInterface pluginInterface) {
			this.pluginInterface = pluginInterface;
		}

		public void Save() {
			this.pluginInterface!.SavePluginConfig(this);
		}
		#endregion
	}
}
