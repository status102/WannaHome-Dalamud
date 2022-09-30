using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Data;
using WannaHome.Model;
using WannaHome.Model.Structure;

namespace WannaHome
{
	public class HouseWard
	{
		private readonly WannaHome WannaHome;
		private  Configuration Config => WannaHome.Configuration;
		private Dictionary<ushort, Dictionary<ushort, List<ushort>>> successList = new();
		private CancellationTokenSource? cancel;
		private object obj = new();

		public void onHousingWardInfo(HousingWardInfo? data) {

			if (data == null || !data.HasValue)
				return;
			if (data.Value.Server == 0 || data.Value.TerritoryId == 0)
				return;
			onHousingWardInfo(data.Value);
		}
		public void onHousingWardInfo(HousingWardInfo wardLandInfo) {
			List<LandInfo> landList = new();
			wardLandInfo.HouseList.ToList().ForEach(i =>
			{
				var land = new LandInfo(i);
				if (land.isEmpty && land.GetSize() >= Config.alertSize)
					WannaHome.ChatGui.Print($"{land.GetSizeStr()}：" + string.Format("{0:}{1:##}-{2:##}", Territory.TerritoriesMap[wardLandInfo.TerritoryId].nickName, wardLandInfo.WardId + 1, landList.Count + 1));

				landList.Add(land);
			});
			if (WannaHome.serverId == WannaHome.sendServerId && WannaHome.territoryId == WannaHome.sendTerritoryId && WannaHome.wardId == WannaHome.sendWardId) {
				WannaHome.sendServerId = wardLandInfo.Server;
				WannaHome.sendTerritoryId = wardLandInfo.TerritoryId;
				WannaHome.sendWardId = wardLandInfo.WardId;
			}
			WannaHome.serverId = wardLandInfo.Server;
			WannaHome.territoryId = wardLandInfo.TerritoryId;
			WannaHome.wardId = wardLandInfo.WardId;
			SaveLandList(wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.WardId, landList);
			WannaHome.SaveLandMap();

			Task.Run(() =>
			{
				Config.Token.ForEach(async token =>
				{
					if (token.serverId != wardLandInfo.Server) { } else if (!token.enable || string.IsNullOrEmpty(token.url) || string.IsNullOrEmpty(token.token)) { } else {
						var title = string.Format("<{0:} {1:}-{2:}{3:}区>", token.nickname, Server.ServerMap[wardLandInfo.Server], Territory.TerritoriesMap[wardLandInfo.TerritoryId].nickName, wardLandInfo.WardId + 1);
						try {
							var res = await API.Web.UploadWardLand(token.url, wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.WardId, token.Encrypt(landList.ToArray()), CancellationToken.None);
							if (res != null) {
								if (res.code == 200) {
									PluginLog.Debug($"{title}上传成功");
									UploadSuccess(wardLandInfo.Server, wardLandInfo.TerritoryId, (ushort)(wardLandInfo.WardId + 1));
									output();
								} else
									PluginLog.Warning($"{title}上传失败：{res.code}-{res.msg}");
							} else {
								PluginLog.Error($"{title}上传出错：返回空");
							}
						} catch (HttpRequestException e) {
							PluginLog.Error($"{title}上传出错：{e}");
						}
					}
				});
			});
		}
		public void UploadSuccess(ushort server, ushort territoryId, ushort wardIndex) {
			lock (obj) {
				if (successList.ContainsKey(server)) {
					if (successList[server].ContainsKey(territoryId)) {
						successList[server][territoryId].Add(wardIndex);
					} else {
						successList[server].Add(territoryId, new() { wardIndex });
					}
				} else {
					successList[server] = new() { { territoryId, new() { wardIndex } } };
				}
			}
		}
		public void output() => output(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(3));
		public void output(TimeSpan delay, TimeSpan outTime) {

			lock (obj) {
				if (this.cancel != null) {
					try {
						this.cancel.Cancel();
					} catch (ObjectDisposedException) { }
					this.cancel.Dispose();
					this.cancel = null;
				}

				var cancel = new CancellationTokenSource();
				cancel.Token.Register(() => { cancel = null; });
				cancel.CancelAfter(outTime);
				Task.Delay(delay, cancel.Token).ContinueWith(_ =>
				{
					cancel.Token.ThrowIfCancellationRequested();
					lock (obj) {
						pp();

						cancel.Dispose();
						cancel = null;
					}
				}, cancel.Token);
				this.cancel = cancel;
			}
		}
		public void pp() {
			var str = new StringBuilder("<");
			foreach (var server in successList) {
				str.Append(Server.ServerMap[server.Key]);
				foreach (var territory in server.Value) {
					var list = territory.Value.ToHashSet().OrderBy(i => i).ToList();
					var strList = new List<string>();
					ushort start = list[0], end = start;
					for (int i = 1; i < list.Count; i++) {
						if (list[i] == end + 1)
							end++;
						else {
							if (list[i - 1] == start)
								strList.Add(start.ToString());
							else
								strList.Add($"{start}-{list[i - 1]}");
							start = list[i];
							end = start;
						}
					}

					if (list[^1] == start)
						strList.Add(start.ToString());
					else
						strList.Add($"{start}-{list[^1]}");
					str.Append($" {Territory.TerritoriesMap[territory.Key].nickName}：{String.Join(",", strList)}");
				}
			}
			successList.Clear();
			str.Append(">上传成功");
			PluginLog.Information(str.ToString());
			WannaHome.ChatGui.Print(str.ToString());
		}

		private void SaveLandList(ushort serverId, ushort territoryId, ushort wardId, List<LandInfo> landList) {
			if (WannaHome.landMap.ContainsKey(serverId)) {
				if (WannaHome.landMap[serverId].ContainsKey(territoryId)) {
					WannaHome.landMap[serverId][territoryId][wardId] = landList;
				} else
					WannaHome.landMap[serverId][territoryId] = new() { { wardId, landList } };
			} else
				WannaHome.landMap[serverId] = new() { { territoryId, new() { { wardId, landList } } } };
		}

		#region init
		public HouseWard(WannaHome wannaHome) {
			WannaHome = wannaHome;
		}
		#endregion
	}
}
