using System;
using System.Linq;
using System.Text;

namespace WannaHome.Model
{
	[Serializable]
	public class LandInfo
	{
		private readonly static byte[] LAND_SHOW = new byte[] { 0x00, 0x35, 0x02, 0x00, 0x00, 0x00, 0x00, 0x93, 0x65, 0x2C, 0x02 };
		private bool onsell = true;
		public byte[] name { get; set; } = new byte[32];
		public string Owner { get; set; } = "";
		public uint Price { get; set; }
		public LandInfo() { }

		public LandInfo(byte[] name, bool isFreeCompany = false) {
			if (Enumerable.SequenceEqual(LAND_SHOW, name)) {
				Owner = "不可出售";
				onsell = false;
			} else {
				Owner = name[0] != 0 ? Encoding.UTF8.GetString(name).TrimEnd('\u0000') : "";
			}
			if (isFreeCompany && !string.IsNullOrEmpty(Owner)) { Owner = $"《{Owner}》"; }
		}

		/// <summary>
		/// 根据价格计算地皮类型 0-S 1-M 2-L
		/// </summary>
		public byte GetSize() {
			if (Price <= 3750000) { return 0; }
			if (Price <= 20000000) { return 1; }
			return 2;
		}
		public string GetSizeStr() {
			if (Price <= 3750000) { return "S"; }
			if (Price <= 20000000) { return "M"; }
			return "L";
		}
		public bool IsOnSell() { return onsell; }
		[Newtonsoft.Json.JsonIgnore]
		public bool isEmpty => string.IsNullOrEmpty(Owner);

	}
}
