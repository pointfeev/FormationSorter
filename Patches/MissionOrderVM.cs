using HarmonyLib;
using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace FormationSorter
{
    [HarmonyPatch(typeof(MissionOrderVM))]
    public static class PatchMissionOrderVM
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] {
            typeof(Camera), typeof(List<DeploymentPoint>), typeof(Action<bool>), typeof(bool), typeof(GetOrderFlagPositionDelegate),
            typeof(OnRefreshVisualsDelegate), typeof(ToggleOrderPositionVisibilityDelegate), typeof(OnToggleActivateOrderStateDelegate),
            typeof(OnToggleActivateOrderStateDelegate), typeof(OnToggleActivateOrderStateDelegate), typeof(bool)
        })]
        [HarmonyPostfix]
        public static void MissionOrderVM(MissionOrderVM __instance)
        {
            try
            {
                Mission.MissionOrderVM = __instance;
                if (!Mission.IsCurrentOrderable()) return;
                Update();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        [HarmonyPatch("OnTransferFinished")]
        [HarmonyPatch("SetActiveOrders")]
        [HarmonyPatch("OnOrder")]
        [HarmonyPostfix]
        public static void Update()
        {
            Selection.UpdateAllFormationOrderTroopItemVMs();
        }
    }
}