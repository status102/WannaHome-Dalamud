using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Common;
using WannaHome.Data;
using WannaHome.Model;
using WannaHome.Model.HouseHelper;
using WannaHome.Structure;

namespace WannaHome
{
	public class HouseWard
	{
		private readonly WannaHome WannaHome;
		private Configuration Config => WannaHome.Configuration;
		private Dictionary<ushort, Dictionary<ushort, List<ushort>>> successList = new();
		private CancellationTokenSource? cancel;
		private object obj = new();

		private object houseHelperLock = new();
		private List<Model.HouseHelper.Info> infoList = new();
		private CancellationTokenSource? houseHelperCancel = null;

		public void onHousingWardInfo(HousingWardInfo wardLandInfo) {
			List<LandInfo> landList = new();
			wardLandInfo.HouseList.ToList().ForEach(i => {
				var land = new LandInfo(i.Name, (i.Info & 0b1_0000) == 0b1_0000) { Price = i.Price };
				if (land.isEmpty && land.GetSize() >= Config.AlertSize) {
					Service.ChatGui.Print($"[{WannaHome.Plugin_Name}]{land.GetSizeStr()}：" + string.Format("{0:}{1:##}-{2:##}", Territory.TerritoriesMap[wardLandInfo.TerritoryId].nickName, wardLandInfo.SlotId + 1, landList.Count + 1));
				}
				landList.Add(land);
			});

			#region HouseHelper数据初始化
			Model.HouseHelper.Info info = new() {
				Server = wardLandInfo.Server,
				Territory = Territory.TerritoriesMap[wardLandInfo.TerritoryId].fullName,
				Slot = wardLandInfo.SlotId,
				PurchaseMain = wardLandInfo.PurchaseMain,
				PurchaseSub = wardLandInfo.PurchaseSub,
				RegionMain = wardLandInfo.RegionMain,
				RegionSub = wardLandInfo.RegionSub
			};
			for (int i = 0; i < landList.Count; i++) {
				info.HouseList.Add(new() {
					Id = (uint)(i + 1),
					Owner = landList[i].Owner,
					Price = landList[i].Price,
					Size = landList[i].GetSizeStr(),
					Tag = wardLandInfo.HouseList[i].Tag.Select(i => (uint)i).ToList(),
					IsPersonal = (wardLandInfo.HouseList[i].Info & 0b1_0000) != 0b1_0000,
					IsEmpty = landList[i].isEmpty,
					IsPublic = (wardLandInfo.HouseList[i].Info & 0b10) == 0b10,
					HasGreeting = (wardLandInfo.HouseList[i].Info & 0b100) == 0b100
				});

			}
			#endregion

			if (WannaHome.serverId == WannaHome.sendServerId && WannaHome.territoryId == WannaHome.sendTerritoryId && WannaHome.wardId == WannaHome.sendWardId) {
				WannaHome.sendServerId = wardLandInfo.Server;
				WannaHome.sendTerritoryId = wardLandInfo.TerritoryId;
				WannaHome.sendWardId = wardLandInfo.SlotId;
			}
			WannaHome.serverId = wardLandInfo.Server;
			WannaHome.territoryId = wardLandInfo.TerritoryId;
			WannaHome.wardId = wardLandInfo.SlotId;
			SaveLandList(wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.SlotId, landList);
			WannaHome.SaveLandMap();

			// 上传到WanaHome服务器
			Task.Run(() => {
				Config.Token.Where(token => token.enable && token.serverId == wardLandInfo.Server && !string.IsNullOrEmpty(token.url) && !string.IsNullOrEmpty(token.token))
				.ToList()
				.ForEach(async token => {
					var title = string.Format("[{0:}]<{1:} {2:}-{3:}{4:}区>", WannaHome.Plugin_Name, string.Join("", token.nickname.Take(2)), Server.ServerMap[wardLandInfo.Server], Territory.TerritoriesMap[wardLandInfo.TerritoryId].nickName, wardLandInfo.SlotId + 1);
					try {
						// 房屋数据使用token加密
						var res = await API.WanaHome.UploadWardLand(token.url, wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.SlotId, token.Encrypt(landList.ToArray()), CancellationToken.None);
						if (res != null) {
							if (res.code == 200) {
								PluginLog.Debug($"{title}上传成功");
								UploadSuccess(wardLandInfo.Server, wardLandInfo.TerritoryId, (ushort)(wardLandInfo.SlotId + 1));
								CallPrintMsg();
							} else
								PluginLog.Warning($"{title}上传失败：{res.code}-{res.msg}");
						} else {
							PluginLog.Error($"{title}上传出错：返回空");
						}
					} catch (HttpRequestException e) {
						PluginLog.Error($"{title}上传出错：{e}");
					}
				});
			});

			// 上传到HouseHelper服务器
			// todo 增加上传返回的输出
			if (Config.UploadToHouseHelper) {
				CallUploadHouseHelper(info, Config.HouseHelperToken);
			}
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
		private void CallPrintMsg() {

			lock (obj) {
				if (this.cancel != null) {
					try {
						this.cancel.Cancel();
					} catch (ObjectDisposedException) { }
					this.cancel.Dispose();
				}

				var cancel = new CancellationTokenSource();
				cancel.Token.Register(() => { cancel = null; });
				cancel.CancelAfter(TimeSpan.FromMinutes(1));
				Task.Delay(TimeSpan.FromSeconds(30), cancel.Token)
					.ContinueWith(_ => {
						cancel.Token.ThrowIfCancellationRequested();
						lock (obj) {
							PrintMsg();

							cancel.Dispose();
							cancel = null;
						}
					}, cancel.Token);
				this.cancel = cancel;
			}
		}
		private void PrintMsg() {
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

					if (list[^1] == start) {
						strList.Add(start.ToString());
					} else {
						strList.Add($"{start}-{list[^1]}");
					}
					str.Append($" {Territory.TerritoriesMap[territory.Key].nickName}：{String.Join(",", strList)}");
				}
			}
			successList.Clear();
			str.Append(">上传成功");
			PluginLog.Information(str.ToString());
			Service.ChatGui.Print(str.ToString());
		}

		private void CallUploadHouseHelper(Model.HouseHelper.Info info, string? token) {
			lock (houseHelperLock) {
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
						lock (houseHelperLock) {
							infoList.ForEach(i => list.Add(i));
							infoList.Clear();
						}
						cancel.Token.ThrowIfCancellationRequested();
						UploadHouseHelper(list, token);

						cancel.Dispose();
						cancel = null;
					}, cancel.Token);
				houseHelperCancel = cancel;
			}
		}

		private static void UploadHouseHelper(List<Info> infoList, string? token) {
			Task.Run(async () => {
				try {
					var response = await API.HouseHelper.PostHouseInfo(infoList, token, CancellationToken.None);

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
