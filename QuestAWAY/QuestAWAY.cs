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
using Dalamud.Interface.Windowing;
using QuestAWAY.Gui;
using ECommons;
using Dalamud.Interface.Utility;
using ECommons.Schedulers;
using Dalamud.Interface.Internal;

namespace QuestAWAY
{
    unsafe class QuestAWAY : IDalamudPlugin
    {
        public string Name => "QuestAWAY";
        internal static QuestAWAY P;
        internal bool collect = false;
        internal bool collectDisplay = false;
        internal HashSet<string> texSet = new HashSet<string>();
        internal HashSet<string> userDefinedTextureSet = new HashSet<string>();
        internal Vector2 quickMenuPos = new Vector2(0f, 0f);
        internal Vector2 quickMenuSize = Vector2.Zero;
        internal Dictionary<string, IDalamudTextureWrap> textures;
        internal Configuration cfg;
        internal static Vector2 Vector2Scale = new Vector2(48f, 48f);
        internal bool onlySelected = false;
        internal bool openQuickEnable = false;
        internal byte[][] cfgHideSet = { };
        internal volatile bool reprocessNaviMap = true;
        internal volatile bool reprocessAreaMap = true;
        internal long tick = 0;
        internal bool profiling = false;
        internal long totalTime;
        internal long totalTicks;
        internal byte[][] fateTexture;
        internal byte[] areaMarkerTexture;
        internal Stopwatch stopwatch;
        internal WindowSystem windowSystem;
        internal ConfigGui configGui;

        PluginAddressResolver addressResolver = new PluginAddressResolver();
        private Hook<AddonAreaMapOnUpdateDelegate> AddonAreaMapOnUpdateHook;
        private Hook<AddonNaviMapOnUpdateDelegate> AddonNaviMapOnUpdateHook;
        private Hook<NaviMapOnMouseMoveDelegate> NaviMapOnMouseMoveHook;
        private Hook<CheckAtkCollisionNodeIntersectDelegate> CheckAtkCollisionNodeIntersectHook;
        private Hook<AreaMapOnMouseMoveDelegate> AreaMapOnMouseMoveHook;

        public static MemoryReplacer AreaMapCtrlAlwaysOn;

        public static Configuration CurrentProfile;

        public void Dispose()
        {
            //Dalamud doesn't lets reload plugin if exception is thrown by Dispose method. It is unwanted behavior, bypassing it.
            //Individual try-catches let as many things execute as possible
            Safe(() => {
                Svc.Framework.Update -= Tick;
                Svc.PluginInterface.UiBuilder.Draw -= Draw;
                Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
                Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
            });
            Safe(AddonAreaMapOnUpdateHook.Dispose);
            Safe(AddonNaviMapOnUpdateHook.Dispose);
            Safe(NaviMapOnMouseMoveHook.Dispose);
            Safe(AreaMapOnMouseMoveHook.Dispose);
            Safe(CheckAtkCollisionNodeIntersectHook.Dispose);
            Safe(cfg.Save);

            Safe(AreaMapCtrlAlwaysOn.Dispose);

            Safe(() =>
            {
                CurrentProfile.Enabled = false;
                reprocessNaviMap = true;
                reprocessAreaMap = true;
                ProcessMinimap((AtkUnitBase*)Svc.GameGui.GetAddonByName("_NaviMap", 1));
                ProcessAreaMap((AtkUnitBase*)Svc.GameGui.GetAddonByName("AreaMap", 1));

                Svc.Commands.RemoveHandler("/questaway");
                foreach (var t in textures.Values)
                {
                    t.Dispose();
                }
            });
            ECommonsMain.Dispose();
            P = null;
        }

