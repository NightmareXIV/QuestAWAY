global using static ECommons.GenericHelpers;
global using ECommons.DalamudServices;
global using static QuestAWAY.Static;
global using static QuestAWAY.QuestAWAY;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuestAWAY
{
    public static class Static
    {
        public const uint ActiveToggleButtonColor = 0x6600b500;

        internal static void ImGuiDrawImage(string partialPath)
        {
            try
            {
                if (!P.textures.ContainsKey(partialPath))
                {
                    P.textures[partialPath] = Svc.Texture.GetTextureFromGame(partialPath + ".tex");
                }
                ImGui.Image(P.textures[partialPath].ImGuiHandle, Vector2Scale, Vector2.Zero, Vector2.One, Vector4.One);
            }
            catch (Exception ex)
            {
                Svc.Chat.Print("[QuestAWAY error] " + ex.Message + "\n" + ex.StackTrace);
            }
        }

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
            { "ui/icon/060000/060091", "Gemstone trader" },
            { "ui/icon/060000/060311", "Chocobo porter" },
#region Guild Icons
            { "ui/icon/060000/060314", "Adventurer's guild" },
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
#endregion
            { "ui/icon/060000/060404", "Achievement exchange" },
            { "ui/icon/060000/060412", "General purpose merchant" },
            { "ui/icon/060000/060414", "Dungeon entrance" },
            { "ui/icon/060000/060421", "Party member marker"},
            { "ui/icon/060000/060422", "Aggro enemy marker" },
            { "ui/icon/060000/060425", "Retainer bell" },
            { "ui/icon/060000/060426", "Retainer NPC" },
            { "ui/icon/060000/060427", "Linkshell aquisition" },
            { "ui/icon/060000/060428", "Raid entrance" },
            { "ui/icon/060000/060430", "Aethernet shard" },
            { "ui/icon/060000/060434", "Repairs" },
            { "ui/icon/060000/060436", "Inn room" },
            { "ui/icon/060000/060441", "Adjoining area marker" },
            { "ui/icon/060000/060442", "Subarea marker" },
            { "ui/icon/060000/060443", "Player marker" },
            { "ui/icon/060000/060446", "Upstairs marker" },
            { "ui/icon/060000/060447", "Downstairs marker" },
            { "ui/icon/060000/060448", "Settlement" },
            { "ui/icon/060000/060449", "Amalj'aa Stronghold" },
            { "ui/icon/060000/060450", "Ixali Stronghold" },
            { "ui/icon/060000/060451", "Kobold Stronghold" },
            { "ui/icon/060000/060453", "Aetheryte" },
            { "ui/icon/060000/060456", "Ferry docks" },
            { "ui/icon/060000/060457", "Next area" },
            { "ui/icon/060000/060458", "FATE NPC" },
            { "ui/icon/060000/060459", "Dueling Area" },
            { "ui/icon/060000/060460", "Free Company chest" },
            { "ui/icon/060000/060467", "Gate to next area" },
            { "ui/icon/060000/060473", "Specialist NPC" },
            { "ui/icon/060000/060495", "Quest area marker" },
            { "ui/icon/060000/060496", "Quest area marker" },
#region FATE Icons
            { "ui/icon/060000/060501", "Mobbing FATE" },
            { "ui/icon/060000/060502", "Boss FATE" },
            { "ui/icon/060000/060503", "Collect items FATE" },
            { "ui/icon/060000/060504", "Defense FATE" },
            { "ui/icon/060000/060505", "Special/Escort FATE" },
            { "ui/icon/060000/060506", "Chase FATE" },
            { "ui/icon/060000/060507", "Chase FATE" },
#endregion
#region Area extra markers
            { "ui/icon/060000/060541", "Small area above player marker" },
            { "ui/icon/060000/060542", "Medium area above player marker" },
            { "ui/icon/060000/060543", "Large area above player marker" },
            { "ui/icon/060000/060545", "Small area below player marker" },
            { "ui/icon/060000/060546", "Medium area below player marker" },
            { "ui/icon/060000/060547", "Large Area below player marker" },
#endregion
            { "ui/icon/060000/060551", "Delivery Moogle" },
            { "ui/icon/060000/060554", "Sahagin Stronghold" },
            { "ui/icon/060000/060555", "Sylph Stronghold" },
#region Grand company
            { "ui/icon/060000/060567", "Grand company: Maelstrom" },
            { "ui/icon/060000/060568", "Grand company: Order of the Twin Adder" },
            { "ui/icon/060000/060569", "Grand company: Immortal Flames" },
#endregion
            { "ui/icon/060000/060570", "Market board" },
            { "ui/icon/060000/060571", "Hunting log" },
            { "ui/icon/060000/060581", "Weather forecast" },
            { "ui/icon/060000/060582", "Gold Saucer employee" },
            { "ui/icon/060000/060600", "Vundu/Gundu Stronghold" },
            { "ui/icon/060000/060601", "Gnath/Loth Stronghold" },
            { "ui/icon/060000/060603", "Qalyana Stronghold" },
            { "ui/icon/060000/060604", "Red Stronghold" },
#region FC Housing Icons
            { "ui/icon/060000/060751", "Free Company cottage - locked" },
            { "ui/icon/060000/060752", "Free Company house - locked" },
            { "ui/icon/060000/060753", "Free Company mansion - locked" },
            { "ui/icon/060000/060754", "Free Company cottage" },
            { "ui/icon/060000/060755", "Free Company house" },
            { "ui/icon/060000/060756", "Free Company mansion" },
            { "ui/icon/060000/060757", "Free Company housing lot purchased - awaiting Permit" },
#endregion
            { "ui/icon/060000/060758", "Housing lot available for purchase" },
#region User FC Housing Icons
            { "ui/icon/060000/060761", "User Free Company cottage - locked" },
            { "ui/icon/060000/060762", "User Free Company house - locked" },
            { "ui/icon/060000/060763", "User Free Company mansion - locked" },
            { "ui/icon/060000/060764", "User Free Company cottage" },
            { "ui/icon/060000/060765", "User Free Company house" },
            { "ui/icon/060000/060766", "User Free Company mansion" },
            { "ui/icon/060000/060767", "User Free Company housing lot purchased - awaiting Permit" },
#endregion
            { "ui/icon/060000/060768", "Resident caretaker" },
#region Individual Housing Icons
            { "ui/icon/060000/060769", "Individual cottage - locked" },
            { "ui/icon/060000/060770", "Individual house - locked" },
            { "ui/icon/060000/060771", "Individual mansion - locked" },
            { "ui/icon/060000/060772", "Individual cottage" },
            { "ui/icon/060000/060773", "Individual house" },
            { "ui/icon/060000/060774", "Individual mansion" },
            { "ui/icon/060000/060775", "Housing lot purchased - awaiting Permit" },
#endregion
#region User Individual Housing Icons
            { "ui/icon/060000/060776", "User Individual cottage - locked" },
            { "ui/icon/060000/060777", "User Individual house - locked" },
            { "ui/icon/060000/060778", "User Individual mansion - locked" },
            { "ui/icon/060000/060779", "User Individual cottage" },
            { "ui/icon/060000/060780", "User Individual house" },
            { "ui/icon/060000/060781", "User Individual mansion" },
            { "ui/icon/060000/060782", "User housing lot purchased - awaiting Permit" },
#endregion
#region Shared Individual Housing Icons
            { "ui/icon/060000/060783", "Shared Individual cottage - locked" },
            { "ui/icon/060000/060784", "Shared Individual house - locked" },
            { "ui/icon/060000/060785", "Shared Individual mansion - locked" },
            { "ui/icon/060000/060786", "Shared Individual cottage" },
            { "ui/icon/060000/060787", "Shared Individual house" },
            { "ui/icon/060000/060788", "Shared Individual mansion" },
#endregion
#region Apartment Icons
            { "ui/icon/060000/060789", "Apartment building" },
            { "ui/icon/060000/060791", "Rented apartment building" },
#endregion
#region Deep Dungeon
            { "ui/icon/060000/060905", "Cairn of return - inactive" },
            { "ui/icon/060000/060906", "Cairn of return - active" },
            { "ui/icon/060000/060907", "Cairn of passage - inactive" },
            { "ui/icon/060000/060908", "Cairn of passage - active" },
#endregion
            { "ui/icon/060000/060910", "Materia melder" },
            { "ui/icon/060000/060926", "Wonderous tails" },
            { "ui/icon/060000/060927", "Custom delivery" },
            { "ui/icon/060000/060934", "EXP Bonus" },
            { "ui/icon/060000/060935", "Exchange NPC" },
            { "ui/icon/060000/060958", "Notorious monster - Eureka" },
            { "ui/icon/060000/060959", "Aetheryte - Eureka" },
            { "ui/icon/060000/060960", "Masked Rose" },
            { "ui/icon/060000/060968", "Magia melder" },
            { "ui/icon/060000/060969", "Donation basket" },
            { "ui/icon/060000/060971", "Exit - Eureka" },
            { "ui/icon/060000/060983", "Blue Mage activity" },
            { "ui/icon/060000/060986", "Crystarum deliveries" },
            { "ui/icon/060000/060987", "Gemstone trader" },
            { "ui/icon/060000/060988", "" }, // Second inn style maybe island sanctuary?
            { "ui/icon/060000/060993", "Skybuilder's board" },
            { "ui/icon/061000/061731", "Doman Reconstruction/Eureka Expedition" },
            { "ui/icon/063000/063903", "Kupo of fortune" },
            { "ui/icon/063000/063905", "Ocean fishing" },
            { "ui/icon/063000/063907", "Windmire" },
            { "ui/icon/063000/063919", "Sundry splendors" },
            { "ui/icon/063000/063920", "Faux hollows" },
            { "ui/icon/063000/063921", "Lore quest" },
            { "ui/icon/063000/063922", "Itinerant Moogle" },
            { "ui/icon/063000/063932", "Studium delivery" },
            { "ui/icon/063000/063933", "Dialog interaction" },
            { "ui/icon/063000/063934", "Unavailable studium delivery" },
            { "ui/icon/063000/063970", "Island sanctuary ferry" },
            { "ui/icon/063000/063971", "Deep-Dungeon entry" },
            { "ui/icon/063000/063972", "The Forbidden Land, Eureka" },
#region Island Icons
            { "ui/icon/063000/063964", "Island logs" },
            { "ui/icon/063000/063963", "Island leafs" },
            { "ui/icon/063000/063965", "Island crystals" },
            { "ui/icon/063000/063966", "Island crops" },
#endregion
#region Linked Quest Icons
            { "ui/icon/070000/070961", "Linked main scenario quest" },
            { "ui/icon/070000/070962", "Unavailable linked main scenario quest" },
            { "ui/icon/070000/070963", "Linked main scenario quest in progress" },
            { "ui/icon/070000/070964", "Unavailable linked main scenario quest in progress" },
            { "ui/icon/070000/070965", "Linked side quest" },
            { "ui/icon/070000/070966", "Unavailable linked side quest" },
            { "ui/icon/070000/070967", "Linked repeatable side quest" },
            { "ui/icon/070000/070968", "Unavailable linked repeatable side quest" },
            { "ui/icon/070000/070969", "Linked side quest in progress" },
            { "ui/icon/070000/070970", "Unavailable linked side quest in progress" },
            { "ui/icon/070000/070971", "Linked key quest" },
            { "ui/icon/070000/070972", "Unavailable linked key quest" },
            { "ui/icon/070000/070973", "Linked repeatable key quest" },
            { "ui/icon/070000/070974", "Unavailable linked repeatable key quest" },
            { "ui/icon/070000/070975", "Linked key quest in progress" },
            { "ui/icon/070000/070976", "Unavailable linked key quest in progress" },
#endregion
#region MSQ Icons
            { "ui/icon/071000/071001", "Main scenario quest" },
            { "ui/icon/071000/071002", "Repeatable main scenario quest" },
            { "ui/icon/071000/071003", "Main scenario quest in progress" },
            { "ui/icon/071000/071004", "Main scenario quest-related mob" },
            { "ui/icon/071000/071005", "Completed main scenario quest" },
            { "ui/icon/071000/071006", "Main scenario quest interaction" },
            { "ui/icon/071000/071011", "Unavailable main scenario quest" },
            { "ui/icon/071000/071012", "Unavailable repeatable main scenario quest" },
            { "ui/icon/071000/071013", "Unavailable main scenario quest in progress" },
            { "ui/icon/071000/071015", "Unavailable completed main scenario quest" },
            { "ui/icon/071000/071016", "Unavailable main scenario quest interaction" },
#endregion
#region Quest Icons
            { "ui/icon/071000/071021", "Side quest" },
            { "ui/icon/071000/071022", "Repeatable side quest" },
            { "ui/icon/071000/071023", "Side quest in progress" },
            { "ui/icon/071000/071024", "Side quest-related mob" },
            { "ui/icon/071000/071025", "Completed side quest" },
            { "ui/icon/071000/071026", "Side quest interaction" },
            { "ui/icon/071000/071031", "Unavailable side quest" },
            { "ui/icon/071000/071032", "Unavailable repeatable side quest" },
            { "ui/icon/071000/071033", "Unavailable side quest in progress" },
            { "ui/icon/071000/071034", "Unavailable side quest-related mob" },
            { "ui/icon/071000/071035", "Unavailable completed side quest" },
            { "ui/icon/071000/071036", "Side quest interaction" },
#endregion
#region Levemete Icons
            { "ui/icon/071000/071041", "Levemete" },
            { "ui/icon/071000/071042", "Repeatable Levemete" },
            { "ui/icon/071000/071043", "Levemete in progress" },
            { "ui/icon/071000/071044", "Levemete related marker" },
            { "ui/icon/071000/071045", "Completed Levemete" },
            { "ui/icon/071000/071046", "Levemete interaction" },
            { "ui/icon/071000/071051", "Unavailable Levemete" },
            { "ui/icon/071000/071052", "Unavailable repeatable Levemete" },
            { "ui/icon/071000/071053", "Unavailable Levemete in progress" },
            { "ui/icon/071000/071054", "Unavailable Levemete related marker" },
            { "ui/icon/071000/071055", "Unavailable completed Levemete" },
            { "ui/icon/071000/071056", "Unavailable Levemete interaction" },
#endregion
#region Lore Icons
            { "ui/icon/071000/071061", "Lore quest" },
            { "ui/icon/071000/071062", "Repeatable lore quest" },
            { "ui/icon/071000/071063", "Lore quest in progress" },
            { "ui/icon/071000/071064", "Lore quest related marker" },
            { "ui/icon/071000/071065", "Completed lore quest" },
            { "ui/icon/071000/071066", "Lore interaction" },
            { "ui/icon/071000/071071", "Unavailable lore quest" },
            { "ui/icon/071000/071072", "Unavailable repeatable lore quest" },
            { "ui/icon/071000/071073", "Unavailable lore quest in progress" },
            { "ui/icon/071000/071074", "Unavailable lore quest related marker" },
            { "ui/icon/071000/071075", "Unavailable completed lore quest" },
            { "ui/icon/071000/071076", "Unavailable lore interaction" },
#endregion
#region Behest Icons
            { "ui/icon/071000/071081", "Behest" },
            { "ui/icon/071000/071082", "Repeatable Behest" },
            { "ui/icon/071000/071083", "Behest in progress" },
            { "ui/icon/071000/071084", "Behest related marker" },
            { "ui/icon/071000/071085", "Completed Behest" },
            { "ui/icon/071000/071086", "Behest interaction" },
            { "ui/icon/071000/071091", "Unavailable Behest" },
            { "ui/icon/071000/071092", "Unavailable repeatable Behest" },
            { "ui/icon/071000/071093", "Unavailable Behest in progress" },
            { "ui/icon/071000/071094", "Unavailable Behest related marker" },
            { "ui/icon/071000/071095", "Unavailable completed Behest" },
            { "ui/icon/071000/071096", "Unavailable Behest interaction" },
#endregion
#region Triple Triad Icons
            { "ui/icon/071000/071101", "Triple Triad match" },
            { "ui/icon/071000/071102", "Triple Triad match - won" },
#endregion
#region GATE Icons
            { "ui/icon/071000/071111", "GATE" },
            { "ui/icon/071000/071112", "Completed GATE" },
#endregion
#region Beginner quest Icons
            { "ui/icon/071000/071121", "Beginner quest" },
            { "ui/icon/071000/071122", "Repeatable beginner quest" },
            { "ui/icon/071000/071123", "Beginner quest in progress" },
            { "ui/icon/071000/071124", "Beginner quest related marker" },
            { "ui/icon/071000/071125", "Completed beginner quest" },
            { "ui/icon/071000/071126", "Beginner quest interaction" },
            { "ui/icon/071000/071131", "Unavailable beginner quest" },
            { "ui/icon/071000/071132", "Unavailable repeatable beginner quest" },
            { "ui/icon/071000/071133", "Unavailable beginner quest in progress" },
            { "ui/icon/071000/071134", "Unavailable beginner quest related marker" },
            { "ui/icon/071000/071135", "Unavailable completed beginner quest" },
            { "ui/icon/071000/071136", "Unavailable beginner quest interaction" },
#endregion
#region Feature quest Icons
            { "ui/icon/071000/071141", "Key quest" },
            { "ui/icon/071000/071142", "Repeatable key quest" },
            { "ui/icon/071000/071143", "Key quest in progress" },
            { "ui/icon/071000/071145", "Completed key quest" },
            { "ui/icon/071000/071146", "Key quest interaction" },
            { "ui/icon/071000/071151", "Unavailable key quest" },
            { "ui/icon/071000/071152", "Unavailable repeatable key quest" },
            { "ui/icon/071000/071153", "Unavailable key quest in progress" },
            { "ui/icon/071000/071155", "Unavailable completed key quest" },
            { "ui/icon/071000/071156", "Unavailable key quest interaction" },
#endregion
        };

        public static SortedSet<string> MapIcons = new SortedSet<string>(MapIconNames.Keys);
    }
}
