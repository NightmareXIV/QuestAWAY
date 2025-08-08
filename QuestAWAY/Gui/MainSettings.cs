using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using ECommons.ImGuiMethods;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Numerics;

namespace QuestAWAY.Gui
{
    internal class MainSettings
    {
        internal static void Draw()
        {
            ImGuiEx.Text(ImGuiColors.DalamudYellow, "You are editing global profile.");

            if (P.cfg.ZoneSettings.ContainsKey(Svc.ClientState.TerritoryType))
            {
                ImGuiEx.Text(ImGuiColors.DalamudRed, "There are custom settings for current zone. Global settings are not effective.");
            }

            DrawProfile(P.cfg);
        }

        internal static void DrawProfile(Configuration config)
        {
            if (ImGui.Button("Copy settings to clipboard"))
            {
                Safe(delegate
                {
                    var copy = JsonConvert.DeserializeObject<Configuration>(JsonConvert.SerializeObject(config));
                    copy.ZoneSettings = null;
                    ImGui.SetClipboardText(JsonConvert.SerializeObject(copy));
                });
            }

            ImGui.SameLine();

            if (ImGui.Button("Paste settings from clipboard") && ImGui.GetIO().KeyCtrl)
            {
                Safe(delegate
                {
                    var imp = JsonConvert.DeserializeObject<Configuration>(ImGui.GetClipboardText());
                    config.QuickEnable = imp.QuickEnable;
                    config.Enabled = imp.Enabled;
                    config.AetheryteInFront = imp.AetheryteInFront;
                    config.Bigmap = imp.Bigmap;
                    config.Minimap = imp.Minimap;
                    config.CustomPathes = imp.CustomPathes;
                    config.HideAreaMarkers = imp.HideAreaMarkers;
                    config.HideFateCircles = imp.HideFateCircles;
                    config.HiddenTextures = imp.HiddenTextures;
                    QuestAWAY.ApplyMemoryReplacer();
                });
            }

            ImGuiEx.Tooltip("Hold CTRL and click button");

            if (ImGui.Checkbox("Plugin enabled", ref config.Enabled))
            {
                QuestAWAY.ApplyMemoryReplacer();
            }

            ImGui.Checkbox("Hide icons on big map", ref config.Bigmap);
            ImGui.Checkbox("Hide icons on minimap", ref config.Minimap);
            ImGui.Checkbox("Display quick enable/disable on big map", ref config.QuickEnable);

            if(ImGui.Checkbox("Aetherytes always in front on big map (invert Ctrl key for Map)", ref config.AetheryteInFront))
            {
                QuestAWAY.ApplyMemoryReplacer();
            }

            ImGui.Text("Additional pathes to hide (one per line, without _hr1 and .tex)");
            ImGui.InputTextMultiline("##QAUSERADD", ref config.CustomPathes, 1000000, new Vector2(300f, 100f));
            ImGui.Text("Special hiding options:");
            ImGui.Checkbox("Hide fate circles", ref config.HideFateCircles);
            ImGui.Checkbox("Hide subarea markers, but keep text", ref config.HideAreaMarkers);
            ImGui.Text("Category:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200f);

            if (ImGui.BeginCombo("##QASELCAT", Static.CategoryNames[P.selectedCategory]))
            {
                foreach (var cat in Static.CategoryNames)
                {
                    if (ImGui.Selectable(cat.Value, P.selectedCategory == cat.Key))
                    {
                        P.selectedCategory = cat.Key;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Text("Quick select:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);

            if (ImGui.BeginCombo("##QASELOPT", "Select..."))
            {
                if (ImGui.Selectable("All"))
                {
                    if (P.selectedCategory == Category.All)
                    {
                        config.HiddenTextures.UnionWith(Static.MapIcons);
                    }
                    else
                    {
                        config.HiddenTextures.UnionWith(Static.MapIcons.Where(e => Static.MapIconData[e].Category == P.selectedCategory));
                    }

                    P.BuildHiddenByteSet();
                }

                if (ImGui.Selectable("None"))
                {
                    if (P.selectedCategory == Category.All)
                    {
                        config.HiddenTextures.Clear();
                    }
                    else
                    {
                        config.HiddenTextures.ExceptWith(Static.MapIcons.Where(e => Static.MapIconData[e].Category == P.selectedCategory));
                    }

                    P.BuildHiddenByteSet();
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Checkbox("Show only selected", ref P.onlySelected);

            //ImGui.BeginChild("##QAWAYCHILD");
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
                var b = config.HiddenTextures.Contains(e);

                if ((P.selectedCategory == Category.All || P.selectedCategory == Static.MapIconData[e].Category) && (!P.onlySelected || config.HiddenTextures.Contains(e)))
                {
                    ImGui.Checkbox("##" + e, ref b);
                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 11);
                    ImGuiDrawImage(e + "_hr1");

                    if (ImGui.IsItemHovered() && ImGui.GetMouseDragDelta() == Vector2.Zero)
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

                        if (Static.MapIconData[e].Name.Length > 0 || ImGui.GetIO().KeyCtrl)
                        {
                            ImGui.SetTooltip(Static.MapIconData[e].Name.Length > 0 ? Static.MapIconData[e].Name : e);
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

                    if (config.HiddenTextures.Contains(e) && !b)
                    {
                        config.HiddenTextures.Remove(e);

                        P.BuildHiddenByteSet();
                    }

                    if (!config.HiddenTextures.Contains(e) && b)
                    {
                        config.HiddenTextures.Add(e);

                        P.BuildHiddenByteSet();
                    }
                }
            }

            ImGui.Columns(1);
            //ImGui.EndChild();
        }
    }
}
