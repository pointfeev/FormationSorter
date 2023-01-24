using System;
using System.Collections.Generic;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace FormationSorter.Patches;

[HarmonyPatch(typeof(InformationManager))]
public static class PatchInformationManager
{
    public static bool SuppressSelectAllFormations = false;

    private static List<string> ignoredMessages;

    private static TaleWorlds.MountAndBlade.Mission lastCheckedMission;

    private static List<string> IgnoredMessages
    {
        get
        {
            if (ignoredMessages is null)
                ignoredMessages = new();
            TaleWorlds.MountAndBlade.Mission mission = Mission.Current;
            if (lastCheckedMission == mission)
                return ignoredMessages;
            lastCheckedMission = mission;
            ignoredMessages.Clear();
            if (!mission.IsInventoryAccessAllowed && HotKeys.IsGameKeyBound("InventoryWindow"))
                ignoredMessages.Add(GameTexts.FindText(mission.Mode is MissionMode.Battle or MissionMode.Duel
                    ? "str_cannot_reach_inventory_during_battle"
                    : "str_cannot_reach_inventory")?.ToString() ?? "");
            if (!mission.IsQuestScreenAccessAllowed && HotKeys.IsGameKeyBound("QuestsWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_quests")?.ToString() ?? "");
            if (!mission.IsPartyWindowAccessAllowed && HotKeys.IsGameKeyBound("PartyWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_party")?.ToString() ?? "");
            if (!mission.IsEncyclopediaWindowAccessAllowed && HotKeys.IsGameKeyBound("EncyclopediaWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_encyclopedia")?.ToString() ?? "");
            if ((!mission.IsKingdomWindowAccessAllowed || (!Hero.MainHero?.MapFaction.IsKingdomFaction).GetValueOrDefault(false))
             && HotKeys.IsGameKeyBound("KingdomWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_kingdom")?.ToString() ?? "");
            if (!mission.IsClanWindowAccessAllowed && HotKeys.IsGameKeyBound("ClanWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_clan")?.ToString() ?? "");
            if (!mission.IsCharacterWindowAccessAllowed && HotKeys.IsGameKeyBound("CharacterWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_character")?.ToString() ?? "");
            if ((!mission.IsBannerWindowAccessAllowed || (!Campaign.Current?.IsBannerEditorEnabled).GetValueOrDefault(false))
             && HotKeys.IsGameKeyBound("BannerWindow"))
                ignoredMessages.Add(GameTexts.FindText("str_cannot_open_banner")?.ToString() ?? "");
            return ignoredMessages;
        }
    }

    [HarmonyPatch("DisplayMessage"), HarmonyPrefix]
    public static bool DisplayMessage(InformationMessage message)
    {
        try
        {
            if (Mission.IsCurrentValid())
                if (IgnoredMessages.Contains(message.Information) || SuppressSelectAllFormations
                 && message.Information == new TextObject("{=xTv4tCbZ}Everybody!! Listen to me").ToString())
                    return false;
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
        return true;
    }
}