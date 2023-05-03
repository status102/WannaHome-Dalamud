using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Common;
using WannaHome.Data;
using WannaHome.Structure;

namespace WannaHome
{
	public class HouseVote
	{
		private WannaHome WannaHome { get; init; }
		private static Configuration Config => WannaHome.Instance.Config;
		private static ushort serverId, territoryId, wardId, houseId, type;
		private static AvailableType saleType;
		private static uint voteCount, endTime, winnerIndex;
		private static bool clientTrigger = false, voteInfo = false, available = false;
		private static object obj = new();
		private static readonly List<ushort> territoryList = Territory.Territories.Select(i => i.Id).ToList();

		public HouseVote(WannaHome wannaHome) {
			WannaHome = wannaHome;
		}


		public static void onClientTrigger(ClientTrigger data) {
			//03前置
			if (data.ActionId == 3 && data.HouseId == 1) {
				Clear();
			} else if (data.ActionId == 0x0452) {
				//已出售
				//52-04-00-00-81-02-00-00-3A-01-00-00-00-00-00-00-00-00-00-00-74-01-00-00-00-00-00-00-00-00-00-00
				lock (obj) {
					serverId = (ushort)(Service.ClientState.LocalPlayer?.CurrentWorld.Id ?? 0);
					territoryId = data.TerritoryId;
					wardId = data.WardId;
					houseId = data.HouseId;
					//clientTrigger = true;
				}
				Upload();
			} else if (data.ActionId == 0x0451) {
				//摇号中
				//51-04-00-00-D3-03-00-00-0A-13-00-00-00-00-00-00-00-00-00-00-CF-00-00-00-00-00-00-00-00-00-00-00
				lock (obj) {
					available = true;
					serverId = (ushort)(Service.ClientState.LocalPlayer?.CurrentWorld.Id ?? 0);
					territoryId = data.TerritoryId;
					wardId = data.WardId;
					houseId = data.HouseId;
					clientTrigger = true;
					PluginLog.Debug("摇号中");
				}
				Upload();
			}
		}

		public static void onVoteInfo(VoteInfo data) {
			lock (obj) {
				if (data.PurchaseType == PurchaseType.Lottery) {
					voteCount = data.VoteCount;
					winnerIndex = data.WinnerIndex;
					endTime = data.EndTime;
					saleType = data.AvailableType;
					voteInfo = true;
					type = (ushort)(((byte)data.PurchaseType << 1) + ((byte)data.TenantType >> 1));// 1抽奖 1抢 1个人0部队
					PluginLog.Debug($"VoteCount：{data.VoteCount}，winner：{data.WinnerIndex}，endTime：{data.EndTime}，available：{(ushort)data.AvailableType}，PurchaseType：{(ushort)data.PurchaseType}，TenantType：{(ushort)data.TenantType}");
				}
			}
		}
		/// <summary>
		/// 上传房屋信息
		/// </summary>
		private static void Upload() {
			lock (obj) {
				if (voteInfo && ((saleType == AvailableType.LotteryResult && winnerIndex > 0) || clientTrigger)) {
					PluginLog.Debug($"上传数据：voteInfo：{voteInfo}，clientTrigger：{clientTrigger}，saleType：{saleType}");

					Task.Run(async () => {
						string owner = "";
						if (saleType == AvailableType.Available) {
							owner = "摇号中";
						} else if (saleType == AvailableType.LotteryResult) {
							owner = $"已开奖({winnerIndex}号)";
						} else if (saleType == AvailableType.Unavailable) {
							owner = "准备中";
						}

						var territory = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.HousingLandSet>()?.FirstOrDefault(r => r.RowId == territoryList.IndexOf(territoryId));
						byte size = 0;
						uint price = 0;
						if (territory != null) {
							size = territory.PlotSize[houseId];
							price = territory.InitialPrice[houseId];
						}
						try {
							Model.HouseHelper.Lottery lottery = new() {
								ServerId = serverId,
								Area = (uint)Territory.TerritoriesMap.Keys.OrderBy(x => x).ToList().IndexOf(territoryId),
								Slot = wardId,
								LandId = (uint)(houseId + 1),
								State = saleType,
								Participate = voteCount,
								Winner = winnerIndex,
								EndTime = endTime
							};
							var prefix = $"<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].NickName}{wardId + 1}-{houseId + 1}>";
							var response = await API.HouseHelper.PostLottery(new() { lottery }, Config.HouseHelperToken, CancellationToken.None);
							if (response == null) {
								PluginLog.Warning(prefix + $"请求失败，参与：{voteCount}人，中奖：{winnerIndex}号，到期：{endTime}\nres：${response}");
							} else {
								PluginLog.Log(prefix + "请求成功：" + response);
							}
						} catch (Exception e) {
							PluginLog.Warning($"上传<serverId：{serverId} territoryId：{territoryId} wardId：{wardId} houseId：{houseId}>失败：\nException：{e}");
						}
						try {
							Model.WanaHome.UploadVoteInfo info = new() {
								server = serverId,
								territory = territoryId,
								ward = wardId,
								housenumber = houseId,
								size = size,
								type = type,
								owner = owner,
								sell = saleType,
								price = price,
								votecount = voteCount,
								winner = winnerIndex
							};
							var res = await API.Web.UpdateVoteInfo(info, CancellationToken.None);
							var prefix = $"<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].NickName}{wardId + 1}-{houseId + 1}>";
							PluginLog.Debug(prefix + $"请求返回：\n{res}");
							if (res == "null") {
								PluginLog.Log(prefix + $"上传成功");
							} else {
								PluginLog.Warning(prefix + $"请求失败，参与：{voteCount}人，中奖：{winnerIndex}号，到期：{endTime}\nres：${res}");
							}
						} catch (KeyNotFoundException) {
							PluginLog.Warning($"刷新过快");
						} catch (Exception e) {
							PluginLog.Warning($"上传<serverId：{serverId} territoryId：{territoryId} wardId：{wardId} houseId：{houseId}>失败：\nException：{e}");
						}
					});
				}
			}
		}

		private static void Clear() {
			lock (obj) {
				clientTrigger = voteInfo = available = false;
				serverId = territoryId = wardId = houseId = type = 0;
				saleType = 0;
				voteCount = endTime = 0;
			}
		}
	}
}
