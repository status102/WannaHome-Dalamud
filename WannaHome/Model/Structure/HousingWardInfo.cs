using System.Runtime.InteropServices;

namespace WannaHome.Model.Structure
{
	[StructLayout(LayoutKind.Sequential, Size = 2408)]
	public struct HousingWardInfo
	{
		public ushort LandId;
		/// <summary>
		/// 小区号
		/// </summary>
		public ushort WardId;
		/// <summary>
		/// 房区id
		/// </summary>
		public ushort TerritoryId;
		public ushort Server;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
		public HouseSimple[] HouseList;

	}
	[StructLayout(LayoutKind.Sequential, Size = 40)]
	public struct HouseSimple
	{
		public uint Price;

		/// <summary>
		/// ()()()(1部队)()(1有房屋问候语)(1开门)()
		/// </summary>
		//伊修加德未开发房区：0x10
		//白银乡的出售中房区：0x00
		public byte Info;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
		public byte[] Tag;
		/// <summary>
		/// UTF-8编码名称
		/// </summary>
		//伊修加德未开发房区：00-35-02-00-00-00-00-93-65-2C-02-00-00-00-00-00-F4-D3-64-F3-7F-00-00-10-96-2E-04-00-00-00-00-00-
		//白银乡的出售中房区则是全0
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] Name;

	}
}
