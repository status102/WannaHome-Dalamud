using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WannaHome.Model
{
	[Serializable]
	public class UploadToken
	{
		public bool enable = false;
		public ushort serverId = 0;
		public string nickname = "";
		public string url = "";
		public string token = "";

		/// <summary>
		/// 将一个房区单个小区的数据转换为base64编码字符串，记得转码
		/// </summary>
		/// <param name="data">["房主",价格]</param>
		public string Encrypt(LandInfo[] data) {
			var list = new List<List<object>>();
			data.ToList().ForEach(landInfo => list.Add(new() { landInfo.Owner, landInfo.Price }));
			var Encoder = Encoding.UTF8;
			var tokenBytes = Encoder.GetBytes(token);
			var dataBytes = Encoder.GetBytes(JsonConvert.SerializeObject(list));

			for (var i = 0; i < dataBytes.Length; i++)
				dataBytes[i] = (byte)(dataBytes[i] ^ tokenBytes[i % tokenBytes.Length]);

			return Convert.ToBase64String(dataBytes);
		}

	}
}
