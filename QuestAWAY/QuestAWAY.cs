using Dalamud;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Interface;
using Dalamud.Logging;
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
using Dalamud.Hooking;

namespace QuestAWAY
{
    unsafe class QuestAWAY : IDalamudPlugin
    {
        public string Name => "QuestAWAY";
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
        private bool reprocess = true;
        long tick = 0;
        bool profiling = false;
        long totalTime;
        long totalTicks;
        byte[][] fateTexture;
        byte[] areaMarkerTexture;
        Stopwatch stopwatch;

        PluginAddressResolver addressResolver = new PluginAddressResolver();
        private readonly Hook<MapAreaSetVisibilityAndRotationDelegate> MapAreaSetVisibilityAndRotationHook;
        private readonly Hook<ClientUIAddonAreaMapOnUpdateDelegate> ClientUIAddonAreaMapOnUpdateHook;
        private readonly Hook<ClientUIAddonAreaMapOnRefreshDelegate> ClientUIAddonAreaMapOnRefreshHook;
        private readonly Hook<AddonNaviMapOnUpdateDelegate> AddonNaviMapOnUpdateHook;

        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Svc.PluginInterface.UiBuilder.Draw -= Draw;
            MapAreaSetVisibilityAndRotationHook.Dispose();
            ClientUIAddonAreaMapOnUpdateHook.Dispose();
            ClientUIAddonAreaMapOnRefreshHook.Dispose();
            AddonNaviMapOnUpdateHook.Dispose();
            cfg.Save();
            cfg.Enabled = false;
            Svc.Commands.RemoveHandler("/questaway");
            reprocess = true;
            Tick(null);
            foreach (var t in textures.Values)
            {
                t.Dispose();
            }
        }

        public QuestAWAY(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Svc>();
            textures = new Dictionary<string, TextureWrap>();
            cfg = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            cfg.Initialize(this);
            BuildByteSet();
            Static.PaddingVector = ImGui.GetStyle().WindowPadding;
            
            addressResolver.Setup();
            this.ClientUIAddonAreaMapOnUpdateHook = new Hook<ClientUIAddonAreaMapOnUpdateDelegate>(addressResolver.ClientUiAddonAreaMapOnUpdateAddress, this.ClientUIAddonAreaMapOnUpdateDetour);
            this.ClientUIAddonAreaMapOnUpdateHook.Enable();
            this.ClientUIAddonAreaMapOnRefreshHook = new Hook<ClientUIAddonAreaMapOnRefreshDelegate>(addressResolver.ClientUiAddonAreaMapOnRefreshAddress, this.ClientUIAddonAreaMapOnRefreshDetour);
            this.ClientUIAddonAreaMapOnRefreshHook.Enable();
            this.MapAreaSetVisibilityAndRotationHook = new Hook<MapAreaSetVisibilityAndRotationDelegate>(addressResolver.MapAreaSetVisibilityAndRotationAddress, this.MapAreaSetVisibilityAndRotationDetour);
            MapAreaSetVisibilityAndRotationHook.Disable();
            this.AddonNaviMapOnUpdateHook = new Hook<AddonNaviMapOnUpdateDelegate>(addressResolver.AddonNaviMapOnUpdateAddress, this.AddonNaviMapOnUpdateDetour);
            AddonNaviMapOnUpdateHook.Enable();

            Svc.Framework.Update += Tick;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { open = true; };
            stopwatch = new Stopwatch();
            Svc.Commands.AddHandler("/questaway", new CommandInfo(delegate
            {
                open = !open;
            })
            {
                HelpMessage = "open/close configuration"
            });
        }

        private delegate void* AddonNaviMapOnUpdateDelegate(void* a1, void* a2, void* a3);

        private void* AddonNaviMapOnUpdateDetour(void* a1, void* a2, void* a3)
        {
            var result = AddonNaviMapOnUpdateHook.Original(a1,a2,a3);
            ProcessMinimap(!(cfg.Enabled && cfg.Minimap));
            return result;
        }

        private delegate void* ClientUIAddonAreaMapOnRefreshDelegate(void* a1, void* a2, void* a3);

        private void* ClientUIAddonAreaMapOnRefreshDetour(void* a1, void* a2, void* a3)
        {
            var result = ClientUIAddonAreaMapOnRefreshHook.Original(a1,a2,a3);
            reprocess = true;
            return result;
        }

        private delegate void* ClientUIAddonAreaMapOnUpdateDelegate(void* a1, void* a2, void* a3);

        private void* ClientUIAddonAreaMapOnUpdateDetour(void* a1, void* a2, void* a3)
        {
            MapAreaSetVisibilityAndRotationHook.Enable();
            var result = ClientUIAddonAreaMapOnUpdateHook.Original(a1,a2,a3);
            MapAreaSetVisibilityAndRotationHook.Disable();
            return result;
        }

