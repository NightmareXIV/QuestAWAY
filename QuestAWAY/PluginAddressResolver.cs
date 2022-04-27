using System;
using Dalamud.Game;
using Dalamud.Logging;

namespace QuestAWAY;

internal class PluginAddressResolver : BaseAddressResolver
{
    // to update this sig:
    // breakpoint Component::GUI::AtkResNode_SetVisibility with parameters (x, 1)
    // where x is an AtkResNode (Multipurpose Component Node) currently visible on the map (use dalamud to copy the ptr)
    // check the stack to see where it is being called
    // should be from a function that ends with:
    //     Component::GUI::AtkResNode_SetVisibility(v15, 1);
    //     Component::GUI::AtkResNode_SetRotationDegrees(v15);
    //     sub_1405517F0(*a1);
    // sig that function
    private const string MapAreaSetVisibilityAndRotation = "E8 ?? ?? ?? ?? 66 0F 6E 44 24 ?? 48 8B CE";
    
    // to update this sig:
    // breakpoint Component__GUI__AtkTooltipManager_ShowTooltip
    // slowly mouseover an icon on the minimap and nothing else, check the stack for the caller
    // in IDA search for references, only has 1 in patch 6.1, appears in a condition
    // on the first branch of that condition is an offset (a1 + 14872)
    // check what writes to that address, mouseover the minimap and take the code address that does a write everytime the mouse moves
    // in that code address there should be something like "result = sub_141030B80(a1, *a2, a2[1]);" , in 6.1 it is the first function call
    // sig that sub_141030B80
    private const string NaviMapOnMouseMove = "E8 ?? ?? ?? ?? 8B F0 3B 87";
    
    // in NaviMapOnMouseMove function, there's 3 ifs. this function is the second condition in all of them
    private const string CheckAtkCollisionNodeIntersect = "E8 ?? ?? ?? ?? 84 C0 75 B9";
    
    private const string AddonAreaMapOnUpdate = "48 8B C4 48 89 48 08 55 57";
    private const string AddonAreaMapOnRefresh = "4C 8B DC 55 56 41 56 41 57 49 8D 6B A1";
    private const string AddonNaviMapOnUpdate = "48 8B C4 55 48 81 EC ?? ?? ?? ?? F6 81";

    public IntPtr MapAreaSetVisibilityAndRotationAddress { get; private set; }
    public IntPtr ClientUiAddonAreaMapOnUpdateAddress { get; private set; }
    public IntPtr ClientUiAddonAreaMapOnRefreshAddress { get; private set; }
    public IntPtr AddonNaviMapOnUpdateAddress { get; private set; }
    public IntPtr NaviMapOnMouseMoveAddress { get; private set; }
    public IntPtr CheckAtkCollisionNodeIntersectAddress { get; private set; }

    /// <inheritdoc/>
    protected override void Setup64Bit(SigScanner scanner)
    {
        MapAreaSetVisibilityAndRotationAddress = scanner.ScanText(MapAreaSetVisibilityAndRotation);
        ClientUiAddonAreaMapOnUpdateAddress = scanner.ScanText(AddonAreaMapOnUpdate);
        ClientUiAddonAreaMapOnRefreshAddress = scanner.ScanText(AddonAreaMapOnRefresh);
        AddonNaviMapOnUpdateAddress = scanner.ScanText(AddonNaviMapOnUpdate);
        NaviMapOnMouseMoveAddress = scanner.ScanText(NaviMapOnMouseMove);
        CheckAtkCollisionNodeIntersectAddress = scanner.ScanText(CheckAtkCollisionNodeIntersect);

        PluginLog.Verbose("===== QuestAWAY =====");
        PluginLog.Verbose($"{nameof(MapAreaSetVisibilityAndRotationAddress)} {MapAreaSetVisibilityAndRotationAddress:X}");
        PluginLog.Verbose($"{nameof(ClientUiAddonAreaMapOnUpdateAddress)} {ClientUiAddonAreaMapOnUpdateAddress:X}");
        PluginLog.Verbose($"{nameof(ClientUiAddonAreaMapOnRefreshAddress)} {ClientUiAddonAreaMapOnRefreshAddress:X}");
        PluginLog.Verbose($"{nameof(AddonNaviMapOnUpdateAddress)} {AddonNaviMapOnUpdateAddress:X}");
        PluginLog.Verbose($"{nameof(NaviMapOnMouseMoveAddress)} {NaviMapOnMouseMoveAddress:X}");
        PluginLog.Verbose($"{nameof(CheckAtkCollisionNodeIntersectAddress)} {CheckAtkCollisionNodeIntersectAddress:X}");
    }
}