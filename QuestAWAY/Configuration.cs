using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace QuestAWAY
{
    [Serializable]
    class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public bool Enabled = true;
        public bool Minimap = true;
        public bool Bigmap = true;
        public bool QuickEnable = true;
        public HashSet<string> HiddenTextures = new();
        public string CustomPathes = "";
        public bool HideFateCircles = false;
        public bool HideAreaMarkers = false;
        public bool AetheryteInFront = false;
        public Dictionary<uint, Configuration> ZoneSettings = new();

        public void Save()
        {
            Svc.PluginInterface.SavePluginConfig(this);
        }
    }
}
