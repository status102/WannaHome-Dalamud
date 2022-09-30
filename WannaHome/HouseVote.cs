using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WannaHome.Model.Structure;

namespace WannaHome
{
	public class HouseVote
	{
		private WannaHome WannaHome;
		private Configuration Config => WannaHome.Configuration;
		private ushort serverId, territoryId, wardId, houseId, saleType, type;
		private uint voteCount, endTime, winnerIndex;
		private bool clientTrigger = false, voteInfo = false, available = false;
		private object obj = new();
		private readonly List<ushort> territoryList;

		public HouseVote(WannaHome wannaHome) {
			WannaHome = wannaHome;
			territoryList = Data.Territory.Territories.Select(i => i.id).ToList();
		}

		public void onClientTrigger(ClientTrigger? data) {
			if (data != null && data.HasValue)
				onClientTrigger(data.Value);
		}

		public void onClientTrigger(ClientTrigger data) {
			//03前置
			if (data.ActionId == 3 && data.HouseId == 1) {
				Clear();
			} else if (data.ActionId == 0x0452) {
				//已出售
				//52-04-00-00-81-02-00-00-3A-01-00-00-00-00-00-00-00-00-00-00-74-01-00-00-00-00-00-00-00-00-00-00
				lock (obj) {
					serverId = (ushort)(WannaHome.ClientState.LocalPlayer?.CurrentWorld.Id ?? 0);
					territoryId = data.TerritoryId;
					wardId = data.WardId;
					houseId = data.HouseId;
					clientTrigger = true;
				}
				Upload();
			} else if (data.ActionId == 0x0451) {
				//摇号中
				//51-04-00-00-D3-03-00-00-0A-13-00-00-00-00-00-00-00-00-00-00-CF-00-00-00-00-00-00-00-00-00-00-00
				lock (obj) {
					available = true;
					serverId = (ushort)(WannaHome.ClientState.LocalPlayer?.CurrentWorld.Id ?? 0);
					territoryId = data.TerritoryId;
					wardId = data.WardId;
					houseId = data.HouseId;
					clientTrigger = true;
				}
				Upload();
			}
		}

		public void onVoteInfo(VoteInfo? data) {
			if (data != null && data.HasValue)
				onVoteInfo(data.Value);
		}
		public void onVoteInfo(VoteInfo data) {
			lock (obj) {
				if (data.PurchaseType == PurchaseType.Lottery) {
					voteCount = data.VoteCount;
					winnerIndex = data.WinnerIndex;
					endTime = data.EndTime;
					saleType = (ushort)data.AvailableType;
					voteInfo = true;
					type = (ushort)(((byte)data.PurchaseType << 1) + ((byte)data.TenantType >> 1));// 1抽奖 1抢 1个人0部队
				}
			}
		}

		private void Upload() {
			lock (obj) {
				if (available || saleType > 1) {
					var serverId = this.serverId;
					var territoryId = this.territoryId;
					var wardId = this.wardId;
					var houseId = this.houseId;

					var type = this.type;
					var voteCount = this.voteCount;
					var winnerIndex = this.winnerIndex;
					var endTime = this.endTime;
					Task.Run(async () =>
					{
						string note = "";
						if (saleType == 1)
							note = "摇号中";
						else if (saleType == 2)
							note = $"已开奖({winnerIndex}号)";
						else if (saleType == 3)
							note = "准备中";

						var ter = WannaHome.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.HousingLandSet>()?.FirstOrDefault(r => r.RowId == territoryList.IndexOf(territoryId));
						byte size = 0;
						uint price = 0;
						if (ter != null) {
							size = ter.PlotSize[houseId];
							price = ter.InitialPrice[houseId];
						}
						try {
							var res = await API.Web.UpdateVoteInfo(serverId, territoryId, wardId, houseId, size, type, note, saleType, price, voteCount, winnerIndex, CancellationToken.None);
							PluginLog.Information($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>参与：{voteCount}人\n{res}");
							if (res == "null") {
								if (saleType == 1) {
									WannaHome.ChatGui.Print($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，参与：{voteCount}人");
									PluginLog.Information($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，参与：{voteCount}人");
								} else if (saleType == 2) {
									WannaHome.ChatGui.Print($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，参与：{voteCount}人，中奖：{winnerIndex}号");
									PluginLog.Information($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，参与：{voteCount}人，中奖：{winnerIndex}号");
								} else if (saleType == 3) {
									WannaHome.ChatGui.Print($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，房屋准备中");
									PluginLog.Information($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>成功，房屋准备中");
								}
							} else {
								PluginLog.Warning($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>结果不明，参与：{voteCount}人，中奖：{winnerIndex}号，到期：{endTime}\nres：${res}");
							}
						} catch (KeyNotFoundException) {
							PluginLog.Warning($"刷新过快");
						} catch (Exception e) {
							PluginLog.Warning($"上传<serverId：{serverId} territoryId：{territoryId} wardId：{wardId} houseId：{houseId}>失败：\n{e}");
						}
					});
				}
			}
		}

		private void Clear() {
			lock (obj) {
				clientTrigger = voteInfo = available = false;
				serverId = territoryId = wardId = houseId = saleType = type = 0;
				voteCount = endTime = 0;
				//note = "";
			}
		}
	}
}
