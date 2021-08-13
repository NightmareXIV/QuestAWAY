using Dalamud;
using Dalamud.Game.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuestAWAY
{
    unsafe class QuestAWAY : IDalamudPlugin
    {
        public string Name => "QuestAWAY";
        internal DalamudPluginInterface pi;
        bool open = false;
        bool collect = false;
        bool collectDisplay = false;
        HashSet<string> texSet = new HashSet<string>();
        HashSet<string> userDefinedTextureSet = new HashSet<string>();
        internal Vector2 quickMenuPos = new Vector2(0f, 0f);
        internal Vector2 quickMenuSize = Vector2.Zero;
        Dictionary<string, TextureWrap> textures;
        Configuration cfg;
        static Vector2 Vector2Scale = new Vector2(48f, 48f);
        private bool onlySelected = false;
        private bool openQuickEnable = false;
        byte[][] cfgHideSet = { };
        bool reprocess = false;
        long tick = 0;
        bool profiling = false;
        long totalTime;
        long totalTicks;
        Stopwatch stopwatch;

        public void Dispose()
        {
            pi.Framework.OnUpdateEvent -= Tick;
            pi.UiBuilder.OnBuildUi -= Draw;
            cfg.Save();
            foreach(var t in textures.Values)
            {
                t.Dispose();
            }
            pi.Dispose();
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            pi = pluginInterface;
            textures = new Dictionary<string, TextureWrap>();
            cfg = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(this);
            BuildByteSet();
            Static.PaddingVector = ImGui.GetStyle().WindowPadding;
            pi.Framework.OnUpdateEvent += Tick;
            pi.UiBuilder.OnBuildUi += Draw;
            pi.UiBuilder.OnOpenConfigUi += delegate { open = true; };
            stopwatch = new Stopwatch();
        }

        void ImGuiDrawImage(string partialPath)
        {
            try
            {
                if (!textures.ContainsKey(partialPath))
                {
                    textures[partialPath] = pi.Data.GetImGuiTexture(partialPath + ".tex");
                }
                ImGui.Image(textures[partialPath].ImGuiHandle, Vector2Scale, Vector2.Zero, Vector2.One, Vector4.One);
            }
            catch (Exception ex)
            {
                pi.Framework.Gui.Chat.Print("[QuestAWAY error] " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void Draw()
        {
            if (openQuickEnable)
            {
                bool b = true;
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.Zero);
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(quickMenuPos);
                ImGui.Begin("QuestAWAY quick enable", ref b,
                    ImGuiWindowFlags.NoBackground
                    | ImGuiWindowFlags.NoNavFocus
                    | ImGuiWindowFlags.AlwaysUseWindowPadding
                    | ImGuiWindowFlags.AlwaysAutoResize
                    | ImGuiWindowFlags.NoScrollbar
                    | ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoFocusOnAppearing
                    );
                if(Static.ImGuiToggleButton("QuestAWAY", ref cfg.Enabled)) reprocess = true;
                Static.ImGuiTextTooltip((cfg.Enabled?"Disable":"Enable") + " plugin");
                ImGui.SameLine();
                if(Static.ImGuiIconButton(FontAwesomeIcon.Cog, "Settings"))
                {
                    open = true;
                }
                quickMenuSize = ImGui.GetWindowSize();
                ImGui.End();
                ImGui.PopStyleVar(2);
            }

            if (open)
            {
                reprocess = true;
                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("QuestAWAY configuration", ref open))
                {
                    if (ImGui.CollapsingHeader("Settings"))
                    {
                        ImGui.Checkbox("Plugin enabled", ref cfg.Enabled);
                        ImGui.Checkbox("Hide icons on big map", ref cfg.Bigmap);
                        ImGui.Checkbox("Hide icons on minimap", ref cfg.Minimap);
                        ImGui.Checkbox("Display quick enable/disable on big map", ref cfg.QuickEnable);
                        ImGui.Text("Additional pathes to hide (one per line, without _hr1 and .tex)");
                        ImGui.InputTextMultiline("##QAUSERADD", ref cfg.CustomPathes, 1000000, new Vector2(300f, 100f));
                        if (ImGui.CollapsingHeader("Developer settings"))
                        {
                            ImGui.Checkbox("[debug] Enable superverbosity", ref cfg.Superverbose);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip("This will cause your log file to grow ENORMMOUS.\n" +
                                    "But in case you are crashing, it will help to find out why.");
                            }
                            ImGui.Checkbox("[dev] Enable texture collecting", ref collect);
                            if (collect)
                            {
                                if (ImGui.Button("Reset"))
                                {
                                    texSet.Clear();
                                }
                                ImGui.SameLine();
                                ImGui.Checkbox("Display textures", ref collectDisplay);
                                if (collectDisplay) 
                                {
                                    foreach (var e in texSet)
                                    {
                                        ImGuiDrawImage(e);
                                        ImGui.SameLine();
                                        if (!Static.MapIcons.Contains(e)) ImGui.PushStyleColor(ImGuiCol.Button, 0xff0000ff);
                                        if(ImGui.Button("Copy: " + e))
                                        {
                                            Clipboard.SetText(e);
                                        }
                                        if (!Static.MapIcons.Contains(e)) ImGui.PopStyleColor();
                                    }
                                }
                                var s = string.Join("\n", texSet);
                                ImGui.InputTextMultiline("##QADATA", ref s, 1000000, new Vector2(300f, 100f));
                            }

                            ImGui.Separator();
                            if(ImGui.Button("Clear hidden textures list"))
                            {
                                cfg.HiddenTextures.Clear();
                            }
                            ImGui.Checkbox("Profiling", ref profiling);
                            if (profiling)
                            {
                                ImGui.Text("Total time: " + totalTime);
                                ImGui.Text("Total ticks: " + totalTicks);
                                ImGui.Text("Tick avg: " + (float)totalTime / (float)totalTicks);
                                ImGui.Text("MS avg: " + ((float)totalTime / (float)totalTicks) / (float)Stopwatch.Frequency * 1000 + " ms");
                                if (ImGui.Button("Reset##SW"))
                                {
                                    totalTicks = 0;
                                    totalTime = 0;
                                }
                            }
                        }
                        ImGui.Separator();
                    }
                    ImGui.Text("Icons to hide:");
                    ImGui.SameLine();
                    ImGui.Checkbox("Only selected", ref onlySelected);
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100f);
                    if (ImGui.BeginCombo("##QASELOPT", "Select..."))
                    {
                        if (ImGui.Selectable("All"))
                        {
                            cfg.HiddenTextures.UnionWith(Static.MapIcons);
                        }
                        if (ImGui.Selectable("None"))
                        {
                            cfg.HiddenTextures.Clear();
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
                        ImGui.SetColumnWidth(i, width / (float)numColumns);
                    }
                    foreach (var e in Static.MapIcons)
                    {
                        var b = cfg.HiddenTextures.Contains(e);
                        if (!onlySelected || cfg.HiddenTextures.Contains(e))
                        {
                            ImGui.Checkbox("##" + e, ref b);
                            if (cfg.HiddenTextures.Contains(e) && !b)
                            {
                                cfg.HiddenTextures.Remove(e);
                            }
                            if (!cfg.HiddenTextures.Contains(e) && b)
                            {
                                cfg.HiddenTextures.Add(e);
                            }
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 11);
                            ImGuiDrawImage(e+"_hr1");
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 11);
                            ImGui.NextColumn();
                        }
                    }
                    ImGui.Columns(1);
                    ImGui.EndChild();
                }
                ImGui.End();
                if (!open)
                {
                    cfg.Save();
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public void Tick(object _)
        {
            try
            {
                if (profiling)
                {
                    totalTicks++;
                    stopwatch.Restart();
                }
                Superverbose("Tick begins:" + ++tick);
                if (reprocess) BuildByteSet();
                openQuickEnable = false;
                if ((cfg.Enabled && cfg.Bigmap) || reprocess)
                {
                    ProcessMap(!(cfg.Enabled && cfg.Bigmap));
                }
                if ((cfg.Enabled && cfg.Minimap) || reprocess)
                {
                    ProcessMinimap(!(cfg.Enabled && cfg.Minimap));
                }
                reprocess = false;
                Superverbose("Tick ends. ");
                if (profiling)
                {
                    stopwatch.Stop();
                    totalTime += stopwatch.ElapsedTicks;
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("=== Error during QuestAway plugin execution ===" + e.Message + "\n" + e.StackTrace);
                pi.Framework.Gui.Chat.Print("[QuestAway] An error occurred, please send your log to developer.");
            }
        }

        void ProcessMap(bool showAll = false)
        {
            var o = pi.Framework.Gui.GetUiObjectByName("AreaMap", 1);
            if (o != IntPtr.Zero)
            {
                var masterWindow = (AtkUnitBase*)o;
                Superverbose("Preparing to access map window");
                if (masterWindow->IsVisible)
                {
                    Superverbose("Preparing to access map component node");
                    var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[3];
                    for (var i = 6; i < mapCNode->Component->UldManager.NodeListCount; i++)
                    {
                        Superverbose("Preparing to process icon #"+i);
                        ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i], showAll);
                    }
                    if (cfg.QuickEnable)
                    {
                        openQuickEnable = true;
                        quickMenuPos.X = masterWindow->X + mapCNode->AtkResNode.X * masterWindow->Scale + mapCNode->AtkResNode.Width * masterWindow->Scale / 2 - quickMenuSize.X / 2;
                        quickMenuPos.Y = masterWindow->Y + mapCNode->AtkResNode.Y * masterWindow->Scale - quickMenuSize.Y;
                    }
                }
            }
        }

        void ProcessMinimap(bool showAll = false)
        {
            var o = pi.Framework.Gui.GetUiObjectByName("_NaviMap", 1);
            if (o != IntPtr.Zero)
            {
                var masterWindow = (AtkUnitBase*)o;
                Superverbose("Preparing to access minimap window");
                if (masterWindow->IsVisible)
                {
                    Superverbose("Preparing to access map component node");
                    var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[2];
                    for (var i = 4; i < mapCNode->Component->UldManager.NodeListCount; i++)
                    {
                        Superverbose("Preparing to process icon #" + i);
                        ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i], showAll);
                    }
                }
            }
        }

        void ProcessShit(AtkComponentNode* mapIconNode, bool showUnconditionally = false)
        {
            Superverbose("Accessing visibility state");
            if (!mapIconNode->AtkResNode.IsVisible) return;
            Superverbose("Accessing image node");
            var imageNode = (AtkImageNode*)mapIconNode->Component->UldManager.NodeList[4];
            Superverbose("Accessing texture info");
            var textureInfo = imageNode->PartsList->Parts[imageNode->PartId].UldAsset;
            Superverbose("Accessing texture type");
            if (textureInfo->AtkTexture.TextureType == TextureType.Resource)
            {
                if (collect) texSet.Add(Marshal.PtrToStringAnsi((IntPtr)textureInfo->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName).Replace(".tex", "").Replace("_hr1", ""));
                Superverbose("Running comparison with generated byte array");
                if (!showUnconditionally && StartsWithAny((byte*)textureInfo->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName, cfgHideSet))
                {
                    Superverbose("Setting alpha to 0");
                    if (mapIconNode->AtkResNode.Color.A != 0) mapIconNode->AtkResNode.Color.A = 0;
                }
                else
                {
                    Superverbose("Setting alpha to ff");
                    if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                }
            }
        }

        bool StartsWithAny(byte* sPtr, byte[][] set)
        {
            foreach (var b in set)
            {
                for(int i = 0; i < b.Length; i++)
                {
                    if (*(sPtr+i) != b[i]) break;
                    if (i == b.Length - 1) return true;
                }
            }
            return false;
        }

        void BuildByteSet()
        {
            var userLines = cfg.CustomPathes.Split('\n');
            for (var n = 0; n < userLines.Length; n++)
            {
                userLines[n] = userLines[n].Trim();
            }
            var userLinesSet = userLines.ToHashSet();
            userLinesSet.RemoveWhere((string line) => { return line.Length == 0; });
            userLinesSet.UnionWith(cfg.HiddenTextures);
            cfgHideSet = new byte[userLinesSet.Count][];
            var i = 0;
            foreach(var e in userLinesSet)
            {
                cfgHideSet[i++] = Encoding.ASCII.GetBytes(e);
            }
        }

        void Superverbose(string s)
        {
            if(cfg.Superverbose) PluginLog.Information("[QuestAWAY " + tick + "] " + s);
        }
    }
}
