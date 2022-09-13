using System.Runtime.InteropServices;

namespace WannaHome.Model.Structure
{

	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct ClientTrigger
	{
		public ushort ActionId;//03用于清空残留数据 0x0451 摇号中 0x0452已出售
		public ushort Unknown1;
		public ushort TerritoryId;
		public ushort Unknown2;
		public byte HouseId;
		public byte WardId;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
		public ushort[] Empty;//可能会有东西，但是用途未知
	}
}
