using HarmonyLib;
using System;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    [HarmonyPatch(typeof(OrderSetVM))]
    public static class PatchOrderSetVM
    {
        [HarmonyPatch("RefreshValues")]
        [HarmonyPostfix]
        public static void RefreshValues(OrderSetVM __instance)
        {
            try
            {
                if (__instance == MissionOrder.OrderSetVM) MissionOrder.RefreshOrderButton();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}