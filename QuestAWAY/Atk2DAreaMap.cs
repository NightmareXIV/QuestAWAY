using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace QuestAWAY;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct Atk2DAreaMap
{
    [FieldOffset(0x70)] public AtkComponentNode* AtkComponentNode;
}