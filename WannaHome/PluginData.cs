using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WannaHome.Model;

namespace WannaHome
{
	[Serializable]
	public class PluginData
	{
		public ushort serverId { get; set; }
		public ushort territoryId { get; set; }
		public ushort wardId { get; set; }
		public Dictionary<ushort, Dictionary<ushort, Dictionary<ushort, List<LandInfo>>>> LandInfo { get; set; } = new();
	}
}
