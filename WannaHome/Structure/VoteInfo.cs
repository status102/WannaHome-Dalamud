using System.Runtime.InteropServices;

namespace WannaHome.Structure
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct VoteInfo
    {
        /// <summary>
        /// 房屋购买方式
        /// </summary>
        public PurchaseType PurchaseType;
        /// <summary>
        /// 房屋出售类型
        /// </summary>
        public TenantType TenantType;
        public AvailableType AvailableType;
        public byte Unknown1;//已出售、已公布结果是00，可摇号是01，猜测是自身是否能购买
        public uint EndTime;
        public uint Unknown2;//已出售地皮为0，摇号中地皮为第十轮结束的时间，暂时未知含义。归0
        public uint VoteCount;//
        public uint WinnerIndex;//
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Unknown3;
    }
    /// <summary>
    /// 房屋出售状态，0未知，1可摇号，2已开奖，3等待下一轮
    /// </summary>
    public enum AvailableType : byte
    {
        Unk = 0, Available = 1, LotteryResult = 2, Unavailable = 3
    }
}
