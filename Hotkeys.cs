using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;

namespace FormationSorter
{
    public static class Hotkeys
    {
        public static int UniqueId;

        public static GameKey OrderGameKey;

        public static void Initialize()
        {
            UniqueId = 'F' + 'o' + 'r' + 'm' + 'a' + 't' + 'i' + 'o' + 'n' + 'S' + 'o' + 'r' + 't' + 'e' + 'r';
            RefreshOrderGameKey();
        }

        public static void RefreshOrderGameKey()
        {
            OrderGameKey = new GameKey(UniqueId, "FormationSorterOrderHotkey", "FormationSorterHotkeyGroup", Settings.OrderKey);
            if (MissionOrder.IsCurrentMissionReady()) MissionOrder.MissionOrderVM.RefreshValues();
        }

        private static Dictionary<InputKey, bool> pressedLastTick = new Dictionary<InputKey, bool>();

        public static void OnApplicationTick(float dt)
        {
            try
            {
                if (!MissionOrder.IsCurrentMissionReady()) return;

                ProcessKey(Settings.OrderKey, () => MissionOrder.OnOrderHotkeyPressed());

                ProcessKey(Settings.SelectAllKey, () => Selection.SelectAllFormations());

                ProcessKey(Settings.SelectAllMeleeCavalryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry }, "melee cavalry"));
                ProcessKey(Settings.SelectAllHorseArchersKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.HorseArcher }, "horse archer"));

                ProcessKey(Settings.SelectAllInfantryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry }, "infantry"));
                ProcessKey(Settings.SelectAllArchersAndSkirmishersKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher }, "archer and skirmisher"));

                ProcessKey(Settings.SelectAllNormalMeleeKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.Cavalry }, "normal melee"));
                ProcessKey(Settings.SelectAllRangedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.HorseArcher }, "archer"));

                ProcessKey(Settings.SelectAllGroundedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher }, "grounded"));
                ProcessKey(Settings.SelectAllMountedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }, "mounted"));
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        private static void ProcessKey(InputKey inputKey, Action action)
        {
            if (inputKey.IsPressed())
            {
                if (!pressedLastTick.TryGetValue(inputKey, out bool b) || !b)
                {
                    pressedLastTick[inputKey] = true;
                    action();
                }
            }
            else
            {
                pressedLastTick[inputKey] = false;
            }
        }
    }
}