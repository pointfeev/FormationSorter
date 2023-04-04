using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FormationSorter.Utilities;

internal static class FormationClassUtils
{
    public static string GetGameTextString(this FormationClass formationClass) => Regex.Replace(formationClass.GetName(), "[A-Z]", " $0").Trim();

    public static IEnumerable<FormationClass> EnumerateCavalryMelee()
    {
        yield return FormationClass.Cavalry;
        yield return FormationClass.LightCavalry;
        yield return FormationClass.HeavyCavalry;
    }

    public static IEnumerable<FormationClass> EnumerateCavalryRanged()
    {
        yield return FormationClass.HorseArcher;
    }

    public static IEnumerable<FormationClass> EnumerateGroundMelee()
    {
        yield return FormationClass.Infantry;
        yield return FormationClass.HeavyInfantry;
    }

    public static IEnumerable<FormationClass> EnumerateGroundRanged()
    {
        yield return FormationClass.Ranged;
        yield return FormationClass.Skirmisher;
    }

    public static IEnumerable<FormationClass> EnumerateMelee()
    {
        yield return FormationClass.Cavalry;
        yield return FormationClass.Infantry;
        yield return FormationClass.HeavyInfantry;
    }

    public static IEnumerable<FormationClass> EnumerateRanged()
    {
        yield return FormationClass.HorseArcher;
        yield return FormationClass.Ranged;
        yield return FormationClass.Skirmisher;
    }

    public static IEnumerable<FormationClass> EnumerateCavalry()
    {
        yield return FormationClass.Cavalry;
        yield return FormationClass.LightCavalry;
        yield return FormationClass.HeavyCavalry;
        yield return FormationClass.HorseArcher;
    }

    public static IEnumerable<FormationClass> EnumerateGround()
    {
        yield return FormationClass.Infantry;
        yield return FormationClass.HeavyInfantry;
        yield return FormationClass.Ranged;
        yield return FormationClass.Skirmisher;
    }

    internal static FormationClass GetFormationClass(this Formation formation)
    {
        FormationClass formationClass = formation.FormationIndex;
        if (!Settings.Instance.UserDefinedFormationClasses)
            return formationClass;
        formationClass = formation.Index switch
        {
            0 => Settings.Instance.Formation1, 1 => Settings.Instance.Formation2, 2 => Settings.Instance.Formation3, 3 => Settings.Instance.Formation4,
            4 => Settings.Instance.Formation5, 5 => Settings.Instance.Formation6, 6 => Settings.Instance.Formation7, 7 => Settings.Instance.Formation8,
            _ => formationClass
        };
        return formationClass;
    }

    internal static bool IsFormationOneOfFormationClasses(Formation formation, IEnumerable<FormationClass> formationClasses)
        => formationClasses.Contains(GetFormationClass(formation));

    internal static IEnumerable<Formation> GetFormationsForFormationClass(IEnumerable<Formation> formations, FormationClass formationClass)
        => formations.Where(formation => GetFormationClass(formation) == formationClass);

    internal static FormationClass GetBestFormationClassForAgent(Agent agent, bool useShields = false, bool useSkirmishers = false, bool useCompanions = false)
    {
        if (useCompanions && agent.IsHero && Settings.Instance.CompanionFormation is not FormationClass.Unset)
            return Settings.Instance.CompanionFormation;
        Agent mount = agent.MountAgent;
        return mount?.Health > 0 && mount.IsActive() && (agent.CanReachAgent(mount) || agent.GetTargetAgent() == mount)
            ? agent.IsRangedCached ? FormationClass.HorseArcher : FormationClass.Cavalry
            : agent.IsRangedCached
                ? FormationClass.Ranged
                : useSkirmishers && agent.HasThrownCached || useShields && !agent.HasShieldCached
                    ? FormationClass.Skirmisher
                    : FormationClass.Infantry;
    }
}