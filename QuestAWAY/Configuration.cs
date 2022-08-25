using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Dictionary<uint, Configuration> ZoneHiddenTextures = new();

        [NonSerialized]
        private QuestAWAY p;

        public void Initialize(QuestAWAY p)
        {
            this.p = p;
        }

        public void Save()
        {
            Svc.PluginInterface.SavePluginConfig(this);
        }
    }
}
