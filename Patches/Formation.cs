using System;

using HarmonyLib;

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    [HarmonyPatch(typeof(Formation))]
    public static class PatchFormation
    {
        [HarmonyPatch("CalculateFormationClass")]
        [HarmonyPrefix]
        public static bool CalculateFormationClass(Formation __instance, ref FormationClass ____initialClass)
        {
            try
            {
                if (Mission.IsCurrentValid() && Mission.IsCurrentOrderable())
                {
                    ____initialClass = __instance.FormationIndex;
                    return false;
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return true;
        }
    }
}