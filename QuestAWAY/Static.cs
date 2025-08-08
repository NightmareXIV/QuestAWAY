global using ECommons.DalamudServices;
global using static ECommons.GenericHelpers;
global using static QuestAWAY.QuestAWAY;
global using static QuestAWAY.Static;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
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
                var texture = Svc.Texture.GetFromGame(partialPath + ".tex").GetWrapOrEmpty();

                ImGui.Image(texture.Handle, Vector2Scale, Vector2.Zero, Vector2.One, Vector4.One);
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
            {
                ImGuiTextTooltip(tooltip);
            }


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

        public enum Category
        {
            All, // This is a pseudo category and it shouldnt be assigned to an icon
            AlliedSociety,
            AreaMarkers,
            BeginnerQuest,
            Behest,
            DungeonRaid,
            FATE,
            FCHousing,
            FeatureQuest,
            GATE,
            Guild,
            Housing,
            Island,
            Levemete,
            LinkedQuest,
            Lore,
            MiniMapOnly,
            Miscellanous,
            MSQ,
            NPC,
            QuasiQuest,
            Quest,
            TripleTriad,
        }

        public static Dictionary<Category, string> CategoryNames = new()
        {
            { Category.All, "All" }, // This is a pseudo category and it shouldnt be assigned to an icon
            { Category.AlliedSociety, "Allied Society Markers" },
            { Category.AreaMarkers, "Area Markers" },
            { Category.BeginnerQuest, "Beginner Quest Markers" },
            { Category.Behest, "Behest Markers" },
            { Category.DungeonRaid, "Dungeon/Trial/Raid Markers" },
            { Category.FATE, "FATE Markers" },
            { Category.FCHousing, "FC Housing Markers" },
            { Category.FeatureQuest, "Feature Quest Markers" },
            { Category.GATE, "GATE Markers" },
            { Category.Guild, "Guild Markers" },
            { Category.Housing, "Housing Markers" },
            { Category.Island, "Island Markers" },
            { Category.Levemete, "Levemete Markers" },
            { Category.LinkedQuest, "Linked Quest Markers" },
            { Category.Lore, "Lore Markers" },
            { Category.MiniMapOnly, "MiniMap Only Markers" },
            { Category.Miscellanous, "Miscellanous Markers" },
            { Category.MSQ, "MSQ Markers" },
            { Category.NPC, "NPC Markers" },
            { Category.QuasiQuest, "Quasi-Quest Markers" },
            { Category.Quest, "Quest Markers" },
            { Category.TripleTriad, "Triple Triad Markers" },
        };

        public static Dictionary<string, (string Name, Category Category)> MapIconData = new()
        {
            { "ui/icon/060000/060091", ("Gemstone trader", Category.NPC) },
            { "ui/icon/060000/060311", ("Chocobo porter", Category.NPC) },
            #region Guild Icons I
            { "ui/icon/060000/060314", ("Adventurers' guild", Category.Guild) },
            { "ui/icon/060000/060318", ("Botanists' guild", Category.Guild) },
            { "ui/icon/060000/060319", ("Conjurers' guild", Category.Guild) },
            { "ui/icon/060000/060320", ("Archers' guild", Category.Guild) },
            { "ui/icon/060000/060321", ("Carpenters' guild", Category.Guild) },
            { "ui/icon/060000/060322", ("Lancers' guild", Category.Guild) },
            { "ui/icon/060000/060326", ("Leatherworkers' guild", Category.Guild) },
            { "ui/icon/060000/060330", ("Arcanists' guild", Category.Guild) },
            { "ui/icon/060000/060331", ("Mauradeurs' guild", Category.Guild) },
            { "ui/icon/060000/060333", ("Fishers' guild", Category.Guild) },
            { "ui/icon/060000/060334", ("Armorers' guild", Category.Guild) },
            { "ui/icon/060000/060335", ("Culinarians' guild", Category.Guild) },
            { "ui/icon/060000/060337", ("Blacksmiths' guild", Category.Guild) },
            #endregion
            { "ui/icon/060000/060339", ("Ferry docks", Category.AreaMarkers) },
            #region Guild Icons II
            { "ui/icon/060000/060342", ("Gladiators' guild", Category.Guild) },
            { "ui/icon/060000/060344", ("Thaumaturges' guild", Category.Guild) },
            { "ui/icon/060000/060345", ("Goldsmiths' guild", Category.Guild) },
            { "ui/icon/060000/060346", ("Miners' guild", Category.Guild) },
            { "ui/icon/060000/060347", ("Pugilists' guild", Category.Guild) },
            { "ui/icon/060000/060348", ("Weavers' guild", Category.Guild) },
            { "ui/icon/060000/060351", ("Alchemists' guild", Category.Guild) },
            #endregion
            { "ui/icon/060000/060352", ("Airship landing", Category.AreaMarkers) },
            #region Guild Icons III
            { "ui/icon/060000/060364", ("Machinists' guild", Category.Guild) },
            { "ui/icon/060000/060362", ("Rogues' guild", Category.Guild) },
            { "ui/icon/060000/060363", ("Astrologians' guild", Category.Guild) },
            #endregion
            { "ui/icon/060000/060401", ("Aggro boss marker", Category.MiniMapOnly) },
            { "ui/icon/060000/060402", ("Boss marker", Category.MiniMapOnly) },
            { "ui/icon/060000/060404", ("Achievement exchange", Category.NPC) },
            { "ui/icon/060000/060412", ("General purpose merchant", Category.NPC) },
            { "ui/icon/060000/060414", ("Dungeon entrance", Category.DungeonRaid) },
            { "ui/icon/060000/060421", ("Party member marker", Category.MiniMapOnly) },
            { "ui/icon/060000/060422", ("Aggro enemy marker", Category.MiniMapOnly) },
            { "ui/icon/060000/060424", ("Enemy marker", Category.MiniMapOnly) },
            { "ui/icon/060000/060425", ("Retainer bell", Category.NPC) },
            { "ui/icon/060000/060426", ("Retainer NPC", Category.NPC) },
            { "ui/icon/060000/060427", ("Linkshell aquisition", Category.NPC) },
            { "ui/icon/060000/060428", ("Raid entrance", Category.DungeonRaid) },
            { "ui/icon/060000/060430", ("Aethernet shard", Category.Miscellanous) },
            { "ui/icon/060000/060434", ("Repairs", Category.NPC) },
            { "ui/icon/060000/060436", ("Inn room", Category.Miscellanous) },
            { "ui/icon/060000/060441", ("Adjoining area marker", Category.AreaMarkers) },
            { "ui/icon/060000/060442", ("Subarea marker", Category.AreaMarkers) },
            { "ui/icon/060000/060443", ("Player marker", Category.Miscellanous) },
            { "ui/icon/060000/060446", ("Upstairs marker", Category.AreaMarkers) },
            { "ui/icon/060000/060447", ("Downstairs marker", Category.AreaMarkers) },
            { "ui/icon/060000/060448", ("Settlement", Category.AreaMarkers) },
            #region Beast Tribe Strongholds I
            { "ui/icon/060000/060449", ("Amalj'aa Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060450", ("Ixali Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060451", ("Kobold Stronghold", Category.AlliedSociety) },
            #endregion
            { "ui/icon/060000/060453", ("Aetheryte", Category.AreaMarkers) },
            { "ui/icon/060000/060456", ("Ferry docks", Category.AreaMarkers) },
            { "ui/icon/060000/060457", ("Next area", Category.AreaMarkers) },
            { "ui/icon/060000/060458", ("FATE NPC", Category.FATE) },
            { "ui/icon/060000/060459", ("Dueling Area", Category.AreaMarkers) },
            { "ui/icon/060000/060460", ("Free Company chest", Category.Miscellanous) },
            { "ui/icon/060000/060467", ("Gate to next area", Category.AreaMarkers) },
            { "ui/icon/060000/060473", ("Specialist NPC", Category.NPC) },
            { "ui/icon/060000/060495", ("Quest area marker", Category.Quest) },
            { "ui/icon/060000/060496", ("Quest area marker", Category.Quest) },
            #region FATE Icons
            { "ui/icon/060000/060501", ("Mobbing FATE", Category.FATE) },
            { "ui/icon/060000/060502", ("Boss FATE", Category.FATE) },
            { "ui/icon/060000/060503", ("Collect items FATE", Category.FATE) },
            { "ui/icon/060000/060504", ("Defense FATE", Category.FATE) },
            { "ui/icon/060000/060505", ("Special/Escort FATE", Category.FATE) },
            { "ui/icon/060000/060506", ("Chase FATE", Category.FATE) },
            { "ui/icon/060000/060507", ("Chase FATE", Category.FATE) },
            #endregion
            #region Area extra markers
            { "ui/icon/060000/060541", ("Small area above player marker", Category.AreaMarkers) },
            { "ui/icon/060000/060542", ("Medium area above player marker", Category.AreaMarkers) },
            { "ui/icon/060000/060543", ("Large area above player marker", Category.AreaMarkers) },
            { "ui/icon/060000/060545", ("Small area below player marker", Category.AreaMarkers) },
            { "ui/icon/060000/060546", ("Medium area below player marker", Category.AreaMarkers) },
            { "ui/icon/060000/060547", ("Large Area below player marker", Category.AreaMarkers) },
            #endregion
            #region Beast Tribe Strongholds II
            { "ui/icon/060000/060551", ("Delivery Moogle", Category.AlliedSociety) },
            { "ui/icon/060000/060554", ("Sahagin Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060555", ("Sylph Stronghold", Category.AlliedSociety) },
            #endregion
            #region Grand company
            { "ui/icon/060000/060567", ("Grand company: Maelstrom", Category.Miscellanous) },
            { "ui/icon/060000/060568", ("Grand company: Order of the Twin Adder", Category.Miscellanous) },
            { "ui/icon/060000/060569", ("Grand company: Immortal Flames", Category.Miscellanous) },
            #endregion
            { "ui/icon/060000/060570", ("Market board", Category.Miscellanous) },
            { "ui/icon/060000/060571", ("Hunting log", Category.Miscellanous) },
            { "ui/icon/060000/060581", ("Weather forecast", Category.NPC) },
            { "ui/icon/060000/060582", ("Gold Saucer employee", Category.NPC) },
            #region Beast Tribe Strongholds III
            { "ui/icon/060000/060600", ("Vundu/Gundu Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060601", ("Gnath/Loth Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060603", ("Qalyana Stronghold", Category.AlliedSociety) },
            { "ui/icon/060000/060604", ("Red Stronghold", Category.AlliedSociety) },
            #endregion
            #region Housing Icons
            #region FC Housing Icons
            { "ui/icon/060000/060751", ("Free Company cottage - locked", Category.FCHousing) },
            { "ui/icon/060000/060752", ("Free Company house - locked", Category.FCHousing) },
            { "ui/icon/060000/060753", ("Free Company mansion - locked", Category.FCHousing) },
            { "ui/icon/060000/060754", ("Free Company cottage", Category.FCHousing) },
            { "ui/icon/060000/060755", ("Free Company house", Category.FCHousing) },
            { "ui/icon/060000/060756", ("Free Company mansion", Category.FCHousing) },
            { "ui/icon/060000/060757", ("Free Company housing lot purchased - awaiting Permit", Category.FCHousing) },
            #endregion
            { "ui/icon/060000/060758", ("Housing lot available for purchase", Category.Housing) },
            #region User FC Housing Icons
            { "ui/icon/060000/060761", ("User Free Company cottage - locked", Category.FCHousing) },
            { "ui/icon/060000/060762", ("User Free Company house - locked", Category.FCHousing) },
            { "ui/icon/060000/060763", ("User Free Company mansion - locked", Category.FCHousing) },
            { "ui/icon/060000/060764", ("User Free Company cottage", Category.FCHousing) },
            { "ui/icon/060000/060765", ("User Free Company house", Category.FCHousing) },
            { "ui/icon/060000/060766", ("User Free Company mansion", Category.FCHousing) },
            { "ui/icon/060000/060767", ("User Free Company housing lot purchased - awaiting Permit", Category.FCHousing) },
            #endregion
            { "ui/icon/060000/060768", ("Resident caretaker", Category.NPC) },
            #region Individual Housing Icons
            { "ui/icon/060000/060769", ("Individual cottage - locked", Category.Housing) },
            { "ui/icon/060000/060770", ("Individual house - locked", Category.Housing) },
            { "ui/icon/060000/060771", ("Individual mansion - locked", Category.Housing) },
            { "ui/icon/060000/060772", ("Individual cottage", Category.Housing) },
            { "ui/icon/060000/060773", ("Individual house", Category.Housing) },
            { "ui/icon/060000/060774", ("Individual mansion", Category.Housing) },
            { "ui/icon/060000/060775", ("Housing lot purchased - awaiting Permit", Category.Housing) },
            #endregion
            #region User Individual Housing Icons
            { "ui/icon/060000/060776", ("User Individual cottage - locked", Category.Housing) },
            { "ui/icon/060000/060777", ("User Individual house - locked", Category.Housing) },
            { "ui/icon/060000/060778", ("User Individual mansion - locked", Category.Housing) },
            { "ui/icon/060000/060779", ("User Individual cottage", Category.Housing) },
            { "ui/icon/060000/060780", ("User Individual house", Category.Housing) },
            { "ui/icon/060000/060781", ("User Individual mansion", Category.Housing) },
            { "ui/icon/060000/060782", ("User housing lot purchased - awaiting Permit", Category.Housing) },
            #endregion
            #region Shared Individual Housing Icons
            { "ui/icon/060000/060783", ("Shared Individual cottage - locked", Category.Housing) },
            { "ui/icon/060000/060784", ("Shared Individual house - locked", Category.Housing) },
            { "ui/icon/060000/060785", ("Shared Individual mansion - locked", Category.Housing) },
            { "ui/icon/060000/060786", ("Shared Individual cottage", Category.Housing) },
            { "ui/icon/060000/060787", ("Shared Individual house", Category.Housing) },
            { "ui/icon/060000/060788", ("Shared Individual mansion", Category.Housing) },
            #endregion
            #region Apartment Icons
            { "ui/icon/060000/060789", ("Apartment building", Category.Housing) },
            { "ui/icon/060000/060791", ("Rented apartment building", Category.Housing) },
            #endregion
            #endregion
            #region Deep Dungeon
            { "ui/icon/060000/060905", ("Cairn of return - inactive", Category.DungeonRaid) },
            { "ui/icon/060000/060906", ("Cairn of return - active", Category.DungeonRaid) },
            { "ui/icon/060000/060907", ("Cairn of passage - inactive", Category.DungeonRaid) },
            { "ui/icon/060000/060908", ("Cairn of passage - active", Category.DungeonRaid) },
            #endregion
            { "ui/icon/060000/060910", ("Materia melder", Category.NPC) },
            { "ui/icon/060000/060926", ("Wonderous tails", Category.NPC) },
            { "ui/icon/060000/060927", ("Custom delivery", Category.NPC) },
            { "ui/icon/060000/060934", ("FATE Bonus", Category.FATE) },
            { "ui/icon/060000/060935", ("Exchange NPC", Category.NPC) },
            { "ui/icon/060000/060958", ("Notorious monster - Eureka", Category.FATE) },
            { "ui/icon/060000/060959", ("Aetheryte - Eureka", Category.Miscellanous) },
            { "ui/icon/060000/060960", ("Masked Rose", Category.NPC) },
            { "ui/icon/060000/060961", ("Chocobo Companion", Category.Miscellanous) },
            { "ui/icon/060000/060968", ("Magia melder", Category.Miscellanous) },
            { "ui/icon/060000/060969", ("Donation basket", Category.Miscellanous) },
            { "ui/icon/060000/060971", ("Field operation exit", Category.AreaMarkers) },
            { "ui/icon/060000/060983", ("Blue Mage activity", Category.NPC) },
            { "ui/icon/060000/060986", ("Crystarum deliveries", Category.NPC) },
            { "ui/icon/060000/060987", ("Gemstone trader", Category.NPC) },
            { "ui/icon/060000/060988", ("", Category.Miscellanous) }, // Second inn style maybe island sanctuary?
            { "ui/icon/060000/060993", ("Skybuilders' board", Category.Miscellanous) },
            #region Quasi-Quests
            { "ui/icon/061000/061731", ("Quasi-quest", Category.QuasiQuest) },
            { "ui/icon/061000/061732", ("Unavailable quasi-quest", Category.QuasiQuest) },
            { "ui/icon/061000/061733", ("Key quasi-quest", Category.QuasiQuest) },
            #endregion
            { "ui/icon/063000/063903", ("Kupo of fortune", Category.NPC) },
            { "ui/icon/063000/063905", ("Ocean fishing", Category.Miscellanous) },
            { "ui/icon/063000/063906", ("Ocean fishing - Active", Category.Miscellanous) },
            { "ui/icon/063000/063907", ("Windmire", Category.AreaMarkers) },
            { "ui/icon/063000/063919", ("Sundry splendors", Category.NPC) },
            { "ui/icon/063000/063920", ("Faux hollows", Category.NPC) },
            { "ui/icon/063000/063921", ("Lore quest", Category.QuasiQuest) },
            { "ui/icon/063000/063922", ("Itinerant Moogle", Category.NPC) },
            { "ui/icon/063000/063932", ("Studium delivery", Category.NPC) },
            { "ui/icon/063000/063933", ("Dialog interaction", Category.QuasiQuest) },
            { "ui/icon/063000/063934", ("Unavailable studium delivery", Category.NPC) },
            { "ui/icon/063000/063970", ("Island sanctuary ferry", Category.Island) },
            { "ui/icon/063000/063971", ("Deep-dungeon entrance", Category.DungeonRaid) },
            { "ui/icon/063000/063972", ("Field operation entrance", Category.DungeonRaid) },
            { "ui/icon/063000/063973", ("Variant dungeon entrance", Category.DungeonRaid) },
            #region Island Icons
            { "ui/icon/063000/063964", ("Island logs", Category.Island) },
            { "ui/icon/063000/063963", ("Island leafs", Category.Island) },
            { "ui/icon/063000/063965", ("Island crystals", Category.Island) },
            { "ui/icon/063000/063966", ("Island crops", Category.Island) },
            #endregion
            #region Quest Icons
            #region Linked Quest Icons
            { "ui/icon/070000/070961", ("Linked main scenario quest", Category.LinkedQuest) },
            { "ui/icon/070000/070962", ("Unavailable linked main scenario quest", Category.LinkedQuest) },
            { "ui/icon/070000/070963", ("Linked main scenario quest in progress", Category.LinkedQuest) },
            { "ui/icon/070000/070964", ("Unavailable linked main scenario quest in progress", Category.LinkedQuest) },
            { "ui/icon/070000/070965", ("Linked side quest", Category.LinkedQuest) },
            { "ui/icon/070000/070966", ("Unavailable linked side quest", Category.LinkedQuest) },
            { "ui/icon/070000/070967", ("Linked repeatable side quest", Category.LinkedQuest) },
            { "ui/icon/070000/070968", ("Unavailable linked repeatable side quest", Category.LinkedQuest) },
            { "ui/icon/070000/070969", ("Linked side quest in progress", Category.LinkedQuest) },
            { "ui/icon/070000/070970", ("Unavailable linked side quest in progress", Category.LinkedQuest) },
            { "ui/icon/070000/070971", ("Linked key quest", Category.LinkedQuest) },
            { "ui/icon/070000/070972", ("Unavailable linked key quest", Category.LinkedQuest) },
            { "ui/icon/070000/070973", ("Linked repeatable key quest", Category.LinkedQuest) },
            { "ui/icon/070000/070974", ("Unavailable linked repeatable key quest", Category.LinkedQuest) },
            { "ui/icon/070000/070975", ("Linked key quest in progress", Category.LinkedQuest) },
            { "ui/icon/070000/070976", ("Unavailable linked key quest in progress", Category.LinkedQuest) },
            #endregion
            #region MSQ Icons
            { "ui/icon/071000/071001", ("Main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071002", ("Repeatable main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071003", ("Main scenario quest in progress", Category.MSQ) },
            { "ui/icon/071000/071004", ("Main scenario quest-related mob", Category.MSQ) },
            { "ui/icon/071000/071005", ("Completed main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071006", ("Main scenario quest interaction", Category.MSQ) },
            { "ui/icon/071000/071011", ("Unavailable main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071012", ("Unavailable repeatable main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071013", ("Unavailable main scenario quest in progress", Category.MSQ) },
            { "ui/icon/071000/071015", ("Unavailable completed main scenario quest", Category.MSQ) },
            { "ui/icon/071000/071016", ("Unavailable main scenario quest interaction", Category.MSQ) },
            #endregion
            #region Quest Icons
            { "ui/icon/071000/071021", ("Side quest", Category.Quest) },
            { "ui/icon/071000/071022", ("Repeatable side quest", Category.Quest) },
            { "ui/icon/071000/071023", ("Side quest in progress", Category.Quest) },
            { "ui/icon/071000/071024", ("Side quest-related mob", Category.Quest) },
            { "ui/icon/071000/071025", ("Completed side quest", Category.Quest) },
            { "ui/icon/071000/071026", ("Side quest interaction", Category.Quest) },
            { "ui/icon/071000/071031", ("Unavailable side quest", Category.Quest) },
            { "ui/icon/071000/071032", ("Unavailable repeatable side quest", Category.Quest) },
            { "ui/icon/071000/071033", ("Unavailable side quest in progress", Category.Quest) },
            { "ui/icon/071000/071034", ("Unavailable side quest-related mob", Category.Quest) },
            { "ui/icon/071000/071035", ("Unavailable completed side quest", Category.Quest) },
            { "ui/icon/071000/071036", ("Side quest interaction", Category.Quest) },
            #endregion
            #region Levemete Icons
            { "ui/icon/071000/071041", ("Levemete", Category.Levemete) },
            { "ui/icon/071000/071042", ("Repeatable Levemete", Category.Levemete) },
            { "ui/icon/071000/071043", ("Levemete in progress", Category.Levemete) },
            { "ui/icon/071000/071044", ("Levemete related marker", Category.Levemete) },
            { "ui/icon/071000/071045", ("Completed Levemete", Category.Levemete) },
            { "ui/icon/071000/071046", ("Levemete interaction", Category.Levemete) },
            { "ui/icon/071000/071051", ("Unavailable Levemete", Category.Levemete) },
            { "ui/icon/071000/071052", ("Unavailable repeatable Levemete", Category.Levemete) },
            { "ui/icon/071000/071053", ("Unavailable Levemete in progress", Category.Levemete) },
            { "ui/icon/071000/071054", ("Unavailable Levemete related marker", Category.Levemete) },
            { "ui/icon/071000/071055", ("Unavailable completed Levemete", Category.Levemete) },
            { "ui/icon/071000/071056", ("Unavailable Levemete interaction", Category.Levemete) },
            #endregion
            #region Lore Icons
            { "ui/icon/071000/071061", ("Lore quest", Category.Lore) },
            { "ui/icon/071000/071062", ("Repeatable lore quest", Category.Lore) },
            { "ui/icon/071000/071063", ("Lore quest in progress", Category.Lore) },
            { "ui/icon/071000/071064", ("Lore quest related marker", Category.Lore) },
            { "ui/icon/071000/071065", ("Completed lore quest", Category.Lore) },
            { "ui/icon/071000/071066", ("Lore interaction", Category.Lore) },
            { "ui/icon/071000/071071", ("Unavailable lore quest", Category.Lore) },
            { "ui/icon/071000/071072", ("Unavailable repeatable lore quest", Category.Lore) },
            { "ui/icon/071000/071073", ("Unavailable lore quest in progress", Category.Lore) },
            { "ui/icon/071000/071074", ("Unavailable lore quest related marker", Category.Lore) },
            { "ui/icon/071000/071075", ("Unavailable completed lore quest", Category.Lore) },
            { "ui/icon/071000/071076", ("Unavailable lore interaction", Category.Lore) },
            #endregion
            #region Behest Icons
            { "ui/icon/071000/071081", ("Behest", Category.Behest) },
            { "ui/icon/071000/071082", ("Repeatable Behest", Category.Behest) },
            { "ui/icon/071000/071083", ("Behest in progress", Category.Behest) },
            { "ui/icon/071000/071084", ("Behest related marker", Category.Behest) },
            { "ui/icon/071000/071085", ("Completed Behest", Category.Behest) },
            { "ui/icon/071000/071086", ("Behest interaction", Category.Behest) },
            { "ui/icon/071000/071091", ("Unavailable Behest", Category.Behest) },
            { "ui/icon/071000/071092", ("Unavailable repeatable Behest", Category.Behest) },
            { "ui/icon/071000/071093", ("Unavailable Behest in progress", Category.Behest) },
            { "ui/icon/071000/071094", ("Unavailable Behest related marker", Category.Behest) },
            { "ui/icon/071000/071095", ("Unavailable completed Behest", Category.Behest) },
            { "ui/icon/071000/071096", ("Unavailable Behest interaction", Category.Behest) },
            #endregion
            #region Triple Triad Icons
            { "ui/icon/071000/071101", ("Triple Triad match", Category.TripleTriad) },
            { "ui/icon/071000/071102", ("Triple Triad match - Cards obtainable", Category.TripleTriad) },
            #endregion
            #region GATE Icons
            { "ui/icon/071000/071111", ("GATE", Category.GATE) },
            { "ui/icon/071000/071112", ("Completed GATE", Category.GATE) },
            #endregion
            #region Beginner quest Icons
            { "ui/icon/071000/071121", ("Beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071122", ("Repeatable beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071123", ("Beginner quest in progress", Category.BeginnerQuest) },
            { "ui/icon/071000/071124", ("Beginner quest related marker", Category.BeginnerQuest) },
            { "ui/icon/071000/071125", ("Completed beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071126", ("Beginner quest interaction", Category.BeginnerQuest) },
            { "ui/icon/071000/071131", ("Unavailable beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071132", ("Unavailable repeatable beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071133", ("Unavailable beginner quest in progress", Category.BeginnerQuest) },
            { "ui/icon/071000/071134", ("Unavailable beginner quest related marker", Category.BeginnerQuest) },
            { "ui/icon/071000/071135", ("Unavailable completed beginner quest", Category.BeginnerQuest) },
            { "ui/icon/071000/071136", ("Unavailable beginner quest interaction", Category.BeginnerQuest) },
            #endregion
            #region Key/Feature quest Icons
            { "ui/icon/071000/071141", ("Key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071142", ("Repeatable key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071143", ("Key quest in progress", Category.FeatureQuest) },
            { "ui/icon/071000/071145", ("Completed key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071146", ("Key quest interaction", Category.FeatureQuest) },
            { "ui/icon/071000/071151", ("Unavailable key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071152", ("Unavailable repeatable key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071153", ("Unavailable key quest in progress", Category.FeatureQuest) },
            { "ui/icon/071000/071155", ("Unavailable completed key quest", Category.FeatureQuest) },
            { "ui/icon/071000/071156", ("Unavailable key quest interaction", Category.FeatureQuest) },
            #endregion
            #endregion
        };

        public static SortedSet<string> MapIcons = new SortedSet<string>(MapIconData.Keys);
    }
}
