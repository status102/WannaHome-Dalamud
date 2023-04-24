using Dalamud.Logging;
using System;
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

		private static readonly HttpClient client = new();
		static Web() {
			client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WannaHome-Dalamud", WannaHome.Version));
		}
		public static async Task<string> UpdateVoteInfo(UploadVoteInfo voteInfo, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder("https://home-api.iinformation.info/v2/update/");

			var content_string = JsonSerializer.Serialize(voteInfo);
			var content = new StringContent(content_string);
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			cancellationToken.ThrowIfCancellationRequested();

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

		public static async Task<ServerData?> GetServerData(ushort server, CancellationToken cancellationToken) {
			var uriBuilder = new UriBuilder($"https://home-api.iinformation.info/data/{server}");

			cancellationToken.ThrowIfCancellationRequested();

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