        private delegate void* MapAreaSetVisibilityAndRotationDelegate(void* pointerToSomeMapThing, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9);

        private void* MapAreaSetVisibilityAndRotationDetour(void* pointerToSomeMapThing, void* a2, void* a3, void* a4, void* a5, void* a6, void* a7, void* a8, void* a9)
        {
            var result = MapAreaSetVisibilityAndRotationHook.Original(pointerToSomeMapThing,a2,a3,a4,a5,a6,a7,a8,a9);
            //PluginLog.Debug($"pointerToMapThing: {(UInt64)pointerToSomeMapThing:X}");
            if (pointerToSomeMapThing != null)
            {
                var AtkComponentNodePtr = (AtkComponentNode**)(*(byte**)pointerToSomeMapThing + 0x70);
                //PluginLog.Debug($"mapThing: {(UInt64)AtkComponentNodePtr:X}");
                if (AtkComponentNodePtr != null)
                {
                    var node = *AtkComponentNodePtr;
                    if (node != null)
                    {
                        //PluginLog.Debug($"node: {(UInt64)(*(byte**)node):X}");
                        //node->AtkResNode.ToggleVisibility(false);
                        ProcessShit(node, !(cfg.Enabled && cfg.Bigmap), true);
                    }
                }
            }
            return result;
        }

