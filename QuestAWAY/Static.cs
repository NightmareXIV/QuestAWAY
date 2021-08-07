using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuestAWAY
{
    public static class Static
    {
        public const uint ActiveToggleButtonColor = 0x6600b500;

        public static bool ImGuiToggleButton(string text, ref bool flag)
        {
            var state = false;
            var colored = false;
            if (flag)
            {
                colored = true;
                ImGui.PushStyleColor(ImGuiCol.Button, ActiveToggleButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ActiveToggleButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ActiveToggleButtonColor);
            }
            if (ImGui.Button(text))
            {
                flag = !flag;
                state = true;
            }
            if (colored)
            {
                ImGui.PopStyleColor(3);
            }
            return state;
        }

        public static Vector2 PaddingVector;
        public static bool ImGuiIconButton(FontAwesomeIcon icon, string tooltip)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
            ImGui.PopFont();


            if (tooltip != null)
                ImGuiTextTooltip(tooltip);


            return result;
        }

        public static void ImGuiTextTooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, PaddingVector);
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(text);
                ImGui.EndTooltip();
                ImGui.PopStyleVar();
            }
        }

        public static SortedSet<string> MapIcons = new SortedSet<string>()
            {
                "ui/icon/060000/060311",
                "ui/icon/060000/060314",
                "ui/icon/060000/060318",
                "ui/icon/060000/060319",
                "ui/icon/060000/060320",
                "ui/icon/060000/060321",
                "ui/icon/060000/060322",
                "ui/icon/060000/060326",
                "ui/icon/060000/060330",
                "ui/icon/060000/060331",
                "ui/icon/060000/060333",
                "ui/icon/060000/060334",
                "ui/icon/060000/060335",
                "ui/icon/060000/060337",
                "ui/icon/060000/060339",
                "ui/icon/060000/060342",
                "ui/icon/060000/060344",
                "ui/icon/060000/060345",
                "ui/icon/060000/060346",
                "ui/icon/060000/060347",
                "ui/icon/060000/060348",
                "ui/icon/060000/060352",
                "ui/icon/060000/060362",
                "ui/icon/060000/060363",
                "ui/icon/060000/060364",
                "ui/icon/060000/060404",
                "ui/icon/060000/060412",
                "ui/icon/060000/060414",
                "ui/icon/060000/060425",
                "ui/icon/060000/060426",
                "ui/icon/060000/060427",
                "ui/icon/060000/060428",
                "ui/icon/060000/060430",
                "ui/icon/060000/060434",
                "ui/icon/060000/060436",
                "ui/icon/060000/060442",
                "ui/icon/060000/060443",
                "ui/icon/060000/060448",
                "ui/icon/060000/060449",
                "ui/icon/060000/060450",
                "ui/icon/060000/060451",
                "ui/icon/060000/060453",
                "ui/icon/060000/060456",
                "ui/icon/060000/060458",
                "ui/icon/060000/060459",
                "ui/icon/060000/060460",
                "ui/icon/060000/060467",
                "ui/icon/060000/060473",
                "ui/icon/060000/060496",
                "ui/icon/060000/060501",
                "ui/icon/060000/060502",
                "ui/icon/060000/060503",
                "ui/icon/060000/060504",
                "ui/icon/060000/060551",
                "ui/icon/060000/060554",
                "ui/icon/060000/060555",
                "ui/icon/060000/060561",
                "ui/icon/060000/060567",
                "ui/icon/060000/060568",
                "ui/icon/060000/060569",
                "ui/icon/060000/060570",
                "ui/icon/060000/060571",
                "ui/icon/060000/060581",
                "ui/icon/060000/060582",
                "ui/icon/060000/060600",
                "ui/icon/060000/060601",
                "ui/icon/060000/060603",
                "ui/icon/060000/060604",
                "ui/icon/060000/060751",
                "ui/icon/060000/060752",
                "ui/icon/060000/060753",
                "ui/icon/060000/060754",
                "ui/icon/060000/060755",
                "ui/icon/060000/060756",
                "ui/icon/060000/060768",
                "ui/icon/060000/060769",
                "ui/icon/060000/060770",
                "ui/icon/060000/060772",
                "ui/icon/060000/060773",
                "ui/icon/060000/060774",
                "ui/icon/060000/060776",
                "ui/icon/060000/060789",
                "ui/icon/060000/060910",
                "ui/icon/060000/060926",
                "ui/icon/060000/060927",
                "ui/icon/060000/060934",
                "ui/icon/060000/060935",
                "ui/icon/060000/060960",
                "ui/icon/060000/060983",
                "ui/icon/060000/060986",
                "ui/icon/060000/060987",
                "ui/icon/060000/060988",
                "ui/icon/060000/060993",
                "ui/icon/061000/061731",
                "ui/icon/063000/063903",
                "ui/icon/063000/063905",
                "ui/icon/063000/063919",
                "ui/icon/063000/063920",
                "ui/icon/063000/063922",
                "ui/icon/071000/071003",
                "ui/icon/071000/071013",
                "ui/icon/071000/071021",
                "ui/icon/071000/071022",
                "ui/icon/071000/071022",
                "ui/icon/071000/071031",
                "ui/icon/071000/071041",
                "ui/icon/071000/071121",
                "ui/icon/071000/071141",
                "ui/icon/071000/071151",
                "ui/icon/071000/071153",
                "ui/icon/060000/060791",
                "ui/icon/060000/060757",
                "ui/icon/060000/060771",
                "ui/icon/060000/060457",
                "ui/icon/063000/063921",
                "ui/icon/071000/071152",
                "ui/icon/071000/071081",
                "ui/icon/071000/071142",
                "ui/icon/071000/071145",
                "ui/icon/071000/071155",
                "ui/icon/060000/060779",
                "ui/icon/071000/071032",
                "ui/icon/060000/060758",
                "ui/icon/060000/060446",
                "ui/icon/071000/071005",
                "ui/icon/071000/071015",
                "ui/icon/071000/071001",
                "ui/icon/071000/071011",
                "ui/icon/060000/060512",
                "ui/icon/060000/060447",
                "ui/icon/060000/060545",
                "ui/icon/060000/060505",
                "ui/icon/060000/060422",
                "ui/icon/060000/060495",
                "ui/icon/071000/071001",
                "ui/icon/071000/071004",
        };
    }
}
