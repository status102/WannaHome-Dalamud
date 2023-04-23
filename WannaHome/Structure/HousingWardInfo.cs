using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace WannaHome.Structure
{
    [StructLayout(LayoutKind.Sequential, Size = 2412)]
    public struct HousingWardInfo
    {
        public ushort LandId;
        /// <summary>
        /// 小区号
        /// </summary>
        public ushort SlotId;
        /// <summary>
        /// 房区id
        /// </summary>
        public ushort TerritoryId;
        public ushort Server;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public HouseSimple[] HouseList;
		/// <summary>
		/// 1区购买类型。0: 不可购买,1: 先到先得, 2: 抽签。对应数据包偏移2408位置的数据
		/// </summary>
		public PurchaseType PurchaseMain;
		/// <summary>
		/// 扩展区购买类型. 对应数据包偏移2409位置的数据
		/// </summary>
		public PurchaseType PurchaseSub;
		/// <summary>
		/// 非扩展区限制种类. 1: 仅限部队购买 2: 仅限个人购买。对应数据包偏移2410位置的数据
		/// </summary>
		public TenantType RegionMain;
		/// <summary>
		/// 扩展区限制种类. 对应数据包偏移2411位置的数据
		/// </summary>
		public TenantType RegionSub;
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
	/// <summary>
	/// 房屋购买方式，0不可用，1抢，2摇号
	/// </summary>
	public enum PurchaseType : byte
	{
		Unavailable = 0, FCFS = 1, Lottery = 2
	}
	/// <summary>
	/// 房屋出售类型，1部队房，2个人房
	/// </summary>
	public enum TenantType : byte
	{
		FreeCompany = 1, Person = 2
	}
}
