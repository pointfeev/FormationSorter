using HarmonyLib;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;

namespace FormationSorter
{
    [HarmonyPatch(typeof(MissionFormationMarkerVM))]
    public static class PatchMissionFormationMarkerVM
    {
        [HarmonyPatch("RefreshFormationPositions")]
        [HarmonyPrefix]
        public static bool RefreshFormationPositions(MissionFormationMarkerVM __instance)
        {
            foreach (MissionFormationMarkerTargetVM target in __instance.Targets.ToList())
            {
                if (target.Formation.CountOfUnits <= 0)
                {
                    target.ScreenPosition = new Vec2(-10000f, -10000f);
                    __instance.Targets.Remove(target);
                }
            }
            return true;
        }
    }
}