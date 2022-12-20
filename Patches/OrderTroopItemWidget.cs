using System;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;

namespace FormationSorter.Patches
{
    [HarmonyPatch(typeof(OrderTroopItemBrushWidget))]
    public static class PatchOrderTroopItemWidget
    {
        [HarmonyPatch("UpdateBackgroundState")]
        [HarmonyPostfix]
        public static void UpdateBackgroundState(OrderTroopItemBrushWidget __instance)
        {
            try
            {
                if (Mission.IsCurrentValid() && __instance.IsSelectable && __instance.CurrentMemberCount <= 0)
                    __instance.SetState(__instance.IsSelected ? "Selected" : "Disabled");
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}