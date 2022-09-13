using System;
using System.Text;
using WannaHome.Model.Structure;

namespace WannaHome.Model
{
	[Serializable]
	public class LandInfo
	{
		private readonly static byte[] Ward_Show = new byte[] { 0x00, 0x35, 0x02, 0x00, 0x00, 0x00, 0x00, 0x93, 0x65, 0x2C, 0x02 };

		public byte[] name { get; set; } = new byte[32];
		public string hostName { get; set; } = "";
		public uint price { get; set; }
		public LandInfo() { }
		public LandInfo(HouseSimple houseSimple) {
			hostName = Encoding.UTF8.GetString(houseSimple.Name).TrimEnd('\u0000');
			price = houseSimple.Price;
			Array.Copy(houseSimple.Name, name, 32);
			if (isEmpty)
				hostName = "";
			else if (hostName.Length != 0) {
				if ((houseSimple.Info & 0b1_0000) == 0b1_0000)
					hostName = $"《{hostName}》";
				if (isOnShow)
					hostName = "不可购买";
			}
		}

		/// <summary>
		/// 根据价格计算地皮类型 0-S 1-M 2-L
		/// </summary>
		public byte GetSize() {
			if (price <= 3750000)
				return 0;
			if (price <= 20000000)
				return 1;
			return 2;
		}
		public char GetSizeStr() {
			if (price <= 3750000)
				return 'S';
			if (price <= 20000000)
				return 'M';
			return 'L';
		}
		[Newtonsoft.Json.JsonIgnore]
		public bool isOnShow {
			get {
				for (int i = 0; i < Ward_Show.Length; i++)
					if (name[i] != Ward_Show[i])
						return false;
				return true;
			}
		}
		[Newtonsoft.Json.JsonIgnore]
		public bool isEmpty => name[0] == 0;
			
	}
}
