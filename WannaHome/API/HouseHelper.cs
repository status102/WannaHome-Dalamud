using Dalamud.Logging;
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

		private static readonly object uploadLock = new();
		private static readonly List<Info> infoList = new();
		private static CancellationTokenSource? houseHelperCancel = null;

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

		public static void CallPostHouseInfo(Info info, string? token) {
			lock (uploadLock) {
				if (houseHelperCancel != null) {
					try {
						houseHelperCancel.Cancel();
					} catch (ObjectDisposedException) { }
					houseHelperCancel.Dispose();
					houseHelperCancel = null;
				}
				infoList.Add(info);

				var cancel = new CancellationTokenSource();
				cancel.Token.Register(() => { cancel = null; });
				cancel.CancelAfter(TimeSpan.FromMinutes(1));
				Task.Delay(TimeSpan.FromSeconds(20), cancel.Token)
					.ContinueWith(_ => {
						List<Info> list = new();
						lock (uploadLock) {
							infoList.ForEach(i => list.Add(i));
							infoList.Clear();
						}
						cancel.Token.ThrowIfCancellationRequested();
						CallPostHouseInfo(list, token);

						cancel.Dispose();
						cancel = null;
					}, cancel.Token);
				houseHelperCancel = cancel;
			}
		}
		private static void CallPostHouseInfo(List<Info> infoList, string? token) {
			Task.Run(async () => {
				try {
					var response = await PostHouseInfo(infoList, token, CancellationToken.None);
					// todo 增加上传成功提示
					if (response == null) {
						PluginLog.Warning("上传HouseHelper房区信息失败");
					} else {
						PluginLog.Log("上传房区信息请求成功：" + response);
					}
				} catch (Exception ex) {
					PluginLog.Warning("上传房区信息失败e:" + ex.ToString());
				}
			});
		}

	}
}
