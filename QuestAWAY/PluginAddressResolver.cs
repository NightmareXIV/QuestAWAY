using System;
using Dalamud.Game;
using Dalamud.Logging;

namespace QuestAWAY;

/// <summary>
    /// Plugin address resolver.
    /// </summary>
    internal class PluginAddressResolver : BaseAddressResolver
    {
        private const string MapAreaSetVisibilityAndRotation = "E8 ?? ?? ?? ?? 66 0F 6E 44 24 ?? 48 8B CE";
        private const string AddonAreaMapOnUpdate = "48 8B C4 48 89 48 08 55 57";
        private const string AddonAreaMapOnRefresh = "4C 8B DC 55 56 41 56 41 57 49 8D 6B A1";
        private const string AddonNaviMapOnUpdate = "40 53 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 68 8B C2";
        
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
            PluginLog.Verbose($"{nameof(this.MapAreaSetVisibilityAndRotationAddress)} {this.MapAreaSetVisibilityAndRotationAddress:X}");
            PluginLog.Verbose($"{nameof(this.ClientUiAddonAreaMapOnUpdateAddress)} {this.ClientUiAddonAreaMapOnUpdateAddress:X}");
            PluginLog.Verbose($"{nameof(this.ClientUiAddonAreaMapOnRefreshAddress)} {this.ClientUiAddonAreaMapOnRefreshAddress:X}");
            PluginLog.Verbose($"{nameof(this.AddonNaviMapOnUpdateAddress)} {this.AddonNaviMapOnUpdateAddress:X}");
        }
    }