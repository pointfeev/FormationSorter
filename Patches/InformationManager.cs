﻿using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    [HarmonyPatch(typeof(InformationManager))]
    public static class PatchInformationManager
    {
        public static bool SuppressSelectAllFormations = false;

        private static List<string> ignoredMessages;

        private static Mission checkedMission;

        public static void SetCheckDirty()
        {
            checkedMission = null;
        }

        public static List<string> IgnoredMessages
        {
            get
            {
                if (ignoredMessages is null)
                {
                    ignoredMessages = new List<string>();
                }
                Mission mission = Mission.Current;
                List<GameKey> gameKeys = (List<GameKey>)typeof(InputContext).GetField("_registeredGameKeys", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mission.InputManager);
                if (mission is null || checkedMission == mission) return ignoredMessages;
                checkedMission = mission;
                ignoredMessages.Clear();
                if (!mission.IsInventoryAccessAllowed && gameKeys[37].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText((Mission.Current.Mode == MissionMode.Battle || Mission.Current.Mode == MissionMode.Duel) ? "str_cannot_reach_inventory_during_battle" : "str_cannot_reach_inventory").ToString());
                }
                if (!mission.IsQuestScreenAccessAllowed && gameKeys[41].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_quests", null).ToString());
                }
                if (!mission.IsPartyWindowAccessAllowed && gameKeys[42].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_party", null).ToString());
                }
                if (!mission.IsEncyclopediaWindowAccessAllowed && gameKeys[38].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_encyclopedia", null).ToString());
                }
                if ((!mission.IsKingdomWindowAccessAllowed || !Hero.MainHero.MapFaction.IsKingdomFaction) && gameKeys[39].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_kingdom", null).ToString());
                }
                if (!mission.IsClanWindowAccessAllowed && gameKeys[40].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_clan", null).ToString());
                }
                if (!mission.IsCharacterWindowAccessAllowed && gameKeys[36].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_character", null).ToString());
                }
                if ((!mission.IsBannerWindowAccessAllowed || !Campaign.Current.IsBannerEditorEnabled) && gameKeys[35].KeyboardKey.InputKey.IsKeyBound())
                {
                    ignoredMessages.Add(GameTexts.FindText("str_cannot_open_banner", null).ToString());
                }
                return ignoredMessages;
            }
        }

        [HarmonyPatch("DisplayMessage")]
        [HarmonyPrefix]
        public static bool DisplayMessage(InformationMessage message)
        {
            if (MissionOrder.IsCurrentMissionReady())
            {
                if (IgnoredMessages.Contains(message.Information)) return false;
                if (SuppressSelectAllFormations && message.Information == new TextObject("{=xTv4tCbZ}Everybody!! Listen to me", null).ToString()) return false;
            }
            return true;
        }
    }
}