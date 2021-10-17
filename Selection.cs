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

        private static List<FormationClass> allFormationClasses = AllFormationClasses;

        public static List<FormationClass> AllFormationClasses
        {
            get
            {
                if (allFormationClasses is null)
                {
                    try
                    {
                        allFormationClasses = new List<FormationClass>();
                        allFormationClasses.AddRange((IEnumerable<FormationClass>)Enum.GetValues(typeof(FormationClass)));
                    }
                    catch (Exception e)
                    {
                        OutputUtils.DoOutputForException(e);
                        return new List<FormationClass>();
                    }
                }
                return allFormationClasses;
            }
        }

        public static void SelectAllFormations()
        {
            if (!Mission.IsCurrentOrderable()) return;
            SelectFormationsOfClasses(AllFormationClasses, "all");
        }

        public static void UpdateAllFormationOrderTroopItemVMs()
        {
            try
            {
                if (ReflectionUtils.IsMethodInCallStack(MethodBase.GetCurrentMethod())) return;
                if (!Mission.IsCurrentReady()) return;
                if (!Mission.IsCurrentOrderable()) return;
                foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
                {
                    GetOrderTroopItemVM(formation);
                }
                SortOrderTroopItemVMs();
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        public static void SelectFormationsOfClasses(List<FormationClass> formationClasses, string feedback = null)
        {
            if (formationClasses is null || !formationClasses.Any()) return;
            if (!Mission.MissionOrderVM.IsToggleOrderShown || !Settings.InverseSelectionModifierKey.IsDefinedAndDown())
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
            foreach (Formation formation in Mission.Current?.PlayerTeam?.FormationsIncludingEmpty)
            {
                if (formation is null) continue;
                bool isCorrectFormation = IsFormationOneOfFormationClasses(formation, formationClasses);
                bool wasPreviouslySelected = previousSelections.Contains(formation);
                bool shouldInvertSelection = Settings.InverseSelectionModifierKey.IsDefinedAndDown() && wasPreviouslySelected;
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
                    InformationManager.DisplayMessage(new InformationMessage($"{(invertedSelections.Any() ? "Unselected" : "Selected")} all {feedback}formations", Colors.White, "FormationSorter"));
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage($"There are no troops to be selected in any {feedback.Replace("and", "or")}formations", Colors.White, "FormationSorter"));
                }
            }
            PatchInformationManager.SuppressSelectAllFormations = true;
            SetFormationSelections(selections);
            PatchInformationManager.SuppressSelectAllFormations = false;
            previousSelections = selections.ToList();
        }

        private static List<Formation> previousSelections = new List<Formation>();

        private static bool IsFormationOneOfFormationClasses(Formation formation, List<FormationClass> formationClasses)
        {
            return formation.CountOfUnits > 0 ? formationClasses.Contains(formation.PrimaryClass) : formationClasses.Contains(formation.InitialClass);
        }

        private static void SetFormationSelections(List<Formation> selections = null)
        {
            Mission.MissionOrderVM.OrderController.ClearSelectedFormations();
            Mission.MissionOrderVM.TryCloseToggleOrder();
            if (selections is null || !selections.Any(f => f.CountOfUnits > 0)) return;
            Mission.MissionOrderVM.OpenToggleOrder(false);
            MissionOrderTroopControllerVM troopController = Mission.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = GetOrderTroopItemVM(selections.First());
            if (!(orderTroopItemVM is null))
            {
                Mission.MissionOrderVM.OnSelect((int)selections.First().FormationIndex);
                ReflectionUtils.GetMethod(typeof(MissionOrderTroopControllerVM), "SetSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(troopController, new object[] { orderTroopItemVM });
            }
            for (int i = 1; i <= selections.Count - 1; i++)
            {
                Formation formation = selections[i];
                orderTroopItemVM = GetOrderTroopItemVM(formation);
                if (!(orderTroopItemVM is null))
                {
                    ReflectionUtils.GetMethod(typeof(MissionOrderTroopControllerVM), "AddSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(troopController, new object[] { orderTroopItemVM });
                }
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
                    new Action<OrderTroopItemVM>(item =>
                        ReflectionUtils.GetMethod(typeof(MissionOrderTroopControllerVM), "OnSelectFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                            .Invoke(troopController, new object[] { item })),
                    (int)Mission.Current.GetAverageMoraleOfAgentsWithIndices(formation.CollectUnitIndices()));
                troopController.TroopList.Add(orderTroopItemVM);
                SortOrderTroopItemVMs();
            }
            if (!(orderTroopItemVM is null))
            {
                if (selectable)
                {
                    ReflectionUtils.GetMethod(typeof(MissionOrderTroopControllerVM), "SetTroopActiveOrders", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(Mission.MissionOrderVM.TroopController, new object[] { orderTroopItemVM });
                    orderTroopItemVM.IsSelectable = selectable;
                    orderTroopItemVM.IsSelected = orderTroopItemVM.IsSelectable && Mission.MissionOrderVM.OrderController.IsFormationListening(formation);
                    orderTroopItemVM.SetFormationClassFromFormation(formation);
                }
                else
                {
                    troopController.TroopList.Remove(orderTroopItemVM);
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
                List<OrderTroopItemVM> sorted = troopController.TroopList.OrderBy(item => item.Formation.FormationIndex).ToList();
                troopController.TroopList.Clear();
                for (int i = 0; i < sorted.Count; i++)
                {
                    troopController.TroopList.Insert(i, sorted[i]);
                }
            }
        }
    }
}