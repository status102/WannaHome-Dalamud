using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using WannaHome.Common;
using WannaHome.Model;

namespace WannaHome.Window
{
	public class LandView : IDisposable
	{
		private static readonly Vector4 ALERT_COLOR = new(255f / 255, 153f / 255, 164f / 255, 1);
		private static readonly string[] WARD_ARRAY = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24" };
		private const int WIDTH = 600, HEIGHT = 640;
		private bool edit = false;
		private WannaHome WannaHome { get; init; }
		private Configuration Config => WannaHome.Config;


		public IReadOnlyDictionary<ushort, string> ServerMap => Data.Server.ServerMap;
		private List<KeyValuePair<ushort, string>> serverList, territoryList;
		private string[] serverArray, territoryArray;

		#region init
		public LandView(WannaHome wannaHome) {
			WannaHome = wannaHome;

			serverList = new();
			serverList.Add(new(0, $"==={Data.Server.LuXingNiao.Dc_Name}==="));
			serverList.AddRange(Data.Server.LuXingNiao.Dc_World.Select(i => new KeyValuePair<ushort, string>(i.Key, $"　　{i.Value}")));
			serverList.Add(new(0, $"==={Data.Server.MoGuLi.Dc_Name}==="));
			serverList.AddRange(Data.Server.MoGuLi.Dc_World.Select(i => new KeyValuePair<ushort, string>(i.Key, $"　　{i.Value}")));
			serverList.Add(new(0, $"==={Data.Server.MaoXiaoPang.Dc_Name}==="));
			serverList.AddRange(Data.Server.MaoXiaoPang.Dc_World.Select(i => new KeyValuePair<ushort, string>(i.Key, $"　　{i.Value}")));
			serverList.Add(new(0, $"==={Data.Server.DouDouChai.Dc_Name}==="));
			serverList.AddRange(Data.Server.DouDouChai.Dc_World.Select(i => new KeyValuePair<ushort, string>(i.Key, $"　　{i.Value}")));

			serverArray = serverList.Select(i => i.Value).ToArray();

			territoryList = Data.Territory.Territories.Select(i => new KeyValuePair<ushort, string>(i.Id, i.FullName)).ToList();
			territoryArray = territoryList.Select(i => i.Value).ToArray();
		}
		public void Dispose() {
		}
		#endregion

