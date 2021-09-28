using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
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
            typeof(OnToggleActivateOrderStateDelegate), typeof(OnToggleActivateOrderStateDelegate), typeof(bool)
        })]
        [HarmonyPostfix]
        public static void MissionOrderVM(MissionOrderVM __instance)
        {
            MissionOrder.MissionOrderVM = __instance;
            try
            {
                MissionOrder.OrderSetIndex = __instance.OrderSets.Count;
                if (!MissionOrder.CanSortOrderBeUsedInCurrentMission()) return;
                OrderSetVM OrderSetVM = (OrderSetVM)typeof(OrderSetVM).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
                    typeof(OrderSubType), typeof(int), typeof(Action<OrderItemVM, OrderSetType, bool>), typeof(bool)
                }, null).Invoke(new object[] { OrderSubType.FormClose, MissionOrder.OrderSetIndex, (Action<OrderItemVM, OrderSetType, bool>)((OrderItemVM o, OrderSetType or, bool b) => { }), false });
                __instance.OrderSets.Add(OrderSetVM);
                Selection.AddAllFormationOrderTroopItemVMs();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        [HarmonyPatch("OnTransferFinished")]
        [HarmonyPatch("OpenToggleOrder")]
        [HarmonyPostfix]
        public static void OnTransferFinished_OnOpenToggleOrder()
        {
            try
            {
                Selection.AddAllFormationOrderTroopItemVMs();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }
    }
}