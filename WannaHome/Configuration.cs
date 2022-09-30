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
		public readonly static Dictionary<string, string> defaultToken = new()
		{
			{"拉诺西亚","LhWirar9n7Ept8Dpah8T0pLKbM3ms1Pb"},
			{"幻影群岛","bj4iec0jhB8pKtZwuiiwxHi9kiVqOzZq"},
{"萌芽池","h9wIXwmvVCQW9lKAZyS92rbM5yxIhhRS"},
{"神意之地","yC1hkJDGzcSHllyHGbty5zFuECbXeUEe"},
{"红玉海","3GgnWCfc0ha4V5U1rNyQt9E6njQYrqOA"},
{"宇宙和音","obPlwWTfLKO5TCwMjOyztMdorvPNTpLv"},
{"沃仙曦染","tqfssQXXSpxY5ufPSOuZTRd25Zofg83j"},
{"晨曦王座","5hgmvxHdM8r8oVUkTqrMeK3k0h2lNBps"},
{"白金幻象","3x9VIqxZ1doRwnp06Z50QhvOSmZaojRz"},
{"旅人栈桥","h5em372x9bEOFyqsQotwijhiRmyFfiRd"},
{"拂晓之间","rOqSZY7YF8meO6lkKQPKKbj2ygEHIHO1"},
{"龙巢神殿","9SDRtFZVPO2OQObLoDmpRBKAYVCy34Hn"},
{"潮风亭","s8CbyMUfJD0HF0GUkk0prsPo8o47Ezkn"},
{"神拳痕","8qY4syelBXWHb4EYVgPLNPGVeWlA2yQ4"},
{"白银乡","46Z3mQbiyUht1bp3XqJdJeB63JZK7e2N"},
{"梦羽宝境","J4YzlEhPBwlCsWxGRBdS87FUtTnjBM41"},
{"紫水栈桥","o7gUzb3zmaOhBBKativwNTb6WeTDkltj"},
{"摩杜纳","g0xP7tHu4hTIEE0TFHs9Uwua6yKZM8q9"},
{"静语庄园","CtuWJ01T3ierhtpoxEiC8zl2rzPIbc2c"},
{"延夏","1hvXKDPxHpmwQFXWqKoZi3P49non4qmU"},
{"海猫茶屋","9uwx6iaxYSE2asjkjB72M6SxOdybnlgH"},
{"柔风海湾","IqqmunhzshNJYTnDiXLVVLNwTD0OfX4F"},
{"琥珀原","9JcJ50izWXNqNqdOmNoc7RSyrTJW1hCF"},
{"水晶塔","5I6S1qbcBnOLRA0baPlt6bxsbQPFeEcR"},
{"银泪湖","7RHrH04dQLP5jEyi8Qd7XvK10iWsDRa9"},
{"太阳海岸","AkPixkHJyHgRTFYBYpKH2uU7dk0ycv51"},
{"伊修加德","iFobA2Wr8Y70lVIGuP5AqkRYjr2vsnBp"},
{"红茶川","MbS5KbNdkQYg5tvBwtoDdmOfQ5qRuTBm"},

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
