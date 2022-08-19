using System;

using HarmonyLib;

using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;

namespace FormationSorter
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
                if (Mission.IsCurrentValid() && Mission.IsCurrentOrderable() && __instance.IsSelectable && __instance.CurrentMemberCount <= 0)
                    __instance.SetState(__instance.IsSelected ? "Selected" : "Disabled");
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}