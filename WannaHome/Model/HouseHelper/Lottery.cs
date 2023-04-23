using System;
using WannaHome.Structure;

namespace WannaHome.Model.HouseHelper
{
	public class Lottery
	{
		public long Time { get; } = DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds();
		public uint ServerId { get; init; }
		/// <summary>
		/// 房区ID。0: 海雾村，1: 薰衣草苗圃，2: 高脚孤丘，3: 白银乡，4: 穹顶皓天
		/// </summary>
		public uint Area { get; init; }
		/// <summary>
		/// 小区号，从0起
		/// </summary>
		public uint Slot { get; init; }
		/// <summary>
		/// 房屋号，从1起
		/// </summary>
		public uint LandId { get; init; }
		/// <summary>
		/// 房屋状态，0: 未知，1: 可供购买，2: 结果公示阶段，3: 准备中。参见 <see cref="AvailableType"/>
		/// </summary>
		public AvailableType State { get; init; }
		public uint Participate { get; init; }
		public uint Winner { get; init; }
		/// <summary>
		/// 当前轮结束Unix时间，单位为秒
		/// </summary>
		public long EndTime { get; init; }
	}


}
