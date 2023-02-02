﻿using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Model.WanaHome;

namespace WannaHome.API
{
	public class Web
	{
		public static async Task<string> UpdateVoteInfo(ushort serverId, ushort territoryId, ushort wardId, ushort houseId, byte size, ushort type, string owner, ushort isShell, uint price, uint voteCount, uint winnerIndex, uint? uploaderHomeServerId, ulong uploaderId, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder("https://home-api.iinformation.info/v2/update/");

			UploadVoteInfo voteInfo = new()
			{
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
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Wanna_Home-Dalamud", UploadVoteInfo.plugin_version));
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
		}
		public static async Task<SyncNgld?> UploadWardLand(string url, ushort server, ushort territory_id, ushort ward_id, string data, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder(url);//	($"https://home.iinformation.info/api/sync_ngld/");

			List<KeyValuePair<string?, string?>> post = new() {
				{ new("server", server.ToString()) },
				{ new("territory_id", territory_id.ToString()) },
				{ new("ward_id", ward_id.ToString()) },
				{ new("data", data) } };//{new("data",HttpUtility.UrlEncode(enc))

			var content = new FormUrlEncodedContent(post);
			//content.Headers.Add("user-agent", $"{WannaHome.Instance?.Name} {UploadVoteInfo.plugin_version}");

			cancellationToken.ThrowIfCancellationRequested();

			using var client = new HttpClient();
			var ua = client.DefaultRequestHeaders.UserAgent;
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Wanna_Home-Dalamud", UploadVoteInfo.plugin_version));
			

			var res = await client
			  .PostAsync(uriBuilder.Uri, content, cancellationToken)
			  .ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();

			var str = await res.Content.ReadAsStringAsync(cancellationToken);
			if (!res.IsSuccessStatusCode) {
				PluginLog.Error(uriBuilder.Uri + $"失败{res.StatusCode}：" + str + $"\nurl:{url}\nContent:{string.Join('&', post.Select(i => $"{i.Key}={i.Value}"))}");
				return null;
			}

			return JsonSerializer.Deserialize<SyncNgld>(str);
		}

		public static async Task<ServerData?> GetServerData(ushort server, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder($"https://home-api.iinformation.info/data/{server}");

			cancellationToken.ThrowIfCancellationRequested();

			using var client = new HttpClient();
			var res = await client
			  .GetAsync(uriBuilder.Uri, cancellationToken)
			  .ConfigureAwait(false);

			cancellationToken.ThrowIfCancellationRequested();
			var str = await res.Content.ReadAsStringAsync(cancellationToken);
			if (!res.IsSuccessStatusCode) {
				PluginLog.Error(uriBuilder.Uri + $"失败{res.StatusCode}：" + str + $"\nurl:{res.RequestMessage?.RequestUri}\nContent:{res.RequestMessage?.Content}");
				return null;
			}
			/*
			var parsedRes = await JsonSerializer
			  .DeserializeAsync<ServerData>(res.Content.ReadAsStream(cancellationToken), cancellationToken: cancellationToken)
			  .ConfigureAwait(false);*/

			return JsonSerializer.Deserialize<ServerData>(str);
		}
	}
}
