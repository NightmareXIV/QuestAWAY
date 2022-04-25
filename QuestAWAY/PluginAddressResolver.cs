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
    private const string AddonAreaMapOnUpdate = "48 8B C4 48 89 48 08 55 57";
    private const string AddonAreaMapOnRefresh = "4C 8B DC 55 56 41 56 41 57 49 8D 6B A1";
    private const string AddonNaviMapOnUpdate = "48 8B C4 55 48 81 EC ?? ?? ?? ?? F6 81";

    public IntPtr MapAreaSetVisibilityAndRotationAddress { get; private set; }
    public IntPtr ClientUiAddonAreaMapOnUpdateAddress { get; private set; }
    public IntPtr ClientUiAddonAreaMapOnRefreshAddress { get; private set; }
    public IntPtr AddonNaviMapOnUpdateAddress { get; private set; }

    /// <inheritdoc/>
    protected override void Setup64Bit(SigScanner scanner)
    {
        this.MapAreaSetVisibilityAndRotationAddress = scanner.ScanText(MapAreaSetVisibilityAndRotation);
        this.ClientUiAddonAreaMapOnUpdateAddress = scanner.ScanText(AddonAreaMapOnUpdate);
        this.ClientUiAddonAreaMapOnRefreshAddress = scanner.ScanText(AddonAreaMapOnRefresh);
        this.AddonNaviMapOnUpdateAddress = scanner.ScanText(AddonNaviMapOnUpdate);

        PluginLog.Verbose("===== QuestAWAY =====");
        PluginLog.Verbose(
            $"{nameof(this.MapAreaSetVisibilityAndRotationAddress)} {this.MapAreaSetVisibilityAndRotationAddress:X}");
        PluginLog.Verbose(
            $"{nameof(this.ClientUiAddonAreaMapOnUpdateAddress)} {this.ClientUiAddonAreaMapOnUpdateAddress:X}");
        PluginLog.Verbose(
            $"{nameof(this.ClientUiAddonAreaMapOnRefreshAddress)} {this.ClientUiAddonAreaMapOnRefreshAddress:X}");
        PluginLog.Verbose($"{nameof(this.AddonNaviMapOnUpdateAddress)} {this.AddonNaviMapOnUpdateAddress:X}");
    }
}