		public void Draw(ref bool visible) {
			if (!visible) { return; }
			ImGui.SetNextWindowSize(new(WIDTH, HEIGHT));
			if (ImGui.Begin("地皮信息", ref visible)) {

				List<LandInfo> landList = new();
				try {
					landList = WannaHome.landMap[WannaHome.ServerId][WannaHome.TerritoryId][WannaHome.WardId];
				} catch (KeyNotFoundException) {
					// 如果未录入
					for (int i = 0; i < 60; i++) { landList.Add(new()); }
				}

				#region 上区块，选择查看缓存的地皮信息
				var serverIndex = -1;
				if (ServerMap.ContainsKey(WannaHome.ServerId)) {
					for (int i = 0; i < serverList.Count; i++) {
						if (serverList[i].Key == WannaHome.ServerId) {
							serverIndex = i;
							break;
						}
					}
				}
				ImGui.SetNextItemWidth(160);
				if (ImGui.Combo($"##Server", ref serverIndex, serverArray, serverArray.Length)) {
					if (serverList[serverIndex].Key != 0) {
						WannaHome.ServerId = serverList[serverIndex].Key;
					}
				}

				var territoryIndex = territoryList.Select(i => i.Key).ToList().IndexOf(WannaHome.TerritoryId);
				ImGui.SetNextItemWidth(120);
				ImGui.SameLine();
				if (ImGui.Combo($"##Territory", ref territoryIndex, territoryArray, territoryArray.Length)) {
					WannaHome.TerritoryId = territoryList[territoryIndex].Key;
				}

				int wardIndex = WannaHome.WardId;
				ImGui.SetNextItemWidth(80);
				ImGui.SameLine();
				if (ImGui.Combo($"##Ward", ref wardIndex, WARD_ARRAY, WARD_ARRAY.Length)) {
					WannaHome.WardId = (ushort)wardIndex;
				}

				ImGui.SameLine();
				ImGui.Spacing();
				ImGui.SameLine();
				if (ImGuiComponents.IconButton(FontAwesomeIcon.Edit))
					edit = !edit;
				ImGui.SameLine();
				if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
					WannaHome.PluginUi.SettingsVisible = !WannaHome.PluginUi.SettingsVisible;

				#region 手动发送地皮信息到指定服务器
#if DEBUG

				ImGui.SameLine();
				if (ImGui.Button("手动上传")) {
					Config.Token.Where(i => i.enable && i.serverId == WannaHome.ServerId).ToList()
						.ForEach(async token => {
							if (string.IsNullOrEmpty(token.url) || string.IsNullOrEmpty(token.token)) {
							} else {
								var title = string.Format("<{0:} {1:}-{2:}{3:}区>手动", token.nickname, Data.Server.ServerMap[WannaHome.ServerId], Data.Territory.TerritoriesMap[WannaHome.TerritoryId].NickName, WannaHome.WardId + 1);
								try {
									var res = await API.WanaHome.UploadWardLand(token.url, WannaHome.ServerId, WannaHome.TerritoryId, WannaHome.WardId, token.Encrypt(landList.ToArray()), CancellationToken.None);
									if (res != null) {
										if (res.code == 200) {
											Chat.PrintLog($"{title}上传成功");
										} else
											Chat.PrintError($"{title}上传失败：{res.code}-{res.msg}");
									} else {
										Chat.PrintError($"{title}上传出错：返回空");
									}
								} catch (HttpRequestException e) {
									Chat.PrintError($"{title}上传出错：{e}");
								}
							}
						});
				}
#endif
				#endregion

				#endregion

				if (ImGui.BeginTabBar("##扩展区")) {
					ImGui.SetNextItemWidth(WIDTH / 2);
					if (ImGui.BeginTabItem("1-30号")) {
						DrawLandTable(false, landList);
						ImGui.EndTabItem();
					}
					ImGui.SetNextItemWidth(WIDTH / 2);
					if (ImGui.BeginTabItem("31-60号")) {
						DrawLandTable(true, landList);
						ImGui.EndTabItem();
					}
					ImGui.EndTabBar();
				}

				ImGui.End();
			}
		}

		private void DrawLandTable(bool isExtra, IReadOnlyList<LandInfo> landList) {

			if (ImGui.BeginTable("##房屋信息表", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV)) {

				ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 20);
				ImGui.TableSetupColumn("房主/价格", ImGuiTableColumnFlags.WidthStretch);
				ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 20);
				ImGui.TableSetupColumn("房主/价格", ImGuiTableColumnFlags.WidthStretch);

				ImGui.TableHeadersRow();
				for (int i = 0; i < 15; i++) {
					ImGui.TableNextRow();
					DrawLandText((isExtra ? 30 : 0) + i, landList);
					DrawLandText((isExtra ? 30 : 0) + i + 15, landList);
				}
				ImGui.EndTable();
			}

		}
		private void DrawLandText(int i, IReadOnlyList<LandInfo> landList) {
			ImGui.TableNextColumn();
			ImGui.TextUnformatted($"{i + 1}");
			ImGui.TableNextColumn();

			if (i < landList.Count) {
				if (!edit) {
					if (!landList[i].IsOnSell()) {
						ImGui.TextDisabled("不可出售");
					} else if (!landList[i].Owner.IsNullOrEmpty()) {
						// 正常输出
						ImGui.TextDisabled(landList[i].Owner);
					} else if (landList[i].Price > 0 && landList[i].GetSize() >= Config.AlertSize)
						ImGui.TextColored(ALERT_COLOR, $"{landList[i].Price:#,##0}");
					else if (landList[i].Price > 0) {
						ImGui.TextUnformatted($"{landList[i].Price:#,##0}");
					} else {
						//考虑增加一下未录入时候输出默认价格
						ImGui.TextDisabled($"----");
					}

				} else {
					var editStr = $"{landList[i].Owner}/{landList[i].Price:#,##0}";
					if (ImGui.InputText($"##{i}-{editStr}", ref editStr, 256)) {
						string[] array;
						if ((array = editStr.Split('/')).Length == 2) {
							landList[i].Owner = array[0];
							landList[i].name = Array.Empty<byte>();
							try {
								landList[i].Price = uint.Parse(array[1].Replace(",", ""));
							} catch (FormatException) { }
						}
					}
				}
			}
		}
	}
}
