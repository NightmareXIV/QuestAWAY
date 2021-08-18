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
        public bool Superverbose = false;
        public HashSet<string> HiddenTextures = new HashSet<string>();
        public string CustomPathes = "";
        public bool HideFateCircles = false;

        [NonSerialized]
        private QuestAWAY p;

        public void Initialize(QuestAWAY p)
        {
            this.p = p;
        }

        public void Save()
        {
            p.pi.SavePluginConfig(this);
        }
    }
}
