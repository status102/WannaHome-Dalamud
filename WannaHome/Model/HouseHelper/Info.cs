using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using WannaHome.Structure;

namespace WannaHome.Model.HouseHelper
{
    public class Info
    {
        [JsonPropertyName("time")]
        public long Time { get; } = DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds();
        /// <summary>
        /// 服务器id
        /// </summary>
        [JsonPropertyName("server")]
        public uint Server { get; init; }
        /// <summary>
        /// 房区名称。请使用“海雾村”、“薰衣草苗圃”、“高脚孤丘”、“白银乡”、“穹顶皓天”
        /// </summary>
        [JsonPropertyName("area")]
        public string Territory { get; init; } = string.Empty;
		/// <summary>
		/// 小区号，从0
		/// </summary>
		[JsonPropertyName("slot")]
        public uint Slot { get; init; }
		/// <summary>
		/// 1区购买类型。0: 不可购买,1: 先到先得, 2: 抽签。对应数据包偏移2408位置的数据
		/// </summary>
		[JsonPropertyName("purchase_main")]
        public PurchaseType PurchaseMain { get; init; }
		/// <summary>
		/// 扩展区购买类型. 对应数据包偏移2409位置的数据
		/// </summary>
		[JsonPropertyName("purchase_sub")]
        public PurchaseType PurchaseSub { get; init; }
		/// <summary>
		/// 非扩展区限制种类. 1: 仅限部队购买 2: 仅限个人购买。对应数据包偏移2410位置的数据
		/// </summary>
		[JsonPropertyName("region_main")]
        public TenantType RegionMain { get; init; }
		/// <summary>
		/// 扩展区限制种类. 对应数据包偏移2411位置的数据
		/// </summary>
		[JsonPropertyName("region_sub")]
        public TenantType RegionSub { get; init; }

		[JsonPropertyName("houses")]
        public IList<House> HouseList { get; init; } = new List<House>();

        public class House
        {
            /// <summary>
            /// index，从1
            /// </summary>
            [JsonPropertyName("id")]
            public uint Id { get; init; }
			/// <summary>
			/// 房主
			/// </summary>
			[JsonPropertyName("owner")]
            public string Owner { get; init; } = string.Empty;
			/// <summary>
			/// 价格
			/// </summary>
			[JsonPropertyName("price")]
            public uint Price { get; init; }
			/// <summary>
			/// S、M、L
			/// </summary>
			[JsonPropertyName("size")]
            public string Size { get; init; } = string.Empty;
			/// <summary>
			/// 设置的Tag，整数数组，不使用
			/// </summary>
			[JsonPropertyName("tags")]
            public IList<uint> Tag { get; init; } = new List<uint>();
            /// <summary>
            /// 是否为个人房，不使用
            /// </summary>
            [JsonPropertyName("isPersonal")]
            public bool IsPersonal { get; init; } = false;
            /// <summary>
            /// 是否为个人房，不使用
            /// </summary>
            [JsonPropertyName("isEmpty")]
            public bool IsEmpty { get; init; } = false;
            /// <summary>
            /// 是否访客可访问。不使用
            /// </summary>
            [JsonPropertyName("isPublic")]
            public bool IsPublic { get; init; } = false;
            /// <summary>
            /// 是否有问候语。不使用
            /// </summary>
            [JsonPropertyName("hasGreeting")]
            public bool HasGreeting { get; init; } = false;
        }
    }
}
