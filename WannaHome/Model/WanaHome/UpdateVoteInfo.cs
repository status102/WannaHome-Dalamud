using WannaHome.Structure;

namespace WannaHome.Model.WanaHome
{
	public class UploadVoteInfo
	{
		public ushort server { get; init; }
		public ushort territory { get; init; }
		public ushort ward { get; init; }
		public ushort housenumber { get; init; }
		public byte size { get; init; }
		public ushort type { get; init; }
		public string owner { get; init; } = "";
		public AvailableType sell { get; init; }
		public uint price { get; init; }
		public uint votecount { get; init; }
		public uint winner { get; init; }
		public string plugin_version { get; } = WannaHome.Version;
	}
}
