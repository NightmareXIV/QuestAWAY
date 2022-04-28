using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud;
using Dalamud.Logging;

namespace QuestAWAY;

public class MemoryReplacer : IDisposable
{
    public IntPtr Address { get; private set; } = IntPtr.Zero;
    private readonly byte[] newBytes;
    private readonly byte[] oldBytes;
    public bool IsEnabled { get; private set; } = false;
    public bool IsValid => Address != IntPtr.Zero;

    public MemoryReplacer(string sig, byte[] bytes, bool startEnabled = false)
    {
        var addr = IntPtr.Zero;
        try
        {
            addr = Svc.SigScanner.ScanModule(sig);
        }
        catch
        {
            PluginLog.LogError($"Failed to find signature {sig}");
        }

        if (addr == IntPtr.Zero) return;

        Address = addr;
        newBytes = bytes;
        SafeMemory.ReadBytes(addr, bytes.Length, out oldBytes);

        if (startEnabled)
            Enable();
    }

    public void Enable()
    {
        if (!IsValid) return;
        SafeMemory.WriteBytes(Address, newBytes);
        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsValid) return;
        SafeMemory.WriteBytes(Address, oldBytes);
        IsEnabled = false;
    }

    public void Toggle()
    {
        if (!IsEnabled)
            Enable();
        else
            Disable();
    }

    public void Dispose()
    {
        if (IsEnabled)
            Disable();
    }
}