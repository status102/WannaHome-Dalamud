using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Model.HouseHelper;

namespace WannaHome.API
{
	public class HouseHelper
	{
		private const string HOUSEHELPER_BASE = "https://househelper.ffxiv.cyou/api";
		private const string DEFAUTL_TOKEN = "SqxR10eeVGNJpKVTLIzUgffNHoLM4ggq";
		private static readonly HttpClient client = new();
		static HouseHelper() {
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WannaHome-Dalamud", WannaHome.Version));
		}

		public static async Task<string> PostHouseInfo(List<Info> houseInfo, string? token, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder(HOUSEHELPER_BASE + "/info");
			client.DefaultRequestHeaders.Authorization = new("Token", string.IsNullOrEmpty(token) ? DEFAUTL_TOKEN : token);

			var content = new StringContent(JsonSerializer.Serialize(houseInfo));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var res = await client.PostAsync(uriBuilder.Uri, content, cancellationToken).ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			var parsedRes = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			if (parsedRes == null) { throw new HttpRequestException("HouseHelper returned null response"); }
			return parsedRes;
		}

		public static async Task<string> PostLottery(List<Lottery> lotteryData, string? token, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder(HOUSEHELPER_BASE + "/lottery");
			client.DefaultRequestHeaders.Authorization = new("Token", string.IsNullOrEmpty(token) ? DEFAUTL_TOKEN : token);

			var content = new StringContent(JsonSerializer.Serialize(lotteryData));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var res = await client.PostAsync(uriBuilder.Uri, content, cancellationToken).ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			var parsedRes = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			if (parsedRes == null) { throw new HttpRequestException("HouseHelper returned null response"); }
			return parsedRes;
		}

	}
}
