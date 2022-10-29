using System;
using System.Collections.Generic;

using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public static class Hotkeys
    {
        public static void OnApplicationTick()
        {
            if (!Mission.IsCurrentValid()) return;
            ProcessKey(Settings.Instance.OrderKey, () => Order.OnOrder());
            ProcessKey(Settings.Instance.TierSortKey, () => Order.OnOrder(tierSort: true));
            ProcessKey(Settings.Instance.AllSelectKey, () => Selection.SelectAllFormations());
            ProcessKey(Settings.Instance.MeleeCavalrySelectKey, () => Selection.SelectFormationsOfClasses(Selection.MeleeCavalryFormationClasses, "melee cavalry"));
            ProcessKey(Settings.Instance.RangedCavalrySelectKey, () => Selection.SelectFormationsOfClasses(Selection.RangedCavalryFormationClasses, "ranged cavalry"));
            ProcessKey(Settings.Instance.MeleeGroundSelectKey, () => Selection.SelectFormationsOfClasses(Selection.GroundMeleeFormationClasses, "ground melee"));
            ProcessKey(Settings.Instance.RangedGroundSelectKey, () => Selection.SelectFormationsOfClasses(Selection.GroundRangedFormationClasses, "ground ranged"));
            ProcessKey(Settings.Instance.MeleeSelectKey, () => Selection.SelectFormationsOfClasses(Selection.BasicMeleeFormationClasses, "basic melee"));
            ProcessKey(Settings.Instance.RangedSelectKey, () => Selection.SelectFormationsOfClasses(Selection.BasicRangedFormationClasses, "basic ranged"));
            ProcessKey(Settings.Instance.GroundSelectKey, () => Selection.SelectFormationsOfClasses(Selection.GroundFormationClasses, "ground"));
            ProcessKey(Settings.Instance.CavalrySelectKey, () => Selection.SelectFormationsOfClasses(Selection.CavalryFormationClasses, "cavalry"));
        }

        private static void ProcessKey(InputKey inputKey, Action action)
        {
            if (pressedLastTick is null)
                pressedLastTick = new Dictionary<InputKey, bool>();
            if (inputKey.IsDefinedAndDown())
            {
                if (!pressedLastTick.TryGetValue(inputKey, out bool b) || !b)
                {
                    pressedLastTick[inputKey] = true;
                    if (Mission.CanPlayerInteract() && inputKey == GetActionKey())
                        return;
                    action();
                }
            }
            else pressedLastTick[inputKey] = false;
        }

        private static Dictionary<InputKey, bool> pressedLastTick;

        private static InputKey GetActionKey() => TryGetGameKeyFromStringId("Action", out GameKey gameKey) ? gameKey.KeyboardKey.InputKey : InputKey.F;

        public static bool IsKeyBound(this InputKey inputKey) => IsDefined(inputKey) && (Settings.Instance.OrderKey == inputKey
            || Settings.Instance.ShieldSortKey == inputKey || Settings.Instance.SkirmisherSortKey == inputKey
            || Settings.Instance.EqualSortKey == inputKey || Settings.Instance.InverseSelectKey == inputKey
            || Settings.Instance.TierSortKey == inputKey || Settings.Instance.AllSelectKey == inputKey
            || Settings.Instance.MeleeCavalrySelectKey == inputKey || Settings.Instance.RangedCavalrySelectKey == inputKey
            || Settings.Instance.MeleeGroundSelectKey == inputKey || Settings.Instance.RangedGroundSelectKey == inputKey
            || Settings.Instance.MeleeSelectKey == inputKey || Settings.Instance.RangedSelectKey == inputKey
            || Settings.Instance.GroundSelectKey == inputKey || Settings.Instance.CavalrySelectKey == inputKey);

        public static bool IsGameKeyBound(string stringId) => !TryGetGameKeyFromStringId(stringId, out GameKey gameKey) || gameKey.KeyboardKey.InputKey.IsKeyBound();

        public static bool TryGetGameKeyFromStringId(string stringId, out GameKey gameKey)
        {
            List<GameKey> gameKeys = GenericGameKeyContext.Current?.GameKeys;
            if (!(gameKeys is null))
                foreach (GameKey _gameKey in gameKeys)
                    if (_gameKey.StringId == stringId && !(_gameKey.KeyboardKey is null))
                    {
                        gameKey = _gameKey;
                        return true;
                    }
            gameKey = null;
            return false;
        }

        public static bool IsDefined(this InputKey inputKey)
        {
            if (isKeyDefined.TryGetValue(inputKey, out bool defined))
                return defined;
            defined = Enum.IsDefined(typeof(InputKey), inputKey);
            isKeyDefined.Add(inputKey, defined);
            return defined;
        }

        private static readonly Dictionary<InputKey, bool> isKeyDefined = new Dictionary<InputKey, bool>();

        public static bool IsDefinedAndDown(this InputKey inputKey) => inputKey.IsDefined() && inputKey.IsDown();
    }
}