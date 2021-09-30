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
        public static InputKey OrderKey = InputKey.X;

        public static InputKey ControlKey = InputKey.LeftControl;
        public static InputKey AlternateKey = InputKey.LeftAlt;

        public static InputKey SelectAllKey = InputKey.F;

        public static InputKey SelectAllMeleeCavalryKey = InputKey.C;
        public static InputKey SelectAllHorseArchersKey = InputKey.V;

        public static InputKey SelectAllInfantryKey = InputKey.H;
        public static InputKey SelectAllArchersAndSkirmishersKey = InputKey.J;

        public static InputKey SelectAllMeleeKey = InputKey.Y;
        public static InputKey SelectAllRangedKey = InputKey.U;

        public static InputKey SelectAllGroundedKey = InputKey.N;
        public static InputKey SelectAllMountedKey = InputKey.M;

        public static void Initialize()
        {
            UniqueId = 'F' + 'o' + 'r' + 'm' + 'a' + 't' + 'i' + 'o' + 'n' + 'E' + 'd' + 'i' + 't'; // 1333
            OrderGameKey = new GameKey(UniqueId, "FormationSorterOrderHotKey", "FormationSorterHotKeyGroup", OrderKey);
        }

        private static Dictionary<InputKey, bool> pressedLastTick = new Dictionary<InputKey, bool>();

        public static void OnApplicationTick(float dt)
        {
            try
            {
                if (!MissionOrder.IsCurrentMissionReady()) return;

                ProcessKey(OrderKey, () => MissionOrder.OnOrderHotkeyPressed());

                ProcessKey(SelectAllKey, () => Selection.SelectAllFormations());

                ProcessKey(SelectAllMeleeCavalryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry }, "melee cavalry"));
                ProcessKey(SelectAllHorseArchersKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.HorseArcher }, "horse archer"));

                ProcessKey(SelectAllInfantryKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry }, "infantry"));
                ProcessKey(SelectAllArchersAndSkirmishersKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher }, "archer and skirmisher"));

                ProcessKey(SelectAllMeleeKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.Cavalry }, "melee"));
                ProcessKey(SelectAllRangedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.HorseArcher }, "archer"));

                ProcessKey(SelectAllGroundedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher }, "grounded"));
                ProcessKey(SelectAllMountedKey, () => Selection.SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }, "mounted"));
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