using Dalamud.Interface.Colors;
using ECommons;
using ECommons.ImGuiMethods;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;

namespace QuestAWAY.Gui
{
    internal class ZoneSettings
    {
        internal static uint zoneConfigId = 0;
        static bool OnlyCreated = false;
        static string Filter = "";

        internal static void Draw()
        {
            ImGuiEx.SetNextItemFullWidth();

            if (ImGui.BeginCombo("##zSelector", zoneConfigId == 0 ? "Select zone..." : TerritoryName.GetTerritoryName(zoneConfigId)))
            {
                ImGui.SetNextItemWidth(150f);
                ImGui.InputTextWithHint("##fltr", "Filter...", ref Filter, 50);
                ImGui.SameLine();
                ImGui.Checkbox("Only zones with custom settings created", ref OnlyCreated);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudViolet);

                if (Svc.ClientState.LocalPlayer != null && ImGui.Selectable($"Current: {TerritoryName.GetTerritoryName(Svc.ClientState.TerritoryType)}"))
                {
                    zoneConfigId = Svc.ClientState.TerritoryType;
                }

                ImGui.PopStyleColor();

                foreach (var x in Svc.Data.GetExcelSheet<TerritoryType>())
                {
                    var col = P.cfg.ZoneSettings.ContainsKey(x.RowId);

                    if (col)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.HealerGreen);
                    }

                    if ((Filter == string.Empty || TerritoryName.GetTerritoryName(x.RowId).Contains(Filter, StringComparison.OrdinalIgnoreCase)) && (!OnlyCreated || col) && x.PlaceName.Value.Name.ToString() != "")
                    {
                        if (ImGui.Selectable(TerritoryName.GetTerritoryName(x.RowId)))
                        {
                            zoneConfigId = x.RowId;
                        }

                        if (zoneConfigId == x.RowId && ImGui.IsWindowAppearing())
                        {
                            ImGui.SetScrollHereY();
                        }
                    }

                    if (col)
                    {
                        ImGui.PopStyleColor();
                    }
                }

                ImGui.EndCombo();
            }

            if (P.cfg.ZoneSettings.TryGetValue(zoneConfigId, out var zoneSettings))
            {
                ImGuiEx.Text(ImGuiColors.DalamudYellow, $"You are editing profile for zone {TerritoryName.GetTerritoryName(zoneConfigId)}.");
                ImGui.SameLine();

                if (ImGui.SmallButton("Delete settings") && ImGui.GetIO().KeyCtrl)
                {
                    P.cfg.ZoneSettings.Remove(zoneConfigId);
                    P.ClientState_TerritoryChanged(Svc.ClientState.TerritoryType);
                }

                ImGuiEx.Tooltip("Hold CTRL and click");

                if (zoneConfigId != Svc.ClientState.TerritoryType)
                {
                    ImGuiEx.Text(ImGuiColors.DalamudRed, "This is different zone than you are currently in.");
                }

                MainSettings.DrawProfile(zoneSettings);
            }
            else
            {
                ImGuiEx.Text($"For selected zone there are no custom settings. Global settings are used.");

                if (zoneConfigId != 0 && ImGui.Button("Create custom settings"))
                {
                    P.cfg.ZoneSettings.Add(zoneConfigId, new());
                    P.ClientState_TerritoryChanged(Svc.ClientState.TerritoryType);
                }
            }
        }
    }
}
