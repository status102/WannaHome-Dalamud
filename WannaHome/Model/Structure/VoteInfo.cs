﻿using System.Runtime.InteropServices;

namespace WannaHome.Model.Structure
{
	[StructLayout(LayoutKind.Sequential, Size = 0x20)]
	public struct VoteInfo
	{
		public PurchaseType PurchaseType;
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
	public enum PurchaseType : byte
	{
		Unavailable = 0, FCFS = 1, Lottery = 2
	}
	public enum TenantType : byte
	{
		FreeCompany = 1, Person = 2
	}
	public enum AvailableType : byte
	{
		Available = 1, LotteryResult = 2, Unavailable = 3
	}
}
