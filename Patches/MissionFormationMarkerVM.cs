using System;
using System.Linq;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;

namespace FormationSorter.Patches;

[HarmonyPatch(typeof(MissionFormationMarkerVM))]
public static class PatchMissionFormationMarkerVM
{
    [HarmonyPatch("RefreshFormationPositions"), HarmonyPrefix]
    public static bool RefreshFormationPositions(MissionFormationMarkerVM __instance)
    {
        try
        {
            if (Mission.IsCurrentValid())
                foreach (MissionFormationMarkerTargetVM target in __instance.Targets.ToList().Where(target => target.Formation.CountOfUnits == 0))
                {
                    target.ScreenPosition = new(-10000f, -10000f);
                    _ = __instance.Targets.Remove(target);
                }
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
        return true;
    }
}