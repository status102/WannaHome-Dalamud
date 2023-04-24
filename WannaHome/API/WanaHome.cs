using Dalamud.Logging;
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
    public class WanaHome
    {
        private static readonly HttpClient client = new();
        static WanaHome() {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WannaHome-Dalamud", WannaHome.Version));
        }

        public static async Task<SyncNgld?> UploadWardLand(string url, ushort server, ushort territory_id, ushort ward_id, string data, CancellationToken cancellationToken) {
            var uriBuilder = new UriBuilder(url);//	($"https://home.iinformation.info/api/sync_ngld/");

            List<KeyValuePair<string?, string?>> post = new() {
                { new("server", server.ToString()) },
                { new("territory_id", territory_id.ToString()) },
                { new("ward_id", ward_id.ToString()) },
                { new("data", data) } };//{new("data",HttpUtility.UrlEncode(enc))

            var content = new FormUrlEncodedContent(post);

            cancellationToken.ThrowIfCancellationRequested();

            var res = await client.PostAsync(uriBuilder.Uri, content, cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var str = await res.Content.ReadAsStringAsync(cancellationToken);
            if (!res.IsSuccessStatusCode) {
                PluginLog.Error(uriBuilder.Uri + $"失败{res.StatusCode}：" + str + $"\nurl:{url}\nContent:{string.Join('&', post.Select(i => $"{i.Key}={i.Value}"))}");
                return null;
            }

            return JsonSerializer.Deserialize<SyncNgld>(str);
        }
    }
}
