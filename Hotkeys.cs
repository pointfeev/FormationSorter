using System;
using System.Collections.Generic;
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
                ProcessKey(Settings.SelectAllMeleeCavalryKey, () => Selection.SelectFormationsOfClasses(Selection.MeleeCavalryFormationClasses, "melee cavalry"));
                ProcessKey(Settings.SelectAllRangedCavalryKey, () => Selection.SelectFormationsOfClasses(Selection.RangedCavalryFormationClasses, "ranged cavalry"));
                ProcessKey(Settings.SelectAllGroundMeleeKey, () => Selection.SelectFormationsOfClasses(Selection.GroundMeleeFormationClasses, "ground melee"));
                ProcessKey(Settings.SelectAllGroundRangedKey, () => Selection.SelectFormationsOfClasses(Selection.GroundRangedFormationClasses, "ground ranged"));
                ProcessKey(Settings.SelectAllBasicMeleeKey, () => Selection.SelectFormationsOfClasses(Selection.BasicMeleeFormationClasses, "basic melee"));
                ProcessKey(Settings.SelectAllBasicRangedKey, () => Selection.SelectFormationsOfClasses(Selection.BasicRangedFormationClasses, "basic ranged"));
                ProcessKey(Settings.SelectAllGroundKey, () => Selection.SelectFormationsOfClasses(Selection.GroundFormationClasses, "ground"));
                ProcessKey(Settings.SelectAllCavalryKey, () => Selection.SelectFormationsOfClasses(Selection.CavalryFormationClasses, "cavalry"));
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        private static void ProcessKey(InputKey inputKey, Action action)
        {
            try
            {
                if (inputKey is null || action is null) return;
                if (pressedLastTick is null)
                {
                    pressedLastTick = new Dictionary<InputKey, bool>();
                }
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
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}