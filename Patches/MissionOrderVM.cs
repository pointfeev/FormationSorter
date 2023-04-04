using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter.Patches;

[HarmonyPatch(typeof(MissionOrderVM))]
public static class PatchMissionOrderVM
{
    [HarmonyPatch(MethodType.Constructor),
     HarmonyPatch(new[]
     {
         typeof(Camera), typeof(List<DeploymentPoint>), typeof(Action<bool>), typeof(bool), typeof(GetOrderFlagPositionDelegate),
         typeof(OnRefreshVisualsDelegate), typeof(ToggleOrderPositionVisibilityDelegate), typeof(OnToggleActivateOrderStateDelegate),
         typeof(OnToggleActivateOrderStateDelegate), typeof(OnToggleActivateOrderStateDelegate), typeof(OnBeforeOrderDelegate), typeof(bool)
     }), HarmonyPostfix]
    public static void MissionOrderVM(MissionOrderVM __instance) => Mission.MissionOrderVM = __instance;
}