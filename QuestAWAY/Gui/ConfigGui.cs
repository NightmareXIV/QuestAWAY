using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using ImGuiNET;
using System.Numerics;

namespace QuestAWAY.Gui
{
    internal class ConfigGui : Window
    {
        public ConfigGui() : base("QuestAWAY configuration")
        { }

        public override void OnClose()
        {
            base.OnClose();
            P.cfg.Save();
        }

        public override void PreDraw()
        {
            base.PreDraw();
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
        }

        public override void Draw()
        {
            P.reprocessAreaMap = true;
            P.reprocessNaviMap = true;

            ImGuiEx.EzTabBar("questaway",
                ("Global settings", MainSettings.Draw, null, true),
                ("Per-zone settings", ZoneSettings.Draw, null, true),
                ("Developer functions", DevSettings.Draw, null, true)
                );
        }
    }
}
