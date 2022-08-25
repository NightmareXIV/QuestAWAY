using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuestAWAY.Gui
{
    internal class DevSettings
    {
        internal static void Draw()
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
    }
}
