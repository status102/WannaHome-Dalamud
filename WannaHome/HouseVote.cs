using Dalamud.Logging;
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
		private ushort serverId, territoryId, wardId, houseId, isSell, type;
		private uint voteCount, endTime, winnerIndex;
		private bool clientTrigger = false, voteInfo = false,available = false;
		private object obj = new();
		private string note = "";
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
			if (data.ActionId == 3) {
				Clear();
			} else if (data.ActionId == 0x0452) {
				//已出售
				//52-04-00-00-81-02-00-00-3A-01-00-00-00-00-00-00-00-00-00-00-74-01-00-00-00-00-00-00-00-00-00-00
				lock (obj) {
					serverId = (ushort)(WannaHome.ClientState.LocalPlayer?.HomeWorld.Id ?? 0);
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
					serverId = (ushort)(WannaHome.ClientState.LocalPlayer?.HomeWorld.Id ?? 0);
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
					isSell = (ushort)data.AvailableType;
					voteInfo = true;
					type = (ushort)((ushort)data.PurchaseType + ((ushort)data.TenantType << 1));
				}
			}
		}

		private void Upload() {
			lock (obj) {
				if (available || isSell > 1) {
					Task.Run(async () =>
					{
						if (isSell == 1)
							note = "摇号中";
						else if (isSell == 2)
							note = $"已开奖({winnerIndex}号)";
						else if (isSell == 3)
							note = "准备中";

						var ter = WannaHome.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.HousingLandSet>()?.FirstOrDefault(r => r.RowId == territoryList.IndexOf(territoryId));
						byte size = 0;
						uint price = 0;
						if (ter != null) {
							size = ter.PlotSize[houseId];
							price = ter.InitialPrice[houseId];
						}
						var res = await API.Web.UpdateVoteInfo(serverId, territoryId, wardId, houseId, size, type, note, isSell, price, voteCount, winnerIndex, CancellationToken.None);
						PluginLog.Information($"上传<{Data.Server.ServerMap[serverId]} {Data.Territory.TerritoriesMap[territoryId].nickName}{wardId + 1}-{houseId + 1}>参与：{voteCount}人\n{res}");
					});
				}
			}
		}

		private void Clear() {
			lock (obj) {
				clientTrigger = voteInfo = available= false;
				serverId = territoryId = wardId = houseId = isSell = type = 0;
				voteCount = endTime = 0;
				note = "";
			}
		}
	}
}
