using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WannaHome.Window
{
	public class Setting : IDisposable
	{
		private readonly static string[] size = new string[] { "全部", "M & L", "L" };

		private WannaHome WannaHome { get; init; }
		private Configuration Config => WannaHome.Configuration;
		private static IReadOnlyDictionary<ushort, string> ServerMap => Data.Server.ServerMap;
		private List<string> serverList { get; } = new();

		public Setting(WannaHome wannaHome) {
			this.WannaHome = wannaHome;
			serverList.Add($"==={Data.Server.LuXingNiao.Dc_Name}===");
			serverList.AddRange(Data.Server.LuXingNiao.Dc_World.Values);
			serverList.Add($"==={Data.Server.MoGuLi.Dc_Name}===");
			serverList.AddRange(Data.Server.MoGuLi.Dc_World.Values);
			serverList.Add($"==={Data.Server.MaoXiaoPang.Dc_Name}===");
			serverList.AddRange(Data.Server.MaoXiaoPang.Dc_World.Values);
			serverList.Add($"==={Data.Server.DouDouChai.Dc_Name}===");
			serverList.AddRange(Data.Server.DouDouChai.Dc_World.Values);
		}

		public void Draw(ref bool settingShow) {
			if (!settingShow) { return; }
			if (ImGui.Begin("设置", ref settingShow)) {

				if (ImGui.CollapsingHeader("其他设置", ImGuiTreeNodeFlags.DefaultOpen)) {
					if (ImGui.Checkbox("Debug", ref Config.Debug)) { Config.Save(); }
					if (Config.Debug) {
						if (ImGui.Checkbox("SavePackage", ref Config.SavePackage)) { Config.Save(); }
						int op = Config.DebugOpcode;
						if (ImGui.InputInt("Debug Opcode", ref op)) {
							Config.DebugOpcode = (ushort)op;
							Config.Save();
						}
						ImGui.TextUnformatted($"Opcode Version：{Config.GameVersion}");
						ImGui.TextUnformatted($"ClientTrigger Opcode：{Config.ClientTriggerOpcode}");
						ImGui.TextUnformatted($"VoteInfoOpcode Opcode：{Config.VoteInfoOpcode}");
					}
					int index = Config.AlertSize;
					if (ImGui.Combo("空地提醒等级", ref index, size, 3)) {
						Config.AlertSize = (byte)index;
						Config.Save();
					}
					if (WannaHome.Calculate.captureOpcode) {
						ImGui.TextUnformatted("正在手动刷新opcode中");
					} else {
						if (ImGui.Button("刷新opcode")) {
							WannaHome.Calculate.CaptureOpcode();
							Task.Run(() => {
								Thread.Sleep(30000);
								WannaHome.Calculate.CaptureCancel();
							});
						}
						if (ImGui.IsItemHovered()) { ImGui.SetTooltip("点击后30秒内查看门牌信息手动刷新opcode"); }
					}
				}
				if (ImGui.CollapsingHeader("Wanahome上传Token")) {
					int count = 0;
					Config.Token.ToList().ForEach((pair) => {
						var index = count++;

						ImGui.PushID(index);
						var enable = pair.enable;
						if (ImGui.Checkbox($"##{index}Enable", ref enable)) {
							Config.Token[index].enable = enable;
							Config.Save();
						}

						ImGui.SameLine();
						ImGui.SetNextItemWidth(160);
						var array = serverList.ToArray();
						var arrayIndex = ServerMap.ContainsKey(pair.serverId) ? serverList.IndexOf(ServerMap[pair.serverId]) : -1;
						if (ImGui.Combo($"##{index}Key", ref arrayIndex, array, array.Length)) {
							try {
								var select = Data.Server.ServerMap.First(i => i.Value == array[arrayIndex].Trim());

								Config.Token[index].serverId = select.Key;
								Config.Save();
							} catch (InvalidOperationException) { }
						}
						var nickname = pair.nickname;
						ImGui.SameLine();
						ImGui.SetNextItemWidth(120);
						if (ImGui.InputText($"##{index}Name", ref nickname, 1024)) {
							Config.Token[index].nickname = nickname;
							Config.Save();
						}
						var url = pair.url;
						ImGui.SameLine();
						ImGui.SetNextItemWidth(300);
						if (ImGui.InputText($"##{index}Url", ref url, 1024)) {
							Config.Token[index].url = url;
							Config.Save();
						}
						var token = pair.token;
						ImGui.SameLine();
						ImGui.SetNextItemWidth(400);
						if (ImGui.InputText($"##{index}Token", ref token, 1024)) {
							Config.Token[index].token = token;
							Config.Save();
						}

						ImGui.SameLine();
						ImGui.PushFont(UiBuilder.IconFont);
						if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString() + $"##{index}")) {
							Config.Token.RemoveAt(index);
							Config.Save();
						}
						ImGui.PopFont();
					});
					ImGui.PopID();
					#region 添加token按钮
					if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus)) {
						Config.Token.Add(new());
					}
					ImGui.SameLine();
					if (ImGuiComponents.IconButton(FontAwesomeIcon.PlusCircle)) {
						Dictionary<string, ushort> map = new();
						foreach (var pair in Data.Server.ServerMap) {
							map.Add(pair.Value, pair.Key);
						}
						foreach (var pair in Configuration.defaultToken) {
							if (map.ContainsKey(pair.Key))
								Config.Token.Add(new() { enable = true, serverId = map[pair.Key], nickname = "冰音", url = "https://wanahome.ffxiv.bingyin.org/api/sync_ngld/", token = pair.Value });
						}
						Config.Save();
					}
					if (ImGui.IsItemHovered())
						ImGui.SetTooltip("添加内置的冰音Token");
					#endregion
				}

				if (ImGui.CollapsingHeader("HouseHelper上传Token")) {
					bool upload = Config.UploadToHouseHelper;
					if(ImGui.Checkbox("上传到HouseHelper", ref upload)) {
						Config.UploadToHouseHelper = upload;
						Config.Save();
					}
					ImGui.SameLine();
					ImGui.SetNextItemWidth(600);
					string token = Config.HouseHelperToken;
					if (ImGui.InputTextWithHint("", "请输入你的token，未输入时自动使用内置的默认token", ref token, 100)) {
						Config.HouseHelperToken = token.Trim();
						Config.Save();
					}
				}
				ImGui.End();
			}
		}

		public void Dispose() { }
	}
}
