using System;
using System.Collections.Generic;
using FormationSorter.Utilities;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public static class HotKeys
    {
        private static Dictionary<InputKey, bool> pressedLastTick;

        private static readonly Dictionary<InputKey, bool> IsKeyDefined = new Dictionary<InputKey, bool>();

        public static void OnApplicationTick()
        {
            if (!Mission.IsCurrentValid())
                return;
            ProcessKey(Settings.Instance.OrderKey, () => Order.OnOrder());
            ProcessKey(Settings.Instance.TierSortKey, () => Order.OnOrder(true));
            ProcessKey(Settings.Instance.AllSelectKey, () => Selection.SelectFormations());
            ProcessKey(Settings.Instance.CavalryMeleeSelectKey,
                () => Selection.SelectFormations(FormationClassUtils.EnumerateCavalryMelee(), "melee cavalry "));
            ProcessKey(Settings.Instance.CavalryRangedSelectKey,
                () => Selection.SelectFormations(FormationClassUtils.EnumerateCavalryRanged(), "ranged cavalry "));
            ProcessKey(Settings.Instance.GroundMeleeSelectKey, () => Selection.SelectFormations(FormationClassUtils.EnumerateGroundMelee(), "melee ground "));
            ProcessKey(Settings.Instance.GroundRangedSelectKey,
                () => Selection.SelectFormations(FormationClassUtils.EnumerateGroundRanged(), "ranged ground "));
            ProcessKey(Settings.Instance.MeleeSelectKey, () => Selection.SelectFormations(FormationClassUtils.EnumerateMelee(), "melee "));
            ProcessKey(Settings.Instance.RangedSelectKey, () => Selection.SelectFormations(FormationClassUtils.EnumerateRanged(), "ranged "));
            ProcessKey(Settings.Instance.GroundSelectKey, () => Selection.SelectFormations(FormationClassUtils.EnumerateGround(), "ground "));
            ProcessKey(Settings.Instance.CavalrySelectKey, () => Selection.SelectFormations(FormationClassUtils.EnumerateCavalry(), "cavalry "));
        }

        private static void ProcessKey(InputKey inputKey, Action action)
        {
            if (pressedLastTick is null)
                pressedLastTick = new Dictionary<InputKey, bool>();
            if (inputKey.IsDefinedAndDown())
            {
                if (pressedLastTick.TryGetValue(inputKey, out bool b) && b)
                    return;
                pressedLastTick[inputKey] = true;
                if (inputKey == GetActionKey() && Mission.CanPlayerInteract())
                    return;
                action();
            }
            else
                pressedLastTick[inputKey] = false;
        }

        private static InputKey GetActionKey() => TryGetGameKeyFromStringId("Action", out GameKey gameKey) ? gameKey.KeyboardKey.InputKey : InputKey.F;

        private static bool IsKeyBound(this InputKey inputKey)
            => IsDefined(inputKey) && (Settings.Instance.OrderKey == inputKey || Settings.Instance.ShieldSortKey == inputKey
                                                                              || Settings.Instance.SkirmisherSortKey == inputKey
                                                                              || Settings.Instance.EqualSortKey == inputKey
                                                                              || Settings.Instance.InverseSelectKey == inputKey
                                                                              || Settings.Instance.TierSortKey == inputKey
                                                                              || Settings.Instance.AllSelectKey == inputKey
                                                                              || Settings.Instance.CavalryMeleeSelectKey == inputKey
                                                                              || Settings.Instance.CavalryRangedSelectKey == inputKey
                                                                              || Settings.Instance.GroundMeleeSelectKey == inputKey
                                                                              || Settings.Instance.GroundRangedSelectKey == inputKey
                                                                              || Settings.Instance.MeleeSelectKey == inputKey
                                                                              || Settings.Instance.RangedSelectKey == inputKey
                                                                              || Settings.Instance.GroundSelectKey == inputKey
                                                                              || Settings.Instance.CavalrySelectKey == inputKey);

        public static bool IsGameKeyBound(string stringId)
            => !TryGetGameKeyFromStringId(stringId, out GameKey gameKey) || gameKey.KeyboardKey.InputKey.IsKeyBound();

        private static bool TryGetGameKeyFromStringId(string stringId, out GameKey gameKey)
        {
            GenericGameKeyContext gameKeyContext = GenericGameKeyContext.Current;
            if (!(gameKeyContext is null))
            {
                gameKey = gameKeyContext.GetGameKey(stringId);
                if (gameKey?.KeyboardKey != null)
                    return true;
            }
            gameKey = null;
            return false;
        }

        private static bool IsDefined(this InputKey inputKey)
        {
            if (IsKeyDefined.TryGetValue(inputKey, out bool defined))
                return defined;
            defined = Enum.IsDefined(typeof(InputKey), inputKey);
            IsKeyDefined.Add(inputKey, defined);
            return defined;
        }

        public static bool IsDefinedAndDown(this InputKey inputKey) => inputKey.IsDefined() && inputKey.IsDown();
    }
}