        public QuestAWAY(DalamudPluginInterface pluginInterface)
        {
            ECommonsMain.Init(pluginInterface, this);
            P = this;
            //this is because Dalamud can now execute constructor in different thread, which we never want
            new TickScheduler(delegate
            {
                windowSystem = new();
                configGui = new();
                windowSystem.AddWindow(configGui);
                textures = new();
                cfg = pluginInterface.GetPluginConfig() as Configuration ?? new();
                Static.PaddingVector = ImGui.GetStyle().WindowPadding;

                // hook setup
                addressResolver.Setup64Bit(Svc.SigScanner);
#pragma warning disable CS0618
                AddonAreaMapOnUpdateHook =
                    Svc.Hook.HookFromAddress<AddonAreaMapOnUpdateDelegate>(addressResolver.AddonAreaMapOnUpdateAddress,
                        AddonAreaMapOnUpdateDetour);
                AddonNaviMapOnUpdateHook =
                    Svc.Hook.HookFromAddress<AddonNaviMapOnUpdateDelegate>(addressResolver.AddonNaviMapOnUpdateAddress,
                        AddonNaviMapOnUpdateDetour);
                NaviMapOnMouseMoveHook =
                    Svc.Hook.HookFromAddress<NaviMapOnMouseMoveDelegate>(addressResolver.NaviMapOnMouseMoveAddress,
                        NaviMapOnMouseMoveDetour);
                CheckAtkCollisionNodeIntersectHook = Svc.Hook.HookFromAddress<CheckAtkCollisionNodeIntersectDelegate>(
                    addressResolver.CheckAtkCollisionNodeIntersectAddress, CheckAtkCollisionNodeIntersectDetour);
                AreaMapOnMouseMoveHook =
                    Svc.Hook.HookFromAddress<AreaMapOnMouseMoveDelegate>(addressResolver.AreaMapOnMouseMoveAddress,
                        AreaMapOnMouseMoveDetour);
                AddonAreaMapOnUpdateHook.Enable();
                NaviMapOnMouseMoveHook.Enable();
                CheckAtkCollisionNodeIntersectHook.Disable();
                AddonNaviMapOnUpdateHook.Enable();
                AreaMapOnMouseMoveHook.Enable();
                AreaMapCtrlAlwaysOn = new(PluginAddressResolver.AreaMapCtrl, new byte[] { 0x0F, 0x94 });
                Svc.Framework.Update += Tick;
                Svc.PluginInterface.UiBuilder.Draw += Draw;
                Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
                Svc.PluginInterface.UiBuilder.OpenConfigUi += delegate { configGui.IsOpen = true; };
                stopwatch = new Stopwatch();
                Svc.Commands.AddHandler("/questaway", new CommandInfo(delegate { configGui.IsOpen = !configGui.IsOpen; })
                {
                    HelpMessage = "open/close configuration"
                });
                Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
                ClientState_TerritoryChanged(Svc.ClientState.TerritoryType);
            });
        }

        internal void ClientState_TerritoryChanged(ushort e)
        {
            if(!cfg.ZoneSettings.TryGetValue(e, out CurrentProfile))
            {
                CurrentProfile = cfg;
            }
            ApplyMemoryReplacer();
            BuildHiddenByteSet();
        }

