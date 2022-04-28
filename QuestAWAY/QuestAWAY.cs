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
        private byte[][] cfgHideSet = { };
        private volatile bool reprocessNaviMap = true;
        private volatile bool reprocessAreaMap = true;
        long tick = 0;
        bool profiling = false;
        long totalTime;
        long totalTicks;
        byte[][] fateTexture;
        byte[] areaMarkerTexture;
        Stopwatch stopwatch;

        PluginAddressResolver addressResolver = new PluginAddressResolver();
        private readonly Hook<AddonAreaMapOnUpdateDelegate> AddonAreaMapOnUpdateHook;
        private readonly Hook<AddonNaviMapOnUpdateDelegate> AddonNaviMapOnUpdateHook;
        private readonly Hook<NaviMapOnMouseMoveDelegate> NaviMapOnMouseMoveHook;
        private readonly Hook<CheckAtkCollisionNodeIntersectDelegate> CheckAtkCollisionNodeIntersectHook;
        private readonly Hook<AreaMapOnMouseMoveDelegate> AreaMapOnMouseMoveHook;

        public static readonly MemoryReplacer AreaMapCtrlAlwaysOn =
            new(PluginAddressResolver.AreaMapCtrl, new byte[] { 0x0F, 0x94 });

        public void Dispose()
        {
            Svc.Framework.Update -= Tick;
            Svc.PluginInterface.UiBuilder.Draw -= Draw;
            AddonAreaMapOnUpdateHook.Dispose();
            AddonNaviMapOnUpdateHook.Dispose();
            NaviMapOnMouseMoveHook.Dispose();
            AreaMapOnMouseMoveHook.Dispose();
            CheckAtkCollisionNodeIntersectHook.Dispose();
            cfg.Save();
            
            AreaMapCtrlAlwaysOn.Dispose();

            cfg.Enabled = false;
            reprocessNaviMap = true;
            reprocessAreaMap = true;
            ProcessMinimap((AtkUnitBase*)Svc.GameGui.GetAddonByName("_NaviMap", 1));
            ProcessAreaMap((AtkUnitBase*)Svc.GameGui.GetAddonByName("AreaMap", 1));

            Svc.Commands.RemoveHandler("/questaway");
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
            BuildHiddenByteSet();
            Static.PaddingVector = ImGui.GetStyle().WindowPadding;

            // hook setup
            addressResolver.Setup();
            AddonAreaMapOnUpdateHook =
                new Hook<AddonAreaMapOnUpdateDelegate>(addressResolver.AddonAreaMapOnUpdateAddress,
                    AddonAreaMapOnUpdateDetour);
            AddonNaviMapOnUpdateHook =
                new Hook<AddonNaviMapOnUpdateDelegate>(addressResolver.AddonNaviMapOnUpdateAddress,
                    AddonNaviMapOnUpdateDetour);
            NaviMapOnMouseMoveHook =
                new Hook<NaviMapOnMouseMoveDelegate>(addressResolver.NaviMapOnMouseMoveAddress,
                    NaviMapOnMouseMoveDetour);
            CheckAtkCollisionNodeIntersectHook = new Hook<CheckAtkCollisionNodeIntersectDelegate>(
                addressResolver.CheckAtkCollisionNodeIntersectAddress, CheckAtkCollisionNodeIntersectDetour);
            AreaMapOnMouseMoveHook =
                new Hook<AreaMapOnMouseMoveDelegate>(addressResolver.AreaMapOnMouseMoveAddress,
                    AreaMapOnMouseMoveDetour);
            AddonAreaMapOnUpdateHook.Enable();
            NaviMapOnMouseMoveHook.Enable();
            CheckAtkCollisionNodeIntersectHook.Disable();
            AddonNaviMapOnUpdateHook.Enable();
            AreaMapOnMouseMoveHook.Enable();
            
            if(cfg.AetheryteInFront)
                AreaMapCtrlAlwaysOn.Enable();

            Svc.Framework.Update += Tick;
            Svc.PluginInterface.UiBuilder.Draw += Draw;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { open = true; };
            stopwatch = new Stopwatch();
            Svc.Commands.AddHandler("/questaway", new CommandInfo(delegate { open = !open; })
            {
                HelpMessage = "open/close configuration"
            });
        }

        private delegate byte CheckAtkCollisionNodeIntersectDelegate(AtkNineGridNode* node, void* a2, void* a3,
            void* a4);

        private byte CheckAtkCollisionNodeIntersectDetour(AtkNineGridNode* node, void* a2, void* a3, void* a4)
        {
            if (node != null && node->AtkResNode.ParentNode != null)
            {
                // make intersection check fail if the map (both of them) icon is transparent
                return node->AtkResNode.ParentNode->Color.A != 0
                    ? CheckAtkCollisionNodeIntersectHook.Original(node, a2, a3, a4)
                    : (byte)0;
            }

            return CheckAtkCollisionNodeIntersectHook.Original(node, a2, a3, a4);
        }

        private delegate IntPtr AreaMapOnMouseMoveDelegate(IntPtr unk1, IntPtr unk2);

        private IntPtr AreaMapOnMouseMoveDetour(IntPtr unk1, IntPtr unk2)
        {
            // optimization: only hook CheckAtkCollisionNodeIntersect when mouseovering the AreaMap
            CheckAtkCollisionNodeIntersectHook.Enable();
            var result = AreaMapOnMouseMoveHook.Original(unk1, unk2);
            CheckAtkCollisionNodeIntersectHook.Disable();
            return result;
        }

        private delegate IntPtr NaviMapOnMouseMoveDelegate(IntPtr unk1, IntPtr unk2, IntPtr unk3);

        private IntPtr NaviMapOnMouseMoveDetour(IntPtr unk1, IntPtr unk2, IntPtr unk3)
        {
            // optimization: only hook CheckAtkCollisionNodeIntersect when mouseovering the minimap
            CheckAtkCollisionNodeIntersectHook.Enable();
            var result = NaviMapOnMouseMoveHook.Original(unk1, unk2, unk3);
            CheckAtkCollisionNodeIntersectHook.Disable();
            return result;
        }

        private delegate IntPtr AddonNaviMapOnUpdateDelegate(AtkUnitBase* addonNaviMap, IntPtr unk2, IntPtr unk3);

        private IntPtr AddonNaviMapOnUpdateDetour(AtkUnitBase* addonNaviMap, IntPtr unk2, IntPtr unk3)
        {
            // runs every frame (i think) if the minimap is visible
            var result = AddonNaviMapOnUpdateHook.Original(addonNaviMap, unk2, unk3);

            ProfilingContinue();
            ProcessMinimap(addonNaviMap);
            ProfilingPause();

            return result;
        }

        private delegate IntPtr AddonAreaMapOnUpdateDelegate(AtkUnitBase* addonAreaMap, IntPtr unk2, IntPtr unk3);

        private IntPtr AddonAreaMapOnUpdateDetour(AtkUnitBase* addonAreaMap, IntPtr unk2, IntPtr unk3)
        {
            // runs every frame if the area map is open and loaded
            var result = AddonAreaMapOnUpdateHook.Original(addonAreaMap, unk2, unk3);

            ProfilingContinue();
            ProcessAreaMap(addonAreaMap);
            ProfilingPause();

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
                    reprocessAreaMap = true;
                    reprocessNaviMap = true;
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
                reprocessAreaMap = true;
                reprocessNaviMap = true;
                ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("QuestAWAY configuration", ref open))
                {
                    if (ImGui.CollapsingHeader("Settings"))
                    {
                        ImGui.Checkbox("Plugin enabled", ref cfg.Enabled);
                        ImGui.Checkbox("Hide icons on big map", ref cfg.Bigmap);
                        ImGui.Checkbox("Hide icons on minimap", ref cfg.Minimap);
                        ImGui.Checkbox("Display quick enable/disable on big map", ref cfg.QuickEnable);
                        if(ImGui.Checkbox("Aetherytes always in front on big map", ref cfg.AetheryteInFront))
                            AreaMapCtrlAlwaysOn.Toggle();
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
                                BuildHiddenByteSet();
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
                            BuildHiddenByteSet();
                        }
                        if (ImGui.Selectable("None"))
                        {
                            cfg.HiddenTextures.Clear();
                            BuildHiddenByteSet();
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
                                BuildHiddenByteSet();
                            }
                            if (!cfg.HiddenTextures.Contains(e) && b)
                            {
                                cfg.HiddenTextures.Add(e);
                                BuildHiddenByteSet();
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

        private void ProfilingContinue()
        {
            if (!profiling) return;
            stopwatch.Start();
        }

        private void ProfilingPause()
        {
            if (!profiling) return;
            stopwatch.Stop();
        }

        private void ProfilingRestart()
        {
            if (!profiling) return;
            
            totalTime += stopwatch.ElapsedTicks+1;
            totalTicks++;
            stopwatch.Restart();
        }

        [HandleProcessCorruptedStateExceptions]
        public void Tick(object _)
        {
            try
            {
                ProfilingRestart();
                var o = Svc.GameGui.GetAddonByName("AreaMap", 1);
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
                ProfilingPause();
            }
            catch (Exception e)
            {
                PluginLog.Error("=== Error during QuestAway plugin execution ===" + e.Message + "\n" + e.StackTrace);
                Svc.Chat.Print("[QuestAway] An error occurred, please send your log to developer.");
            }
        }

        void ProcessAreaMap(AtkUnitBase* masterWindow)
        {
            if ((!cfg.Enabled || !cfg.Bigmap) && !reprocessAreaMap) return;

            if (masterWindow != null
                && masterWindow->IsVisible
                && masterWindow->UldManager.NodeListCount > 3)
            {
                var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[3];
                for (var i = 6; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i], !(cfg.Enabled && cfg.Bigmap), true);
                }
                reprocessAreaMap = false;
            }
        }

        void ProcessMinimap(AtkUnitBase* masterWindow)
        {
            if ((!cfg.Enabled || !cfg.Minimap) && !reprocessNaviMap) return;

            if (masterWindow != null
                && masterWindow->IsVisible
                && masterWindow->UldManager.NodeListCount > 2)
            {
                var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[2];
                for (var i = 4; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i],
                        !(cfg.Enabled && cfg.Minimap));
                }
                reprocessNaviMap = false;
            }
        }

        void ProcessShit(AtkComponentNode* mapIconNode, bool showUnconditionally = false, bool isAreaMap = false)
        {
            if (!mapIconNode->AtkResNode.IsVisible) return;
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
                        if (mapIconNode->AtkResNode.Color.A != 0) mapIconNode->AtkResNode.Color.A = 0;
                    }
                    else
                    {
                        if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                    }

                    if (isAreaMap && (cfg.HideAreaMarkers || reprocessAreaMap) && StartsWith(fNamePtr, areaMarkerTexture))
                    {
                        if (cfg.HideAreaMarkers)
                        {
                            if (imageNode->AtkResNode.Color.A != 0) imageNode->AtkResNode.Color.A = 0;
                        }
                        else
                        {
                            if (imageNode->AtkResNode.Color.A == 0) imageNode->AtkResNode.Color.A = 0xff;
                        }
                    }
                }
                else
                {
                    if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                    if (StartsWith(fNamePtr, areaMarkerTexture) && imageNode->AtkResNode.Color.A == 0) imageNode->AtkResNode.Color.A = 0xff;
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

        void BuildHiddenByteSet()
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
