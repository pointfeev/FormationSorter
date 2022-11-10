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
            ProcessKey(Settings.Instance.AllSelectKey, () => Selection.SelectFormations());
            ProcessKey(Settings.Instance.CavalryMeleeSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateCavalryMelee(), feedback: "melee cavalry "));
            ProcessKey(Settings.Instance.CavalryRangedSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateCavalryRanged(), feedback: "ranged cavalry "));
            ProcessKey(Settings.Instance.GroundMeleeSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateGroundMelee(), feedback: "melee ground "));
            ProcessKey(Settings.Instance.GroundRangedSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateGroundRanged(), feedback: "ranged ground "));
            ProcessKey(Settings.Instance.MeleeSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateMelee(), feedback: "melee "));
            ProcessKey(Settings.Instance.RangedSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateRanged(), feedback: "ranged "));
            ProcessKey(Settings.Instance.GroundSelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateGround(), feedback: "ground "));
            ProcessKey(Settings.Instance.CavalrySelectKey, () => Selection.SelectFormations(formationClasses: FormationClassUtils.EnumerateCavalry(), feedback: "cavalry "));
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
                    if (inputKey == GetActionKey() && Mission.CanPlayerInteract())
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
            || Settings.Instance.CavalryMeleeSelectKey == inputKey || Settings.Instance.CavalryRangedSelectKey == inputKey
            || Settings.Instance.GroundMeleeSelectKey == inputKey || Settings.Instance.GroundRangedSelectKey == inputKey
            || Settings.Instance.MeleeSelectKey == inputKey || Settings.Instance.RangedSelectKey == inputKey
            || Settings.Instance.GroundSelectKey == inputKey || Settings.Instance.CavalrySelectKey == inputKey);

        public static bool IsGameKeyBound(string stringId) => !TryGetGameKeyFromStringId(stringId, out GameKey gameKey) || gameKey.KeyboardKey.InputKey.IsKeyBound();

        public static bool TryGetGameKeyFromStringId(string stringId, out GameKey gameKey)
        {
            GenericGameKeyContext gameKeyContext = GenericGameKeyContext.Current;
            if (!(gameKeyContext is null))
            {
                gameKey = gameKeyContext.GetGameKey(stringId);
                if (!(gameKey is null) && !(gameKey.KeyboardKey is null))
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