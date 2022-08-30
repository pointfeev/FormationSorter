using HarmonyLib;

using System;
using System.Collections.Generic;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace FormationSorter
{
    [HarmonyPatch(typeof(InformationManager))]
    public static class PatchInformationManager
    {
        public static bool SuppressSelectAllFormations = false;

        private static List<string> ignoredMessages;

        private static TaleWorlds.MountAndBlade.Mission lastCheckedMission;

        public static void SetIgnoredMessagesDirty() => lastCheckedMission = null;

        public static List<string> IgnoredMessages
        {
            get
            {
                if (ignoredMessages is null)
                    ignoredMessages = new List<string>();

                TaleWorlds.MountAndBlade.Mission mission = Mission.Current;
                if (lastCheckedMission == mission)
                    return ignoredMessages;
                lastCheckedMission = mission;

                ignoredMessages.Clear();
                if (!mission.IsInventoryAccessAllowed && Hotkeys.IsGameKeyBound("InventoryWindow"))
                    ignoredMessages.Add(GameTexts.FindText((mission.Mode == MissionMode.Battle || mission.Mode == MissionMode.Duel)
                        ? "str_cannot_reach_inventory_during_battle" : "str_cannot_reach_inventory")?.ToString() ?? "");
                if (!mission.IsQuestScreenAccessAllowed && Hotkeys.IsGameKeyBound("QuestsWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_quests", null)?.ToString() ?? "");
                if (!mission.IsPartyWindowAccessAllowed && Hotkeys.IsGameKeyBound("PartyWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_party", null)?.ToString() ?? "");
                if (!mission.IsEncyclopediaWindowAccessAllowed && Hotkeys.IsGameKeyBound("EncyclopediaWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_encyclopedia", null)?.ToString() ?? "");
                if ((!mission.IsKingdomWindowAccessAllowed || (!Hero.MainHero?.MapFaction?.IsKingdomFaction).GetValueOrDefault(false)) && Hotkeys.IsGameKeyBound("KingdomWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_kingdom", null)?.ToString() ?? "");
                if (!mission.IsClanWindowAccessAllowed && Hotkeys.IsGameKeyBound("ClanWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_clan", null)?.ToString() ?? "");
                if (!mission.IsCharacterWindowAccessAllowed && Hotkeys.IsGameKeyBound("CharacterWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_character", null)?.ToString() ?? "");
                if ((!mission.IsBannerWindowAccessAllowed || (!Campaign.Current?.IsBannerEditorEnabled).GetValueOrDefault(false)) && Hotkeys.IsGameKeyBound("BannerWindow"))
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_banner", null)?.ToString() ?? "");

                return ignoredMessages;
            }
        }

        [HarmonyPatch("DisplayMessage")]
        [HarmonyPrefix]
        public static bool DisplayMessage(InformationMessage message)
        {
            try
            {
                if (Mission.IsCurrentValid())
                {
                    if (IgnoredMessages.Contains(message.Information)
                    || SuppressSelectAllFormations && message.Information == new TextObject("{=xTv4tCbZ}Everybody!! Listen to me").ToString())
                        return false;
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return true;
        }
    }
}