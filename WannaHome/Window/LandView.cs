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
using WannaHome.Model;

namespace WannaHome.Window
{
    public class LandView : IDisposable
	{
		private static readonly Vector4 alertColor = new(255f / 255, 153f / 255, 164f / 255, 1);
		private static readonly string[] Ward_Array = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24" };
		private const int Width = 600, Height = 640;
		private bool edit = false;
		private WannaHome WannaHome { get; init; }
		private Configuration Config => WannaHome.Configuration;


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

			territoryList = Data.Territory.Territories.Select(i => new KeyValuePair<ushort, string>(i.id, i.fullName)).ToList();
			territoryArray = territoryList.Select(i => i.Value).ToArray();
		}
		public void Dispose() {
		}
		#endregion

		public void Draw(ref bool visible) {
			if (!visible)
				return;
			ImGui.SetNextWindowSize(new(Width, Height));
			if (ImGui.Begin("地皮信息", ref visible)) {

				#region 上区块，选择查看缓存的地皮信息

				var serverIndex = -1;
				if (ServerMap.ContainsKey(WannaHome.serverId)) {
					for (int i = 0; i < serverList.Count; i++) {
						if (serverList[i].Key == WannaHome.serverId) {
							serverIndex = i;
							break;
						}
					}
				}
				ImGui.SetNextItemWidth(160);
				if (ImGui.Combo($"##Server", ref serverIndex, serverArray, serverArray.Length)) {
					if (serverList[serverIndex].Key != 0)
						WannaHome.serverId = serverList[serverIndex].Key;
				}

				var territoryIndex = territoryList.Select(i => i.Key).ToList().IndexOf(WannaHome.territoryId);
				ImGui.SetNextItemWidth(120);
				ImGui.SameLine();
				if (ImGui.Combo($"##Territory", ref territoryIndex, territoryArray, territoryArray.Length)) {
					WannaHome.territoryId = territoryList[territoryIndex].Key;
				}

				int wardIndex = WannaHome.wardId;
				ImGui.SetNextItemWidth(80);
				ImGui.SameLine();
				if (ImGui.Combo($"##Ward", ref wardIndex, Ward_Array, Ward_Array.Length)) {
					WannaHome.wardId = (ushort)wardIndex;
				}

				ImGui.SameLine();
				ImGui.Spacing();
				ImGui.SameLine();
				if (ImGuiComponents.IconButton(FontAwesomeIcon.Edit))
					edit = !edit;
				ImGui.SameLine();
				if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
					WannaHome.PluginUi.SettingsVisible = !WannaHome.PluginUi.SettingsVisible;


				#endregion
				List<LandInfo> landList = new();
				try {
					landList = WannaHome.landMap[WannaHome.serverId][WannaHome.territoryId][WannaHome.wardId];
				} catch (KeyNotFoundException) {
					for (int i = 0; i < 60; i++)
						landList.Add(new());
				}

				if (ImGui.BeginTabBar("##扩展区")) {
					if (ImGui.BeginTabItem("1-30号")) {
						DrawLandTable(false, landList);
						ImGui.EndTabItem();
					}
					if (ImGui.BeginTabItem("31-60号")) {
						DrawLandTable(true, landList);
						ImGui.EndTabItem();
					}
					ImGui.EndTabBar();
				}
#if DEBUG
				#region 下区块，发送地皮信息到指定服务器
				serverIndex = -1;
				if (ServerMap.ContainsKey(WannaHome.sendServerId)) {
					for (int i = 0; i < serverList.Count; i++) {
						if (serverList[i].Key == WannaHome.sendServerId) {
							serverIndex = i;
							break;
						}
					}
				}
				ImGui.SetNextItemWidth(160);
				if (ImGui.Combo($"##SendServer", ref serverIndex, serverArray, serverArray.Length)) {
					if (serverList[serverIndex].Key != 0)
						WannaHome.sendServerId = serverList[serverIndex].Key;
				}

				territoryIndex = territoryList.Select(i => i.Key).ToList().IndexOf(WannaHome.sendTerritoryId);
				ImGui.SetNextItemWidth(120);
				ImGui.SameLine();
				if (ImGui.Combo($"##SendTerritory", ref territoryIndex, territoryArray, territoryArray.Length)) {
					WannaHome.sendTerritoryId = territoryList[territoryIndex].Key;
				}

				wardIndex = WannaHome.sendWardId;
				ImGui.SetNextItemWidth(80);
				ImGui.SameLine();
				if (ImGui.Combo($"##SendWard", ref wardIndex, Ward_Array, Ward_Array.Length)) {
					WannaHome.sendWardId = (ushort)wardIndex;
				}

				ImGui.SameLine();
				if (ImGui.Button("手动上传")) {
					Config.Token.ForEach(async token =>
					{
						if (token.serverId != WannaHome.sendServerId) { } else if (!token.enable || string.IsNullOrEmpty(token.url) || string.IsNullOrEmpty(token.token)) { } else {
							var title = string.Format("<{0:} {1:}-{2:}{3:}区>手动", token.nickname, Data.Server.ServerMap[WannaHome.sendServerId], Data.Territory.TerritoriesMap[WannaHome.sendTerritoryId].nickName, WannaHome.sendWardId + 1);
							try {
								var res = await API.WanaHome.UploadWardLand(token.url, WannaHome.sendServerId, WannaHome.sendTerritoryId, WannaHome.sendWardId, token.Encrypt(landList.ToArray()), CancellationToken.None);
								if (res != null) {
									if (res.code == 200) {
										WannaHome.ChatGui.Print($"{title}上传成功");
									} else
										WannaHome.ChatGui.PrintError($"{title}上传失败：{res.code}-{res.msg}");
								} else {
									WannaHome.ChatGui.PrintError($"{title}上传出错：返回空");
								}
							} catch (HttpRequestException e) {
								WannaHome.ChatGui.PrintError($"{title}上传出错：{e}");
							}
						}
					});
				}
				#endregion
#endif
				ImGui.End();
			}
		}

		private void DrawLandTable(bool isExtra, IReadOnlyList<LandInfo> landList) {

			if (ImGui.BeginTable("##房屋信息表", 4, ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV)) {

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
					if (!landList[i].Owner.IsNullOrEmpty()) {
						ImGui.TextDisabled(landList[i].Owner);
					} else {
						if (landList[i].GetSize() >= Config.AlertSize)
							ImGui.TextColored(alertColor, $"{landList[i].Price:#,##0}");
						else
							ImGui.TextUnformatted($"{landList[i].Price:#,##0}");
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
