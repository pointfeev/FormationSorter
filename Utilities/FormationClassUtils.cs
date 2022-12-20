using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FormationSorter.Utilities
{
    internal static class FormationClassUtils
    {
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

        internal static FormationClass GetFormationClass(Formation formation)
        {
            FormationClass formationClass = formation.FormationIndex;
            if (!Settings.Instance.UserDefinedFormationClasses)
                return formationClass;
            switch (formation.Index)
            {
                case 0:
                    formationClass = Settings.Instance.Formation1;
                    break;
                case 1:
                    formationClass = Settings.Instance.Formation2;
                    break;
                case 2:
                    formationClass = Settings.Instance.Formation3;
                    break;
                case 3:
                    formationClass = Settings.Instance.Formation4;
                    break;
                case 4:
                    formationClass = Settings.Instance.Formation5;
                    break;
                case 5:
                    formationClass = Settings.Instance.Formation6;
                    break;
                case 6:
                    formationClass = Settings.Instance.Formation7;
                    break;
                case 7:
                    formationClass = Settings.Instance.Formation8;
                    break;
            }
            return formationClass;
        }

        internal static bool IsFormationOneOfFormationClasses(Formation formation,
                                                              IEnumerable<FormationClass> formationClasses)
            => formationClasses.Contains(GetFormationClass(formation));

        internal static Formation GetFormationForFormationClass(IEnumerable<Formation> formations,
                                                                FormationClass formationClass)
            => formations.FirstOrDefault(formation => GetFormationClass(formation) == formationClass);

        internal static FormationClass GetBestFormationClassForAgent(Agent agent, bool useShields = false,
                                                                     bool useSkirmishers = false,
                                                                     bool useCompanions = false)
        {
            agent.UpdateCachedAndFormationValues(false, false);
            if (useCompanions && agent.IsHero && !(Settings.Instance.CompanionFormation is FormationClass.Unset))
                return Settings.Instance.CompanionFormation;
            Agent mount = agent.MountAgent;
            return !(mount is null) && mount.Health > 0 && mount.IsActive()
                && (agent.CanReachAgent(mount) || agent.GetTargetAgent() == mount)
                ? agent.IsRangedCached ? FormationClass.HorseArcher : FormationClass.Cavalry
                : agent.IsRangedCached
                    ? FormationClass.Ranged
                    : (useSkirmishers && agent.HasThrownCached) || (useShields && !agent.HasShieldCached)
                        ? FormationClass.Skirmisher
                        : FormationClass.Infantry;
        }
    }
}