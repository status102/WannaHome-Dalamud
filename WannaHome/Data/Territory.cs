using System.Collections.Generic;
using System.Linq;

namespace WannaHome.Data
{
	public class Territory
	{
		public static List<Territory> Territories = new()
		{
			new (){id = 339, fullName = "海雾村", nickName = "海"},
			new (){id = 340, fullName = "薰衣草苗园", nickName = "森"},
			new (){id = 341, fullName = "高脚孤丘", nickName = "沙"},
			new (){id = 641, fullName = "白银乡", nickName = "白"},
			new (){id = 979, fullName = "穹顶皓天", nickName = "天", sell = false}
		};
		public static Dictionary<ushort, Territory> TerritoriesMap => Territories.ToDictionary(i => i.id);
		public ushort id { get; init; }
		public string fullName { get; init; } = "";
		public string nickName { get; init; } = "";
		public bool sell { get; init; } = true;
	}
}
