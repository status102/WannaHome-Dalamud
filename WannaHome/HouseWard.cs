using Dalamud.Game.Gui;
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
using WannaHome.Structure;

namespace WannaHome
{
	public class HouseWard
	{
		private static Configuration Config => WannaHome.Instance.Config;
		private static Dictionary<ushort, Dictionary<ushort, List<ushort>>> successList = new();
		private static CancellationTokenSource? cancel;
		private static object obj = new();


		public static void onHousingWardInfo(HousingWardInfo wardLandInfo) {
			List<LandInfo> landList = new();
			wardLandInfo.HouseList.ToList().ForEach(i => {
				var land = new LandInfo(i.Name, (i.Info & HousingFlags.OwnedByFC) == HousingFlags.OwnedByFC) { Price = i.Price };
				if (land.isEmpty && land.GetSize() >= Config.AlertSize) {
					PluginLog.Warning($"{land.GetSizeStr()}数据为空：" + string.Format("{0:}{1:##}-{2:##}", Territory.TerritoriesMap[wardLandInfo.TerritoryId].NickName, wardLandInfo.SlotId + 1, landList.Count + 1));
				}
				landList.Add(land);
			});

			#region HouseHelper数据初始化
			Model.HouseHelper.Info info = new() {
				Server = wardLandInfo.Server,
				Territory = Territory.TerritoriesMap[wardLandInfo.TerritoryId].FullName,
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
					IsPersonal = (wardLandInfo.HouseList[i].Info & HousingFlags.OwnedByFC) != HousingFlags.OwnedByFC,
					IsEmpty = landList[i].isEmpty,
					IsPublic = (wardLandInfo.HouseList[i].Info & HousingFlags.VisitorsAllowed) == HousingFlags.VisitorsAllowed,
					HasGreeting = (wardLandInfo.HouseList[i].Info & HousingFlags.HasIntroduction) == HousingFlags.HasIntroduction
				});

			}
			#endregion

			
			WannaHome.Instance.SaveLandList(wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.SlotId, landList);
			WannaHome.Instance.SaveLandMap();

			// 上传到WanaHome服务器
			Task.Run(() => {
				Config.Token.Where(token => token.enable && token.serverId == wardLandInfo.Server && !string.IsNullOrEmpty(token.url) && !string.IsNullOrEmpty(token.token))
				.ToList()
				.ForEach(async token => {
					var title = string.Format("<{0:} {1:}-{2:}{3:}区>", string.Join("", token.nickname.Take(2)), Server.ServerMap[wardLandInfo.Server], Territory.TerritoriesMap[wardLandInfo.TerritoryId].NickName, wardLandInfo.SlotId + 1);
					try {
						// 房屋数据使用token加密
						var res = await API.WanaHome.UploadWardLand(token.url, wardLandInfo.Server, wardLandInfo.TerritoryId, wardLandInfo.SlotId, token.Encrypt(landList.ToArray()), CancellationToken.None);
						if (res != null) {
							if (res.code == 200) {
								PluginLog.Log($"{title}上传成功");
								UploadSuccess(wardLandInfo.Server, wardLandInfo.TerritoryId, (ushort)(wardLandInfo.SlotId + 1));
								CallPrintMsg();
							} else { Chat.PrintWarning($"{title}上传失败：{res.code}-{res.msg}"); }
						} else {
							Chat.PrintError($"{title}上传出错：返回空");
						}
					} catch (HttpRequestException e) {
						Chat.PrintError($"{title}上传出错：{e}");
					}
				});
			});

			// 上传到HouseHelper服务器
			if (Config.UploadToHouseHelper) { API.HouseHelper.CallPostHouseInfo(info, Config.HouseHelperToken); }
		}
		public static void UploadSuccess(ushort server, ushort territoryId, ushort wardIndex) {
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
		private static void CallPrintMsg() {

			lock (obj) {
				if (HouseWard.cancel != null) {
					try {
						HouseWard.cancel.Cancel();
					} catch (ObjectDisposedException) { }
					HouseWard.cancel.Dispose();
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
				HouseWard.cancel = cancel;
			}
		}
		private static void PrintMsg() {
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
					str.Append($" {Territory.TerritoriesMap[territory.Key].NickName}：{String.Join(",", strList)}");
				}
			}
			successList.Clear();
			str.Append(">(wanahome)上传成功");
			PluginLog.Information(str.ToString());
			Chat.PrintLog(str.ToString());
		}


	}
}
