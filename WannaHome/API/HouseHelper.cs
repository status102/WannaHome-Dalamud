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
		private const string DEFAUTL_TOKEN = "1EkZWwXYZybA9ekZLAqAhCNJ7zsvLeHO";

		public static async Task<string> PostHouseInfo(List<Info> houseInfo, string? token, CancellationToken cancellationToken) {
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(string.IsNullOrEmpty(token) ? DEFAUTL_TOKEN : token);

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


		/*
		public static async Task<CurrentlyShownView> GetMarketData(string worldName, uint itemId, CancellationToken cancellationToken, int listCount = 1, int historyCount = 0) {
			var uriBuilder = new UriBuilder($"https://universalis.app/api/v2/{worldName}/{itemId}?listings={listCount}&entries={historyCount}");

			cancellationToken.ThrowIfCancellationRequested();

			var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
			var res = await client
			  //.SendAsync(new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri),cancellationToken)
			  //.GetAsync(uriBuilder.Uri, cancellationToken)
			  .PostAsync(uriBuilder.Uri, cancellationToken)
			  .ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();

			var parsedRes = await JsonSerializer.DeserializeAsync<CurrentlyShownView>(res, cancellationToken: cancellationToken).ConfigureAwait(false);

			if (parsedRes == null) { throw new HttpRequestException("Universalis returned null response"); }

			return parsedRes;
		}
		public static async Task<string> UpdateVoteInfo(ushort serverId, ushort territoryId, ushort wardId, ushort houseId, byte size, ushort type, string owner, ushort isShell, uint price, uint voteCount, uint winnerIndex, uint? uploaderHomeServerId, ulong uploaderId, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder("https://home-api.iinformation.info/v2/update/");

			UploadVoteInfo voteInfo = new() {
				server = serverId,
				territory = territoryId,
				ward = wardId,
				housenumber = houseId,
				size = size,
				type = type,
				owner = owner,
				sell = isShell,
				price = price,
				votecount = voteCount,
				winner = winnerIndex
			};
			var content_string = JsonSerializer.Serialize(voteInfo);
			string _serverName = uploaderHomeServerId?.ToString() ?? "unknown";
			string _playerName = uploaderId.ToString();
			var content = new StringContent(content_string);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			//content.Headers.Add("user-agent", $"{_serverName}-{_playerName}/{WannaHome.Instance?.Name} {UploadVoteInfo.plugin_version}");

			cancellationToken.ThrowIfCancellationRequested();

			using var client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Wanna_Home-Dalamud", WannaHome.Version));
			//client.DefaultRequestHeaders.Accept.Add().Add("Content-Type", "application/json");
			var res = await client
			  .PostAsync(uriBuilder.Uri, content, cancellationToken)
			  .ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();

			var str = await res.Content.ReadAsStringAsync(cancellationToken);
			if (!res.IsSuccessStatusCode) {
				PluginLog.Error(uriBuilder.Uri + $"，请求失败\nHTTP Status Code：{(int)res.StatusCode}-{res.StatusCode}；\nContent:{content_string}；\nResponse：" + str);
				return "";
			}

			return str;
		}*/
	}
}
