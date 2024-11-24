using Dalamud.Game;
using ECommons.Logging;
using System;

namespace QuestAWAY;

internal class PluginAddressResolver
{
    // to update this sig:
    // breakpoint Component::GUI::AtkResNode_ToggleVisibility with parameters (x, 1)
    // where x is an AtkResNode (Multipurpose Component Node) currently visible on the map (use dalamud to copy the ptr)
    // check the stack to see where it is being called
    // should be from a function that ends with:
    //      Component::GUI::AtkResNode_ToggleVisibility(v17, 1u);
    //      Component::GUI::AtkResNode_SetRotationDegrees((__int64) v17, 0.0);
    //      sub_7FF747A15F00(*a1, a3);
    // if so sig the function youre in (FFXIV 7.01)
    private const string MapAreaSetVisibilityAndRotation = "E8 ?? ?? ?? ?? 66 0F 6E 44 24 ?? 48 8B CE";

    // to update this sig:
    // breakpoint Component__GUI__AtkTooltipManager_ShowTooltip
    // slowly mouseover an icon on the minimap and nothing else, check the stack for the caller
    // in IDA search for references, only has 1 in patch 6.1, appears in a condition
    // on the first branch of that condition is an offset (a1 + 14888)
    // check what writes to that address via a write trace, mouseover the minimap and take the code address that does a write everytime the mouse moves
    // if you have trouble adding the write trace fast enough and handing back execution to the game you can break on a call closeby of (a1 + 14872)
    // that way you only need to scroll up a bit, double click on the (a1 + 14888) offset and put the write trace on before resuming execution
    // sig the function the write trace comes from (FFXIV 7.01)
    private const string NaviMapOnMouseMove = "48 89 5C 24 ?? 57 48 83 EC 40 48 8B F9 0F B7 CA";

    // to update this sig:
    // from AreaMap ReceiveEvent, find the switch case where a2==5 and the if for a3==20
    // then the only unnamed function there is what we want  (FFXIV 7.01)
    private const string AreaMapOnMouseMove = "48 8B C4 53 57 48 83 EC 78 48 8B D9";

    private const string AtkCollisionNodeCheckCollisionAtCoords = "E8 ?? ?? ?? ?? 33 FF 84 C0 48 0F 45 FE";
    private const string AddonAreaMapOnUpdate = "48 8B C4 55 56 48 8D A8 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 ?? ?? ?? ?? ??";
    private const string AddonAreaMapOnRefresh = "4C 8B DC 55 56 57 41 56 49 8D 6B D8";
    private const string AddonNaviMapOnUpdate = "48 8B C4 55 48 81 EC ?? ?? ?? ?? F6 81 ?? ?? ?? ?? ??";

    // to update this sig:
    // Go to Client::UI::Agent::AgentMap_Update
    // There is a pseudo code pattern that looks like this
    //    v60 = (*(_DWORD*) (sub_140615D40(v59) + 72) & 1) != 0;
    //    if (v60 != this->IsControlKeyPressed )
    //    {
    //      this->UpdateFlags |= 0x40u;
    //      this->IsControlKeyPressed = v60;
    //    }
    // What we want to sig is the instruction for the comparison to not equal 0
    // The assembly for that looks like this
    //    call    sub_140615D40
    //    mov     ecx, [rax+48h]
    //    test    cs:dword_141EF0090, ecx
    //    setnz   al
    // The not eqal is the setnz al instruction
    // Just click on the address of the setnz al instruction and sig it (FFXIV 7.01)
    // The memory replacer then takes the 0F 95 (SETNZ) and replaces it with 0F 94 (SETZ)
    // this way it checks if the value at the address is 0 and the Ctrl key held routine is run
    public const string AreaMapCtrl = "0F 95 C0 3A 85 ?? ?? ?? ??";

    public IntPtr AddonAreaMapOnUpdateAddress { get; private set; }
    public IntPtr AddonNaviMapOnUpdateAddress { get; private set; }
    public IntPtr NaviMapOnMouseMoveAddress { get; private set; }
    public IntPtr AtkCollisionNodeCheckCollisionAtCoordsAddress { get; private set; }
    public IntPtr AreaMapOnMouseMoveAddress { get; private set; }

    /// <inheritdoc/>
    internal void Setup64Bit(ISigScanner scanner)
    {
        AddonAreaMapOnUpdateAddress = scanner.ScanText(AddonAreaMapOnUpdate);
        AddonNaviMapOnUpdateAddress = scanner.ScanText(AddonNaviMapOnUpdate);
        NaviMapOnMouseMoveAddress = scanner.ScanText(NaviMapOnMouseMove);
        AtkCollisionNodeCheckCollisionAtCoordsAddress = scanner.ScanText(AtkCollisionNodeCheckCollisionAtCoords);
        AreaMapOnMouseMoveAddress = scanner.ScanText(AreaMapOnMouseMove);

        PluginLog.Verbose("===== QuestAWAY =====");
        PluginLog.Verbose($"{nameof(AddonAreaMapOnUpdateAddress)} {AddonAreaMapOnUpdateAddress:X}");
        PluginLog.Verbose($"{nameof(AddonNaviMapOnUpdateAddress)} {AddonNaviMapOnUpdateAddress:X}");
        PluginLog.Verbose($"{nameof(NaviMapOnMouseMoveAddress)} {NaviMapOnMouseMoveAddress:X}");
        PluginLog.Verbose($"{nameof(AtkCollisionNodeCheckCollisionAtCoordsAddress)} {AtkCollisionNodeCheckCollisionAtCoordsAddress:X}");
        PluginLog.Verbose($"{nameof(AreaMapOnMouseMoveAddress)} {AreaMapOnMouseMoveAddress:X}");
    }
}