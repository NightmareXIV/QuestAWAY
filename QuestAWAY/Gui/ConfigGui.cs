using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static QuestAWAY.QuestAWAY;

namespace QuestAWAY.Gui
{
    internal class ConfigGui : Window
    {
        public ConfigGui() : base("QuestAWAY configuration")
        {
        }

        public override void OnClose()
        {
            base.OnClose();
            P.cfg.Save();
        }

        public override void PreDraw()
        {
            base.PreDraw();
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
        }

        public override void Draw()
        {
            P.reprocessAreaMap = true;
            P.reprocessNaviMap = true;
            if (ImGui.CollapsingHeader("Settings"))
            {
                ImGui.Checkbox("Plugin enabled", ref P.cfg.Enabled);
                ImGui.Checkbox("Hide icons on big map", ref P.cfg.Bigmap);
                ImGui.Checkbox("Hide icons on minimap", ref P.cfg.Minimap);
                ImGui.Checkbox("Display quick enable/disable on big map", ref P.cfg.QuickEnable);
                if (ImGui.Checkbox("Aetherytes always in front on big map", ref P.cfg.AetheryteInFront))
                    AreaMapCtrlAlwaysOn.Toggle();
                ImGui.Text("Additional pathes to hide (one per line, without _hr1 and .tex)");
                ImGui.InputTextMultiline("##QAUSERADD", ref P.cfg.CustomPathes, 1000000, new Vector2(300f, 100f));
                if (ImGui.CollapsingHeader("Developer settings"))
                {
                    ImGui.Checkbox("[dev] Enable texture collecting", ref P.collect);
                    if (P.collect)
                    {
                        if (ImGui.Button("Reset"))
                        {
                            P.texSet.Clear();
                        }
                        ImGui.SameLine();
                        ImGui.Checkbox("Display textures", ref P.collectDisplay);
                        if (P.collectDisplay)
                        {
                            foreach (var e in P.texSet)
                            {
                                ImGuiDrawImage(e);
                                ImGui.SameLine();
                                if (!Static.MapIcons.Contains(e)) ImGui.PushStyleColor(ImGuiCol.Button, 0xff0000ff);
                                if (ImGui.Button("Copy: " + e))
                                {
                                    ImGui.SetClipboardText(e);
                                }
                                if (!Static.MapIcons.Contains(e)) ImGui.PopStyleColor();
                            }
                        }
                        var s = string.Join("\n", P.texSet);
                        ImGui.InputTextMultiline("##QADATA", ref s, 1000000, new Vector2(300f, 100f));
                    }

                    ImGui.Separator();
                    if (ImGui.Button("Clear hidden textures list" + (ImGui.GetIO().KeyCtrl ? "" : " (hold ctrl and click)")) && ImGui.GetIO().KeyCtrl)
                    {
                        P.cfg.HiddenTextures.Clear();
                        P.BuildHiddenByteSet();
                    }
                    ImGui.Checkbox("Profiling", ref P.profiling);
                    if (P.profiling)
                    {
                        ImGui.Text("Total time: " + P.totalTime);
                        ImGui.Text("Total ticks: " + P.totalTicks);
                        ImGui.Text("Tick avg: " + P.totalTime / (float)P.totalTicks);
                        ImGui.Text("MS avg: " + P.totalTime / (float)P.totalTicks / Stopwatch.Frequency * 1000 + " ms");
                        if (ImGui.Button("Reset##SW"))
                        {
                            P.totalTicks = 0;
                            P.totalTime = 0;
                        }
                    }
                }
                ImGui.Separator();
            }
            ImGui.Text("Special hiding options:");
            ImGui.Checkbox("Hide fate circles", ref P.cfg.HideFateCircles);
            ImGui.Checkbox("Hide subarea markers, but keep text", ref P.cfg.HideAreaMarkers);
            ImGui.Text("Icons to hide:");
            ImGui.SameLine();
            ImGui.Checkbox("Only selected", ref P.onlySelected);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            if (ImGui.BeginCombo("##QASELOPT", "Select..."))
            {
                if (ImGui.Selectable("All"))
                {
                    P.cfg.HiddenTextures.UnionWith(Static.MapIcons);
                    P.BuildHiddenByteSet();
                }
                if (ImGui.Selectable("None"))
                {
                    P.cfg.HiddenTextures.Clear();
                    P.BuildHiddenByteSet();
                }
                ImGui.EndCombo();
            }
            ImGui.BeginChild("##QAWAYCHILD");
            ImGuiHelpers.ScaledDummy(10f);
            var width = ImGui.GetColumnWidth();
            var numColumns = Math.Max((int)(width / 100), 2);
            ImGui.Columns(numColumns);
            for (var i = 0; i < numColumns; i++)
            {
                ImGui.SetColumnWidth(i, width / numColumns);
            }
            foreach (var e in Static.MapIcons)
            {
                var b = P.cfg.HiddenTextures.Contains(e);
                if (!P.onlySelected || P.cfg.HiddenTextures.Contains(e))
                {
                    ImGui.Checkbox("##" + e, ref b);
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 11);
                    ImGuiDrawImage(e + "_hr1");
                    if (ImGui.IsItemHovered() && ImGui.GetMouseDragDelta() == Vector2.Zero)
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        if (Static.MapIconNames[e].Length > 0 || ImGui.GetIO().KeyCtrl)
                        {
                            ImGui.SetTooltip(Static.MapIconNames[e].Length > 0 ? Static.MapIconNames[e] : e);
                        }
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Right))
                        {
                            ImGui.SetClipboardText(e);
                        }
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        {
                            b = !b;
                        }
                    }

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 11);
                    ImGui.NextColumn();
                    if (P.cfg.HiddenTextures.Contains(e) && !b)
                    {
                        P.cfg.HiddenTextures.Remove(e);
                        P.BuildHiddenByteSet();
                    }
                    if (!P.cfg.HiddenTextures.Contains(e) && b)
                    {
                        P.cfg.HiddenTextures.Add(e);
                        P.BuildHiddenByteSet();
                    }
                }
            }
            ImGui.Columns(1);
            ImGui.EndChild();
        }

        void ImGuiDrawImage(string partialPath)
        {
            try
            {
                if (!P.textures.ContainsKey(partialPath))
                {
                    P.textures[partialPath] = Svc.Data.GetImGuiTexture(partialPath + ".tex");
                }
                ImGui.Image(P.textures[partialPath].ImGuiHandle, Vector2Scale, Vector2.Zero, Vector2.One, Vector4.One);
            }
            catch (Exception ex)
            {
                Svc.Chat.Print("[QuestAWAY error] " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
