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

		public static async Task<string> PostHouseInfo(List<Info> houseInfo, string? token, CancellationToken cancellationToken) {
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Authorization", "Token " + (string.IsNullOrEmpty(token) ? DEFAUTL_TOKEN : token));

			var content = new StringContent(JsonSerializer.Serialize(houseInfo));
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var res = await client.PostAsync(new UriBuilder(HOUSEHELPER_BASE + "/info").Uri, content, cancellationToken).ConfigureAwait(false);
			cancellationToken.ThrowIfCancellationRequested();
			var parsedRes = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			if (parsedRes == null) { throw new HttpRequestException("HouseHelper returned null response"); }
			return parsedRes;
		}

		public static async Task<string> PostLottery(List<Lottery> lotteryData, string? token, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder(HOUSEHELPER_BASE + "/lottery");

			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(string.IsNullOrEmpty(token) ? DEFAUTL_TOKEN : token);

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
