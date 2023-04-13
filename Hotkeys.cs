using System;
using System.Collections.Generic;
using FormationSorter.Utilities;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace FormationSorter;

public static class HotKeys
{
    private static Dictionary<InputKey, bool> pressedLastTick;

    private static readonly Dictionary<InputKey, bool> IsKeyDefined = new();

    public static void OnApplicationTick()
    {
        if (!Mission.IsCurrentValid())
            return;
        bool equalSort = Settings.Instance.EqualSortKey.IsDefinedAndDown();
        bool useShields = Settings.Instance.ShieldSortKey.IsDefinedAndDown();
        bool useSkirmishers = Settings.Instance.SkirmisherSortKey.IsDefinedAndDown();
        ProcessKey(Settings.Instance.OrderKey, () => Order.OnOrder(false, equalSort, useShields, useSkirmishers));
        ProcessKey(Settings.Instance.TierSortKey, () => Order.OnOrder(true, equalSort, useShields, useSkirmishers));
        ProcessKey(Settings.Instance.AllSelectKey, () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Everyone));
        ProcessKey(Settings.Instance.CavalryMeleeSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Cavalry, FormationClassUtils.EnumerateCavalryMelee(), "melee cavalry "));
        ProcessKey(Settings.Instance.CavalryRangedSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.HorseArchers, FormationClassUtils.EnumerateCavalryRanged(), "ranged cavalry "));
        ProcessKey(Settings.Instance.GroundMeleeSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Infantry, FormationClassUtils.EnumerateGroundMelee(), "melee ground "));
        ProcessKey(Settings.Instance.GroundRangedSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Archers, FormationClassUtils.EnumerateGroundRanged(), "ranged ground "));
        ProcessKey(Settings.Instance.MeleeSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.MixedFormation, FormationClassUtils.EnumerateMelee(), "melee "));
        ProcessKey(Settings.Instance.RangedSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Archers, FormationClassUtils.EnumerateRanged(), "ranged "));
        ProcessKey(Settings.Instance.GroundSelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.MixedFormation, FormationClassUtils.EnumerateGround(), "ground "));
        ProcessKey(Settings.Instance.CavalrySelectKey,
            () => Selection.SelectFormations(SkinVoiceManager.VoiceType.Cavalry, FormationClassUtils.EnumerateCavalry(), "cavalry "));
    }

    private static void ProcessKey(InputKey inputKey, Action action)
    {
        pressedLastTick ??= new();
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
        if (gameKeyContext is not null)
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