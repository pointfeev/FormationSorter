using HarmonyLib;

using System;
using System.Collections.Generic;

using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    [HarmonyPatch(typeof(MissionOrderVM))]
    public static class PatchMissionOrderVM
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] {
            typeof(Camera), typeof(List<DeploymentPoint>), typeof(Action<bool>), typeof(bool), typeof(GetOrderFlagPositionDelegate),
            typeof(OnRefreshVisualsDelegate), typeof(ToggleOrderPositionVisibilityDelegate), typeof(OnToggleActivateOrderStateDelegate),
            typeof(OnToggleActivateOrderStateDelegate), typeof(OnToggleActivateOrderStateDelegate), typeof(OnBeforeOrderDelegate), typeof(bool)
        })]
        [HarmonyPostfix]
        public static void MissionOrderVM(MissionOrderVM __instance)
        {
            try
            {
                Mission.MissionOrderVM = __instance;
                if (!Mission.IsCurrentValid() || !Mission.IsCurrentOrderable())
                    return;
                Selection.UpdateAllFormationOrderTroopItemVMs();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        [HarmonyPatch("OnTransferFinished")]
        [HarmonyPostfix]
        public static void OnTransferFinished() => Selection.UpdateAllFormationOrderTroopItemVMs();

        [HarmonyPatch("SetActiveOrders")]
        [HarmonyPostfix]
        public static void SetActiveOrders() => Selection.UpdateAllFormationOrderTroopItemVMs();

        [HarmonyPatch("OnOrder")]
        [HarmonyPostfix]
        public static void OnOrder() => Selection.UpdateAllFormationOrderTroopItemVMs();
    }
}