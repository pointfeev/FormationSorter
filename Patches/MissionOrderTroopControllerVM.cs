using System;
using System.Linq;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.MountAndBlade;
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
            if (!Mission.IsCurrentValid())
                return;
            MissionOrderVM missionOrder = Mission.MissionOrderVM;
            OrderController orderController = missionOrder.OrderController;
            MissionOrderTroopControllerVM troopController = missionOrder.TroopController;
            foreach (OrderTroopItemVM troopItem in from formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty
                                                   where formation is not null && troopController.TroopList.All(item => item.Formation != formation)
                                                   select new OrderTroopItemVM(formation,
                                                       i => typeof(MissionOrderTroopControllerVM).GetCachedMethod("OnSelectFormation")
                                                          .Invoke(troopController, new object[] { i }),
                                                       f => (int)typeof(MissionOrderTroopControllerVM).GetCachedMethod("GetFormationMorale")
                                                          .Invoke(troopController, new object[] { f }))
                                                   into troopItem
                                                   select (OrderTroopItemVM)typeof(MissionOrderTroopControllerVM).GetCachedMethod("AddTroopItemIfNotExist")
                                                      .Invoke(troopController, new object[] { troopItem, -1 }))
                _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("SetTroopActiveOrders").Invoke(troopController, new object[] { troopItem });
            _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("SortFormations").Invoke(troopController, new object[] { });
            foreach (OrderTroopItemVM troopItem in troopController.TroopList)
            {
                Formation formation = troopItem.Formation;
                troopItem.IsSelectable = orderController.IsFormationSelectable(formation);
                if (troopItem.IsSelectable && orderController.IsFormationListening(formation))
                    troopItem.IsSelected = true;
            }
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
    }
}