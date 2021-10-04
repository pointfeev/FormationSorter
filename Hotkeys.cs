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

        public static bool IsKeyDown(this InputKey inputKey)
        {
            if (inputKey is InputKey.Invalid) return false;
            return inputKey.IsDown();
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
                ProcessKey(Settings.SelectAllRangedCavalryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.HorseArcher }, "ranged cavalry"));

                ProcessKey(Settings.SelectAllGroundMeleeKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry }, "ground melee"));
                ProcessKey(Settings.SelectAllGroundRangedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher }, "ground ranged"));

                ProcessKey(Settings.SelectAllBasicMeleeKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.Cavalry }, "basic melee"));
                ProcessKey(Settings.SelectAllBasicRangedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.HorseArcher }, "basic ranged"));

                ProcessKey(Settings.SelectAllGroundKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher }, "ground"));
                ProcessKey(Settings.SelectAllCavalryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }, "cavalry"));
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