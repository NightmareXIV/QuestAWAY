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
        public static bool ImGuiToggleButton(FontAwesomeIcon icon, string tooltip, ref bool flag)
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
            if (ImGuiIconButton(icon, tooltip))
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

        public static Dictionary<string, string> MapIconNames = new Dictionary<string, string>()
        {
                { "ui/icon/060000/060311", "Chocobo porter" },
                { "ui/icon/060000/060314", "" },
                { "ui/icon/060000/060318", "Botanists' guild" },
                { "ui/icon/060000/060319", "Conjurers' guild" },
                { "ui/icon/060000/060320", "Archers' guild" },
                { "ui/icon/060000/060321", "Carpenters' guild" },
                { "ui/icon/060000/060322", "Lancers' guild" },
                { "ui/icon/060000/060326", "Leatherworkers' guild" },
                { "ui/icon/060000/060330", "Arcanists' guild" },
                { "ui/icon/060000/060331", "Mauradeurs' guild" },
                { "ui/icon/060000/060333", "Fishers' guild" },
                { "ui/icon/060000/060334", "Armorers' guild" },
                { "ui/icon/060000/060335", "Culinarians' guild" },
                { "ui/icon/060000/060337", "Blacksmiths' guild" },
                { "ui/icon/060000/060339", "Ferry docks" },
                { "ui/icon/060000/060342", "Gladiators' guild" },
                { "ui/icon/060000/060344", "Thaumaturges' guild" },
                { "ui/icon/060000/060345", "Goldsmiths' guild" },
                { "ui/icon/060000/060346", "Miners' guild" },
                { "ui/icon/060000/060347", "Pugilists' guild" },
                { "ui/icon/060000/060348", "Weavers' guild" },
                { "ui/icon/060000/060352", "Airship landing" },
                { "ui/icon/060000/060362", "Rogues' guild" },
                { "ui/icon/060000/060363", "Astrologians' guild" },
                { "ui/icon/060000/060364", "Machinists' guild" },
                { "ui/icon/060000/060404", "" },
                { "ui/icon/060000/060412", "General purpose merchant" },
                { "ui/icon/060000/060414", "Dungeon" },
                { "ui/icon/060000/060422", "" },
                { "ui/icon/060000/060425", "Retainer bell" },
                { "ui/icon/060000/060426", "Retainer NPC" },
                { "ui/icon/060000/060427", "" },
                { "ui/icon/060000/060428", "Raid" },
                { "ui/icon/060000/060430", "City aetheryte" },
                { "ui/icon/060000/060434", "Repairs" },
                { "ui/icon/060000/060436", "Inn room" },
                { "ui/icon/060000/060442", "Subarea marker" },
                { "ui/icon/060000/060443", "Player marker" },
                { "ui/icon/060000/060446", "Upstairs marker" },
                { "ui/icon/060000/060447", "Downstairs marker" },
                { "ui/icon/060000/060448", "Settlement" },
                { "ui/icon/060000/060449", "Beast tribe marker" },
                { "ui/icon/060000/060450", "Beast tribe marker" },
                { "ui/icon/060000/060451", "Beast tribe marker" },
                { "ui/icon/060000/060453", "Aetheryte" },
                { "ui/icon/060000/060456", "Ferry docks" },
                { "ui/icon/060000/060457", "Next area" },
                { "ui/icon/060000/060458", "Fate NPC" },
                { "ui/icon/060000/060459", "" },
                { "ui/icon/060000/060460", "Free company storage" },
                { "ui/icon/060000/060467", "Gate to next area" },
                { "ui/icon/060000/060473", "Specialist NPC" },
                { "ui/icon/060000/060495", "Quest area marker" },
                { "ui/icon/060000/060496", "Quest area marker" },
                { "ui/icon/060000/060501", "Mobbing fate" },
                { "ui/icon/060000/060502", "Boss fate" },
                { "ui/icon/060000/060503", "Collect items fate" },
                { "ui/icon/060000/060504", "Fate" },
                { "ui/icon/060000/060505", "Fate" },
                { "ui/icon/060000/060506", "Fate" },
                { "ui/icon/060000/060507", "Fate" },
                { "ui/icon/060000/060512", "Additional fate marker" },
                { "ui/icon/060000/060545", "" },
                { "ui/icon/060000/060551", "Delivery moogle" },
                { "ui/icon/060000/060554", "Beast tribe" },
                { "ui/icon/060000/060555", "Beast tribe" },
                { "ui/icon/060000/060561", "Marker placed by user" },
                { "ui/icon/060000/060567", "Grand company: Maelstrom" },
                { "ui/icon/060000/060568", "Grand company: Order of the Twin Adder" },
                { "ui/icon/060000/060569", "Grand company: Immortal Flames" },
                { "ui/icon/060000/060570", "Market board" },
                { "ui/icon/060000/060571", "Hunting log" },
                { "ui/icon/060000/060581", "Weather forecast" },
                { "ui/icon/060000/060582", "" },
                { "ui/icon/060000/060600", "Beast tribe" },
                { "ui/icon/060000/060601", "Beast tribe" },
                { "ui/icon/060000/060603", "Beast tribe" },
                { "ui/icon/060000/060604", "Beast tribe" },
                { "ui/icon/060000/060751", "" },
                { "ui/icon/060000/060752", "" },
                { "ui/icon/060000/060753", "" },
                { "ui/icon/060000/060754", "" },
                { "ui/icon/060000/060755", "" },
                { "ui/icon/060000/060756", "" },
                { "ui/icon/060000/060757", "" },
                { "ui/icon/060000/060758", "" },
                { "ui/icon/060000/060768", "" },
                { "ui/icon/060000/060769", "" },
                { "ui/icon/060000/060770", "" },
                { "ui/icon/060000/060771", "" },
                { "ui/icon/060000/060772", "" },
                { "ui/icon/060000/060773", "" },
                { "ui/icon/060000/060774", "" },
                { "ui/icon/060000/060776", "" },
                { "ui/icon/060000/060779", "" },
                { "ui/icon/060000/060789", "" },
                { "ui/icon/060000/060791", "" },
                { "ui/icon/060000/060910", "Materia melder" },
                { "ui/icon/060000/060926", "" },
                { "ui/icon/060000/060927", "Custom delivery" },
                { "ui/icon/060000/060934", "" },
                { "ui/icon/060000/060935", "" },
                { "ui/icon/060000/060960", "" },
                { "ui/icon/060000/060983", "" },
                { "ui/icon/060000/060986", "" },
                { "ui/icon/060000/060987", "" },
                { "ui/icon/060000/060988", "" },
                { "ui/icon/060000/060993", "" },
                { "ui/icon/061000/061731", "" },
                { "ui/icon/063000/063903", "" },
                { "ui/icon/063000/063905", "" },
                { "ui/icon/063000/063919", "" },
                { "ui/icon/063000/063920", "" },
                { "ui/icon/063000/063921", "" },
                { "ui/icon/063000/063922", "" },
                { "ui/icon/071000/071001", "Main scenario quest" },
                { "ui/icon/071000/071003", "Main scenario quest in progress" },
                { "ui/icon/071000/071004", "Main scenario quest-related mob" },
                { "ui/icon/071000/071005", "Completed main scenario quest" },
                { "ui/icon/071000/071011", "Unavailable main scenario quest" },
                { "ui/icon/071000/071013", "Unavailable main scenario quest in progress" },
                { "ui/icon/071000/071015", "Unavailable completed main scenario quest" },
                { "ui/icon/071000/071021", "Side quest" },
                { "ui/icon/071000/071022", "Repeatable side quest" },
                { "ui/icon/071000/071031", "Unavailable side quest" },
                { "ui/icon/071000/071032", "Unavailable repeatable side quest" },
                { "ui/icon/071000/071041", "" },
                { "ui/icon/071000/071081", "" },
                { "ui/icon/071000/071101", "" },
                { "ui/icon/071000/071102", "" },
                { "ui/icon/071000/071121", "" },
                { "ui/icon/071000/071141", "Key quest" },
                { "ui/icon/071000/071142", "Repeatable key quest" },
                { "ui/icon/071000/071143", "Key quest in progress" },
                { "ui/icon/071000/071145", "Completed key quest" },
                { "ui/icon/071000/071151", "Unavailable key quest" },
                { "ui/icon/071000/071152", "Unavailable repeatable key quest" },
                { "ui/icon/071000/071153", "Unavailable key quest in progress" },
                { "ui/icon/071000/071155", "Unavailable completed key quest" },
        };

        public static SortedSet<string> MapIcons = new SortedSet<string>(MapIconNames.Keys);
    }
}
