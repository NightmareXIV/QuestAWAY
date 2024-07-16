using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;

namespace QuestAWAY;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Atk2DAreaMap
{
    [FieldOffset(0x70)] public AtkComponentNode* AtkComponentNode;
}