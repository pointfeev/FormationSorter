using System;
using System.Collections.Generic;
using System.Linq;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade;

namespace FormationSorter.Patches;

[HarmonyPatch(typeof(OrderController))]
public static class PatchOrderController
{
    [HarmonyPatch("IsFormationSelectable"), HarmonyPatch(new[] { typeof(Formation), typeof(Agent) }), HarmonyPrefix]
    public static bool IsFormationSelectable(Formation formation, Agent selectorAgent, ref bool __result)
    {
        try
        {
            if (Mission.IsCurrentValid())
            {
                __result = selectorAgent == null || formation.PlayerOwner == selectorAgent;
                return false;
            }
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
        return true;
    }

    [HarmonyPatch("OnSelectedFormationsCollectionChanged"), HarmonyPostfix]
    public static void OnSelectedFormationsCollectionChanged() => Selection.UpdateAllFormationOrderTroopItemVMs();

    [HarmonyPatch("SetOrderWithFormationAndNumber"), HarmonyPrefix]
    public static bool SetOrderWithFormationAndNumber(OrderType orderType, List<Formation> ____selectedFormations)
        => orderType != OrderType.Transfer || ____selectedFormations.Sum(f => f.CountOfUnits) != 0;
}