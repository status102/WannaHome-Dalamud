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
		//public bool settingShow = false;
		private WannaHome WannaHome { get; init; }
		private Configuration Config => WannaHome.Configuration;
		private static IReadOnlyDictionary<ushort, string> ServerMap => Data.Server.ServerMap;

		private List<string> serverList { get; init; }

		public Setting(WannaHome wannaHome) {
			this.WannaHome = wannaHome;
			serverList = new();
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
			if (!settingShow)
				return;
			if (ImGui.Begin("设置", ref settingShow)) {

				if (ImGui.CollapsingHeader("其他设置", ImGuiTreeNodeFlags.DefaultOpen)) {
					if (ImGui.Checkbox("Debug", ref Config.Debug)) {
						Config.Save();
					}
					if (Config.Debug) {
						if (ImGui.Checkbox("SavePackage", ref Config.SavePackage)) {
							Config.Save();
						}
						int op = Config.DebugOpCode;
						if (ImGui.InputInt("Debug OpCode", ref op)) {
							Config.DebugOpCode = (ushort)op;
							Config.Save();
						}
						ImGui.TextUnformatted($"OpCode Version：{Config.GameVersion}");
						ImGui.TextUnformatted($"ClientTrigger OpCode：{Config.ClientTriggerOpCode}");
						ImGui.TextUnformatted($"VoteInfoOpCode OpCode：{Config.VoteInfoOpCode}");
					}
					int index = Config.alertSize;
					if (ImGui.Combo("空地提醒等级", ref index, size, 3)) {
						Config.alertSize = (byte)index;
						Config.Save();
					}
					if (WannaHome.Calculate.captureOpCode) {
						ImGui.TextUnformatted("正在手动刷新OpCode中");
					} else {
						if (ImGui.Button("刷新OpCode")) {
							WannaHome.Calculate.CaptureOpCode();
							Task.Run(() =>
							{
								Thread.Sleep(30000);
								WannaHome.Calculate.CaptureCancel();
							});
						}
						if (ImGui.IsItemHovered())
							ImGui.SetTooltip("点击后30秒内查看门牌信息手动刷新OpCode");
					}
				}
				if (ImGui.CollapsingHeader("服务器Token")) {
					int count = 0;
					Config.Token.ToList().ForEach((pair) =>
					{
						var index = count++;

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
						if (ImGuiComponents.IconButton(index, FontAwesomeIcon.Trash)) {
							Config.Token.RemoveAt(index);
							Config.Save();
						}
					});
					if (ImGuiComponents.IconButton(-2, FontAwesomeIcon.Plus)) {
						Config.Token.Add(new());
					}
					ImGui.SameLine();
					if (ImGuiComponents.IconButton(-3, FontAwesomeIcon.PlusCircle)) {
						foreach (var pair in Configuration.defaultToken) {
							Config.Token.Add(new() { enable = true, serverId = pair.Key, nickname = "猹", url = "https://home.iinformation.info/api/sync_ngld/", token = pair.Value });
						}
					}
				}
				ImGui.End();
			}
		}

		public void Dispose() { }
	}
}
