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
        public static void RefreshValues(OrderSetVM __instance, int ____index)
        {
            try
            {
                if (!MissionOrder.IsCurrentMissionReady() || !MissionOrder.CanSortOrderBeUsedInCurrentMission()) return;
                if (____index == MissionOrder.OrderSetIndex)
                {
                    MissionOrder.RefreshOrderButton(__instance);
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}