        void ImGuiDrawImage(string partialPath)
        {
            try
            {
                if (!textures.ContainsKey(partialPath))
                {
                    textures[partialPath] = Svc.Data.GetImGuiTexture(partialPath + ".tex");
                }
                ImGui.Image(textures[partialPath].ImGuiHandle, Vector2Scale, Vector2.Zero, Vector2.One, Vector4.One);
            }
            catch (Exception ex)
            {
                Svc.Chat.Print("[QuestAWAY error] " + ex.Message + "\n" + ex.StackTrace);
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
                if (Static.ImGuiToggleButton(FontAwesomeIcon.ExclamationCircle, (cfg.Enabled ? "Disable" : "Enable") + " QuestAWAY", ref cfg.Enabled))
                {
                    reprocess = true;
                }
                ImGui.SameLine();
                if (Static.ImGuiIconButton(FontAwesomeIcon.Cog, "QuestAWAY settings"))
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
                                            ImGui.SetClipboardText(e);
                                        }
                                        if (!Static.MapIcons.Contains(e)) ImGui.PopStyleColor();
                                    }
                                }
                                var s = string.Join("\n", texSet);
                                ImGui.InputTextMultiline("##QADATA", ref s, 1000000, new Vector2(300f, 100f));
                            }

                            ImGui.Separator();
                            if (ImGui.Button("Clear hidden textures list" + (ImGui.GetIO().KeyCtrl ? "" : " (hold ctrl and click)")) && ImGui.GetIO().KeyCtrl)
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
                    ImGui.Text("Special hiding options:");
                    ImGui.Checkbox("Hide fate circles", ref cfg.HideFateCircles);
                    ImGui.Checkbox("Hide subarea markers, but keep text", ref cfg.HideAreaMarkers);
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
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 11);
                            ImGuiDrawImage(e+"_hr1");
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
                            if (cfg.HiddenTextures.Contains(e) && !b)
                            {
                                cfg.HiddenTextures.Remove(e);
                            }
                            if (!cfg.HiddenTextures.Contains(e) && b)
                            {
                                cfg.HiddenTextures.Add(e);
                            }
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
                if (reprocess) BuildByteSet();
                var o = Svc.GameGui.GetAddonByName("AreaMap", 1);
                //PluginLog.Debug($"...: {setupCount} {reprocess} {firstUpdateFrame}");
                if (o != IntPtr.Zero)
                {
                    var masterWindow = (AtkUnitBase*)o;
                    if (masterWindow->IsVisible && masterWindow->UldManager.NodeListCount > 3)
                    {
                        var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[3];
                        openQuickEnable = cfg.QuickEnable;
                        if (openQuickEnable)
                        {
                            quickMenuPos.X = masterWindow->X + mapCNode->AtkResNode.X * masterWindow->Scale + mapCNode->AtkResNode.Width * masterWindow->Scale / 2 - quickMenuSize.X / 2;
                            quickMenuPos.Y = masterWindow->Y + mapCNode->AtkResNode.Y * masterWindow->Scale - quickMenuSize.Y;
                        }
                        if (reprocess)
                        {
                            ProcessMap(!(cfg.Enabled && cfg.Bigmap), masterWindow, mapCNode);
                        }
                    }
                    else
                    {
                        openQuickEnable = false;
                    }
                }
                else
                {
                    openQuickEnable = false;
                }
                reprocess = false;
                if (profiling)
                {
                    stopwatch.Stop();
                    totalTime += stopwatch.ElapsedTicks;
                }
            }
            catch (Exception e)
            {
                PluginLog.Error("=== Error during QuestAway plugin execution ===" + e.Message + "\n" + e.StackTrace);
                Svc.Chat.Print("[QuestAway] An error occurred, please send your log to developer.");
            }
        }

        void ProcessMap(bool showAll, AtkUnitBase* masterWindow, AtkComponentNode* mapCNode)
        {
            if (masterWindow->IsVisible)
            {
                PluginLog.Debug($"Reprocessing: {reprocess}");
                for (var i = 6; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i], showAll, true);
                }
            }
        }

        void ProcessMinimap(bool showAll = false)
        {
            var o = Svc.GameGui.GetAddonByName("_NaviMap", 1);
            if (o != IntPtr.Zero)
            {
                var masterWindow = (AtkUnitBase*)o;
                if (masterWindow->IsVisible && masterWindow->UldManager.NodeListCount > 2)
                {
                    var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[2];
                    for (var i = 4; i < mapCNode->Component->UldManager.NodeListCount; i++)
                    {
                        AtkComponentNode* mapIconNode = (AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i];
                        if (!mapIconNode->AtkResNode.IsVisible)
                            continue;
                        ProcessShit(mapIconNode, showAll);
                    }
                }
            }
        }

        void ProcessShit(AtkComponentNode* mapIconNode, bool showUnconditionally = false, bool isMap = false)
        {
            //if (!mapIconNode->AtkResNode.IsVisible) return;
            if (mapIconNode->Component->UldManager.NodeListCount <= 4) return;
            AtkImageNode* imageNode;
            if (mapIconNode->Component->UldManager.NodeList[4]->Type == NodeType.Image)
            {
                imageNode = (AtkImageNode*)mapIconNode->Component->UldManager.NodeList[4];
            }
            else if(mapIconNode->Component->UldManager.NodeList[3]->Type == NodeType.Image)
            {
                imageNode = (AtkImageNode*)mapIconNode->Component->UldManager.NodeList[3];
            }
            else
            {
                return;
            }
            var textureInfo = imageNode->PartsList->Parts[imageNode->PartId].UldAsset;
            if (textureInfo->AtkTexture.TextureType == TextureType.Resource)
            {
                if (collect) texSet.Add(Marshal.PtrToStringAnsi((IntPtr)textureInfo->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName.BufferPtr).Replace(".tex", "").Replace("_hr1", ""));
                var fNamePtr = textureInfo->AtkTexture.Resource->TexFileResourceHandle->ResourceHandle.FileName.BufferPtr;
                if (!showUnconditionally)
                {
                    if (
                        StartsWithAny(fNamePtr, cfgHideSet)
                        ||
                        (cfg.HideFateCircles && StartsWithAny(fNamePtr, fateTexture)
                        && imageNode->AtkResNode.AddBlue == 100 && imageNode->AtkResNode.MultiplyRed == 50)
                        )
                    {
                        //if (mapIconNode->AtkResNode.Color.A != 0) mapIconNode->AtkResNode.Color.A = 0;
                        mapIconNode->AtkResNode.ToggleVisibility(false);
                    }
                    else
                    {
                        //if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                        mapIconNode->AtkResNode.ToggleVisibility(true);
                    }

                    if (isMap && (cfg.HideAreaMarkers || reprocess) && StartsWith(fNamePtr, areaMarkerTexture))
                    {
                        if (cfg.HideAreaMarkers)
                        {
                            if (imageNode->AtkResNode.Color.A != 0) imageNode->AtkResNode.Color.A = 0;
                            //imageNode->AtkResNode.ToggleVisibility(false);
                        }
                        else
                        {
                            if (imageNode->AtkResNode.Color.A == 0) imageNode->AtkResNode.Color.A = 0xff;
                            //imageNode->AtkResNode.ToggleVisibility(true);
                        }
                    }
                }
                else
                {
                    //if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                    mapIconNode->AtkResNode.ToggleVisibility(true);
                    if (StartsWith(fNamePtr, areaMarkerTexture) && imageNode->AtkResNode.Color.A == 0) imageNode->AtkResNode.Color.A = 0xff;
                    //if (StartsWith(fNamePtr, areaMarkerTexture) && !imageNode->AtkResNode.IsVisible) imageNode->AtkResNode.ToggleVisibility(true);
                }
            }
        }

        bool StartsWithAny(byte* sPtr, byte[][] set)
        {
            foreach (var b in set)
            {
                if (StartsWith(sPtr, b)) return true;
            }
            return false;
        }

        bool StartsWith(byte* sPtr, byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (*(sPtr + i) != b[i]) break;
                if (i == b.Length - 1) return true;
            }
            return false;
        }

        void BuildByteSet()
        {
            fateTexture = new byte[][] { Encoding.ASCII.GetBytes("ui/icon/060000/060496"), Encoding.ASCII.GetBytes("ui/icon/060000/060495") };
            areaMarkerTexture = Encoding.ASCII.GetBytes("ui/icon/060000/060442");
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
    }
}
