﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class Selection
    {
        public static void UpdateAllFormationOrderTroopItemVMs()
        {
            try
            {
                if (ReflectionUtils.IsMethodInCallStack(MethodBase.GetCurrentMethod()))
                    return;
                if (!Mission.IsCurrentValid() || !Mission.IsCurrentOrderable())
                    return;
                foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
                    _ = GetOrderTroopItemVM(formation);
                SortOrderTroopItemVMs();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        public static void SelectFormations(IEnumerable<FormationClass> formationClasses = null, string feedback = null)
        {
            if (!(formationClasses is null) && !formationClasses.Any()) return;
            if (!Mission.MissionOrderVM.IsToggleOrderShown || !Settings.Instance.InverseSelectKey.IsDefinedAndDown())
                previousSelections.Clear();
            SetFormationSelections();
            List<Formation> selections = new List<Formation>();
            foreach (Formation formation in previousSelections)
            {
                bool isCorrectFormation = formationClasses is null || FormationClassUtils.IsFormationOneOfFormationClasses(formation, formationClasses);
                if (!isCorrectFormation)
                    selections.Add(formation);
            }
            List<Formation> invertedSelections = new List<Formation>();
            foreach (Formation formation in Mission.Current?.PlayerTeam?.FormationsIncludingEmpty)
            {
                if (formation is null) continue;
                bool isCorrectFormation = formationClasses is null || FormationClassUtils.IsFormationOneOfFormationClasses(formation, formationClasses);
                bool wasPreviouslySelected = previousSelections.Contains(formation);
                bool shouldInvertSelection = Settings.Instance.InverseSelectKey.IsDefinedAndDown() && wasPreviouslySelected;
                if (isCorrectFormation)
                {
                    if (shouldInvertSelection) invertedSelections.Add(formation);
                    else selections.Add(formation);
                }
            }
            if (invertedSelections.Any() || selections.Any(f => f.CountOfUnits > 0))
                InformationManager.DisplayMessage(new InformationMessage($"{(invertedSelections.Any() ? "Unselected" : "Selected")} all {feedback}formations", Colors.White, "FormationSorter"));
            else
                InformationManager.DisplayMessage(new InformationMessage($"There are no troops to be selected in any {feedback.Replace("and", "or")}formations", Colors.White, "FormationSorter"));
            PatchInformationManager.SuppressSelectAllFormations = true;
            SetFormationSelections(selections);
            PatchInformationManager.SuppressSelectAllFormations = false;
            previousSelections = selections.ToList();
        }

        private static List<Formation> previousSelections = new List<Formation>();

        private static void SetFormationSelections(List<Formation> selections = null)
        {
            Mission.MissionOrderVM.OrderController.ClearSelectedFormations();
            _ = Mission.MissionOrderVM.TryCloseToggleOrder();
            if (selections is null || !selections.Any(f => f.CountOfUnits > 0))
                return;

            Mission.MissionOrderVM.OpenToggleOrder(false);
            MissionOrderTroopControllerVM troopController = Mission.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = GetOrderTroopItemVM(selections.First());
            if (!(orderTroopItemVM is null))
            {
                Mission.MissionOrderVM.OnSelect(selections.First().Index);
                _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("SetSelectedFormation").Invoke(troopController, new object[] { orderTroopItemVM });
            }
            for (int i = 1; i <= selections.Count - 1; i++)
            {
                Formation formation = selections[i];
                orderTroopItemVM = GetOrderTroopItemVM(formation);
                if (!(orderTroopItemVM is null))
                    _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("AddSelectedFormation").Invoke(troopController, new object[] { orderTroopItemVM });
            }
            SortOrderTroopItemVMs();
        }

        private static OrderTroopItemVM GetOrderTroopItemVM(Formation formation)
        {
            MissionOrderTroopControllerVM troopController = Mission.MissionOrderVM.TroopController;
            bool selectable = Mission.MissionOrderVM.OrderController.IsFormationSelectable(formation);
            OrderTroopItemVM orderTroopItemVM = troopController.TroopList.SingleOrDefault(t => t.Formation == formation);
            if (orderTroopItemVM is null && Mission.IsCurrentOrderable() && selectable)
            {
                orderTroopItemVM = new OrderTroopItemVM(formation,
                    new Action<OrderTroopItemVM>(item => typeof(MissionOrderTroopControllerVM).GetCachedMethod("OnSelectFormation")
                        .Invoke(troopController, new object[] { item })),
                    new Func<Formation, int>(_formation => (int)typeof(MissionOrderTroopControllerVM).GetCachedMethod("GetFormationMorale")
                        .Invoke(troopController, new object[] { _formation })));
                troopController.TroopList.Add(orderTroopItemVM);
                SortOrderTroopItemVMs();
            }
            if (!(orderTroopItemVM is null))
            {
                if (selectable)
                {
                    _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("SetTroopActiveOrders")
                        .Invoke(Mission.MissionOrderVM.TroopController, new object[] { orderTroopItemVM });
                    orderTroopItemVM.IsSelectable = selectable;
                    orderTroopItemVM.IsSelected = orderTroopItemVM.IsSelectable && Mission.MissionOrderVM.OrderController.IsFormationListening(formation);
                    _ = orderTroopItemVM.SetFormationClassFromFormation(formation);
                }
                else
                {
                    _ = troopController.TroopList.Remove(orderTroopItemVM);
                    SortOrderTroopItemVMs();
                }
            }
            return orderTroopItemVM;
        }

        private static void SortOrderTroopItemVMs()
        {
            if (!Mission.IsCurrentOrderable()) return;
            MissionOrderTroopControllerVM troopController = Mission.MissionOrderVM.TroopController;
            if (troopController.TroopList.Any())
            {
                List<OrderTroopItemVM> sorted = troopController.TroopList.OrderBy(item => item.Formation.Index).ToList();
                troopController.TroopList.Clear();
                for (int i = 0; i < sorted.Count; i++)
                    troopController.TroopList.Insert(i, sorted[i]);
            }
        }
    }
}