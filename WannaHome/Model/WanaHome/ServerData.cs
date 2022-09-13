using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WannaHome.Model.WanaHome
{
	[Serializable]
	public class ServerData
	{
		public int code;
		public string msg = "";
		[JsonPropertyName("onsale")]
		public List<OnSale> onSale = new();
		public List<Change> changes = new();
		[JsonPropertyName("last_update")]
		public long lastUpdate;

		[Serializable]
		public class OnSale
		{
			public ushort server;
			[JsonPropertyName("territoryId")]
			public ushort territory_id;
			[JsonPropertyName("wardId")]
			public ushort wardId;
			[JsonPropertyName("houseId")]
			public ushort houseId;
			public int price;
			[JsonPropertyName("startSell")]
			public long startSell;
			public byte size;
			public string owner = "";
		}

		[Serializable]
		public class Change
		{
			public House house = new();
			[JsonPropertyName("event_type")]
			public string eventType = "";
			public string param1 = "";
			public string param2 = "";
			[JsonPropertyName("record_time")]
			public long recordTime;

			[Serializable]
			public class House
			{
				public ushort server;
				[JsonPropertyName("territory_id")]
				public ushort territoryId;
				[JsonPropertyName("ward_id")]
				public ushort wardId;
				[JsonPropertyName("house_id")]
				public ushort houseId;
			}
		}
	}
}
