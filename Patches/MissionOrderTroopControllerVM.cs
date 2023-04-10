using System;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter.Patches;

[HarmonyPatch(typeof(MissionOrderTroopControllerVM))]
public static class PatchMissionOrderTroopControllerVM
{
    [HarmonyPatch("UpdateTroops"), HarmonyPostfix]
    public static void UpdateTroops()
    {
        try
        {
            Selection.UpdateTroops();
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
    }
}