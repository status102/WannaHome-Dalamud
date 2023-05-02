using System.Collections.Generic;
using System.Linq;

namespace WannaHome.Data
{
	public class Territory
	{
		public static List<Territory> Territories = new()
		{
			new (){Id = 339, FullName = "海雾村", NickName = "海"},
			new (){Id = 340, FullName = "薰衣草苗圃", NickName = "森"},
			new (){Id = 341, FullName = "高脚孤丘", NickName = "沙"},
			new (){Id = 641, FullName = "白银乡", NickName = "白"},
			new (){Id = 979, FullName = "穹顶皓天", NickName = "天"}// , sell = false
		};
		public static Dictionary<ushort, Territory> TerritoriesMap => Territories.ToDictionary(i => i.Id);
		public ushort Id { get; init; }
		public string FullName { get; init; } = "";
		public string NickName { get; init; } = "";
		public bool OnSell { get; init; } = true;
	}
}