        internal static void ApplyMemoryReplacer()
        {
            if (CurrentProfile.AetheryteInFront)
            {
                if (!AreaMapCtrlAlwaysOn.IsEnabled)
                {
                    AreaMapCtrlAlwaysOn.Enable();
                }
            }
            else
            {
                if (AreaMapCtrlAlwaysOn.IsEnabled)
                {
                    AreaMapCtrlAlwaysOn.Disable();
                }
            }
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
                if (Static.ImGuiToggleButton(FontAwesomeIcon.ExclamationCircle, (CurrentProfile.Enabled ? "Disable" : "Enable") + " QuestAWAY", ref CurrentProfile.Enabled))
                {
                    reprocessAreaMap = true;
                    reprocessNaviMap = true;
                }
                ImGui.SameLine();
                if (Static.ImGuiIconButton(FontAwesomeIcon.Cog, "QuestAWAY settings"))
                {
                    configGui.IsOpen = true;
                }
                quickMenuSize = ImGui.GetWindowSize();
                ImGui.End();
                ImGui.PopStyleVar(2);
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
                        openQuickEnable = CurrentProfile.QuickEnable;
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
            if ((!CurrentProfile.Enabled || !CurrentProfile.Bigmap) && !reprocessAreaMap) return;

            if (masterWindow != null
                && masterWindow->IsVisible
                && masterWindow->UldManager.NodeListCount > 3)
            {
                var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[3];
                for (var i = 6; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i], !(CurrentProfile.Enabled && CurrentProfile.Bigmap), true);
                }
                reprocessAreaMap = false;
            }
        }

        void ProcessMinimap(AtkUnitBase* masterWindow)
        {
            if ((!CurrentProfile.Enabled || !CurrentProfile.Minimap) && !reprocessNaviMap) return;

            if (masterWindow != null
                && masterWindow->IsVisible
                && masterWindow->UldManager.NodeListCount > 2)
            {
                var mapCNode = (AtkComponentNode*)masterWindow->UldManager.NodeList[2];
                for (var i = 4; i < mapCNode->Component->UldManager.NodeListCount; i++)
                {
                    ProcessShit((AtkComponentNode*)mapCNode->Component->UldManager.NodeList[i],
                        !(CurrentProfile.Enabled && CurrentProfile.Minimap));
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
                    /*if(StartsWithAny(fNamePtr, fateTexture))
                    {
                        PluginLog.Information($"{imageNode->AtkResNode.AddBlue}, {imageNode->AtkResNode.AddRed}, {imageNode->AtkResNode.AddGreen}, {imageNode->AtkResNode.MultiplyBlue}, {imageNode->AtkResNode.MultiplyRed}, {imageNode->AtkResNode.MultiplyGreen}, ");
                    }*/
                    if (
                        StartsWithAny(fNamePtr, cfgHideSet)
                        ||
                        (CurrentProfile.HideFateCircles && StartsWithAny(fNamePtr, fateTexture)
                        && imageNode->AtkResNode.AddBlue == 128 && imageNode->AtkResNode.AddGreen == 48 && imageNode->AtkResNode.MultiplyBlue == 100 && imageNode->AtkResNode.MultiplyGreen == 60)
                        )
                    {
                        if (mapIconNode->AtkResNode.Color.A != 0) mapIconNode->AtkResNode.Color.A = 0;
                    }
                    else
                    {
                        if (mapIconNode->AtkResNode.Color.A == 0) mapIconNode->AtkResNode.Color.A = 0xff;
                    }

                    if (isAreaMap && (CurrentProfile.HideAreaMarkers || reprocessAreaMap) && StartsWith(fNamePtr, areaMarkerTexture))
                    {
                        if (CurrentProfile.HideAreaMarkers)
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

        internal bool StartsWithAny(byte* sPtr, byte[][] set)
        {
            foreach (var b in set)
            {
                if (StartsWith(sPtr, b)) return true;
            }
            return false;
        }

        internal bool StartsWith(byte* sPtr, byte[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (*(sPtr + i) != b[i]) break;
                if (i == b.Length - 1) return true;
            }
            return false;
        }

        internal void BuildHiddenByteSet()
        {
            fateTexture = new byte[][] { Encoding.ASCII.GetBytes("ui/icon/060000/060496"), Encoding.ASCII.GetBytes("ui/icon/060000/060495") };
            areaMarkerTexture = Encoding.ASCII.GetBytes("ui/icon/060000/060442");
            var userLines = CurrentProfile.CustomPathes.Split('\n');
            for (var n = 0; n < userLines.Length; n++)
            {
                userLines[n] = userLines[n].Trim();
            }
            var userLinesSet = userLines.ToHashSet();
            userLinesSet.RemoveWhere((string line) => { return line.Length == 0; });
            userLinesSet.UnionWith(CurrentProfile.HiddenTextures);
            
            cfgHideSet = new byte[userLinesSet.Count][];
            var i = 0;
            foreach(var e in userLinesSet)
            {
                cfgHideSet[i++] = Encoding.ASCII.GetBytes(e);
            }
        }
    }
}
