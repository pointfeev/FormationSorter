using System;
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
        public static readonly List<FormationClass> MeleeCavalryFormationClasses = new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry };
        public static readonly List<FormationClass> RangedCavalryFormationClasses = new List<FormationClass>() { FormationClass.HorseArcher };
        public static readonly List<FormationClass> GroundMeleeFormationClasses = new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry };
        public static readonly List<FormationClass> GroundRangedFormationClasses = new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher };
        public static readonly List<FormationClass> BasicMeleeFormationClasses = new List<FormationClass>() { FormationClass.Infantry, FormationClass.Cavalry };
        public static readonly List<FormationClass> BasicRangedFormationClasses = new List<FormationClass>() { FormationClass.Ranged, FormationClass.HorseArcher };
        public static readonly List<FormationClass> GroundFormationClasses = new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher };
        public static readonly List<FormationClass> CavalryFormationClasses = new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher };

        public static void SelectAllFormations()
        {
            if (!MissionOrder.IsCurrentMissionReady()) return;
            List<FormationClass> allFormationClasses = new List<FormationClass>();
            allFormationClasses.AddRange((IEnumerable<FormationClass>)Enum.GetValues(typeof(FormationClass)));
            SelectFormationsOfClasses(allFormationClasses, "all");
        }

        public static void AddAllFormationOrderTroopItemVMs()
        {
            if (!MissionOrder.IsCurrentMissionReady()) return;
            if (!MissionOrder.CanSortOrderBeUsedInCurrentMission()) return;
            foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
            {
                GetOrderTroopItemVM(formation);
            }
            SortOrderTroopItemVMs();
        }

        public static void SelectFormationsOfClasses(List<FormationClass> formationClasses, string feedback = null)
        {
            if (!MissionOrder.MissionOrderVM.IsToggleOrderShown || !Settings.InverseSelectionModifierKey.IsKeyDown())
            {
                previousSelections.Clear();
            }
            SetFormationSelections();
            List<Formation> selections = new List<Formation>();
            foreach (Formation formation in previousSelections)
            {
                bool isCorrectFormation = IsFormationOneOfFormationClasses(formation, formationClasses);
                if (!isCorrectFormation)
                {
                    selections.Add(formation);
                }
            }
            List<Formation> invertedSelections = new List<Formation>();
            foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
            {
                bool isCorrectFormation = IsFormationOneOfFormationClasses(formation, formationClasses);
                bool wasPreviouslySelected = previousSelections.Contains(formation);
                bool shouldInvertSelection = Settings.InverseSelectionModifierKey.IsKeyDown() && wasPreviouslySelected;
                if (isCorrectFormation)
                {
                    if (shouldInvertSelection)
                    {
                        invertedSelections.Add(formation);
                    }
                    else
                    {
                        selections.Add(formation);
                    }
                }
            }
            if (!(feedback is null))
            {
                if (feedback == "all") feedback = ""; else feedback += " ";
                if (invertedSelections.Any() || selections.Any(f => f.CountOfUnits > 0))
                {
                    InformationManager.DisplayMessage(new InformationMessage($"{(invertedSelections.Any() ? "Unselected" : "Selected")} all {feedback}formations", Colors.Cyan, "FormationSorter"));
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage($"There are no units to be selected in any {feedback.Replace("and", "or")}formations", Colors.Cyan, "FormationSorter"));
                }
            }
            SetFormationSelections(selections);
            previousSelections = selections.ToList();
        }

        private static List<Formation> previousSelections = new List<Formation>();

        private static bool IsFormationOneOfFormationClasses(Formation formation, List<FormationClass> formationClasses)
        {
            return formation.CountOfUnits > 0 ? formationClasses.Contains(formation.PrimaryClass) : formationClasses.Contains(formation.InitialClass);
        }

        private static void SetFormationSelections(List<Formation> selections = null)
        {
            MissionOrder.MissionOrderVM.OrderController.ClearSelectedFormations();
            MissionOrder.MissionOrderVM.TryCloseToggleOrder();
            if (selections is null || !selections.Any(f => f.CountOfUnits > 0)) return;
            MissionOrder.MissionOrderVM.OpenToggleOrder(false);
            MissionOrderTroopControllerVM troopController = MissionOrder.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = GetOrderTroopItemVM(selections.First());
            if (!(orderTroopItemVM is null))
            {
                MissionOrder.MissionOrderVM.OnSelect((int)selections.First().FormationIndex);
                typeof(MissionOrderTroopControllerVM).GetMethod("SetSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(troopController, new object[] { orderTroopItemVM });
            }
            for (int i = 1; i <= selections.Count - 1; i++)
            {
                Formation formation = selections[i];
                orderTroopItemVM = GetOrderTroopItemVM(formation);
                if (!(orderTroopItemVM is null))
                {
                    typeof(MissionOrderTroopControllerVM).GetMethod("AddSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(troopController, new object[] { orderTroopItemVM });
                }
            }
            SortOrderTroopItemVMs();
        }

        private static OrderTroopItemVM GetOrderTroopItemVM(Formation formation)
        {
            MissionOrderTroopControllerVM troopController = MissionOrder.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = troopController.TroopList.SingleOrDefault(t => t.Formation == formation);
            if (orderTroopItemVM is null && MissionOrder.CanSortOrderBeUsedInCurrentMission())
            {
                orderTroopItemVM = new OrderTroopItemVM(formation,
                    new Action<OrderTroopItemVM>(item => typeof(MissionOrderTroopControllerVM).GetMethod("OnSelectFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(troopController, new object[] { item })),
                    (int)Mission.Current.GetAverageMoraleOfAgentsWithIndices(formation.CollectUnitIndices()));
                orderTroopItemVM.FormationClass = (int)formation.InitialClass;
                troopController.TroopList.Add(orderTroopItemVM);
                SortOrderTroopItemVMs();
            }
            if (!(orderTroopItemVM is null))
            {
                typeof(MissionOrderTroopControllerVM).GetMethod("SetTroopActiveOrders", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(MissionOrder.MissionOrderVM.TroopController, new object[] { orderTroopItemVM });
                orderTroopItemVM.IsSelectable = MissionOrder.MissionOrderVM.OrderController.IsFormationSelectable(formation);
                orderTroopItemVM.IsSelected = orderTroopItemVM.IsSelectable && MissionOrder.MissionOrderVM.OrderController.IsFormationListening(formation);
            }
            return orderTroopItemVM;
        }

        private static void SortOrderTroopItemVMs()
        {
            if (!MissionOrder.CanSortOrderBeUsedInCurrentMission()) return;
            MissionOrderTroopControllerVM troopController = MissionOrder.MissionOrderVM.TroopController;
            if (troopController.TroopList.Any())
            {
                List<OrderTroopItemVM> sorted = troopController.TroopList.OrderBy(item => item.InitialFormationClass).ToList();
                troopController.TroopList.Clear();
                for (int i = 0; i < sorted.Count; i++)
                {
                    troopController.TroopList.Insert(i, sorted[i]);
                }
            }
        }
    }
}