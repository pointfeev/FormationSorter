﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class Selection
    {
        public static int UniqueId;

        public static GameKey OrderGameKey;
        public static InputKey OrderKey = InputKey.X;

        public static InputKey ModifierKey = InputKey.LeftControl;

        public static InputKey SelectAllKey = InputKey.F;

        public static InputKey SelectAllMeleeCavalryKey = InputKey.C;
        public static InputKey SelectHorseArchersKey = InputKey.V;

        public static InputKey SelectAllInfantryKey = InputKey.H;
        public static InputKey SelectArchersAndSkirmishersKey = InputKey.J;

        public static InputKey SelectAllMeleeKey = InputKey.Y;
        public static InputKey SelectAllRangedKey = InputKey.U;

        public static InputKey SelectAllGroundedKey = InputKey.N;
        public static InputKey SelectAllMountedKey = InputKey.M;

        public static void Initialize()
        {
            UniqueId = 'F' + 'o' + 'r' + 'm' + 'a' + 't' + 'i' + 'o' + 'n' + 'E' + 'd' + 'i' + 't'; // 1333
            OrderGameKey = new GameKey(UniqueId, "FormationSorterOrderHotKey", "FormationSorterHotKeyGroup", OrderKey);
        }

        public static void SelectAllFormations()
        {
            if (!IsMissionOrderVMActive()) return;
            List<FormationClass> allFormationClasses = new List<FormationClass>();
            allFormationClasses.AddRange((IEnumerable<FormationClass>)Enum.GetValues(typeof(FormationClass)));
            SelectFormationsOfClasses(allFormationClasses);
        }

        public static void AddAllFormationOrderTroopItemVMs()
        {
            if (!IsMissionOrderVMActive()) return;
            foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
            {
                GetOrderTroopItemVM(formation, false);
            }
            SortOrderTroopItemVMs();
        }

        private static Dictionary<InputKey, bool> pressedLastTick = new Dictionary<InputKey, bool>();

        public static void HotKeysTick(float dt)
        {
            try
            {
                if (!IsMissionOrderVMActive()) return;

                ProcessKey(OrderKey, () => Order.OnOrderHotKeyPressed());

                ProcessKey(SelectAllKey, () => SelectAllFormations());

                ProcessKey(SelectAllMeleeCavalryKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry }));
                ProcessKey(SelectHorseArchersKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.HorseArcher }));

                ProcessKey(SelectAllInfantryKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry }));
                ProcessKey(SelectArchersAndSkirmishersKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher }));

                ProcessKey(SelectAllMeleeKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.Cavalry }));
                ProcessKey(SelectAllRangedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.HorseArcher }));

                ProcessKey(SelectAllGroundedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher }));
                ProcessKey(SelectAllMountedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }));
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        private static void ProcessKey(InputKey inputKey, Action action)
        {
            if (inputKey.IsPressed())
            {
                if (!pressedLastTick.TryGetValue(inputKey, out bool b) || !b)
                {
                    pressedLastTick[inputKey] = true;
                    action();
                }
            }
            else
            {
                pressedLastTick[inputKey] = false;
            }
        }

        private static bool IsMissionOrderVMActive()
        {
            if (Mission.Current is null) return false;
            if (Mission.Current.PlayerTeam is null) return false;
            if (Order.MissionOrderVM is null) return false;
            if (Order.MissionOrderVM.OrderController is null) return false;
            if (Order.MissionOrderVM.TroopController is null) return false;
            if (Order.MissionOrderVM.DeploymentController is null) return false;
            return true;
        }

        private static bool IsFormationOneOfFormationClasses(Formation formation, List<FormationClass> formationClasses)
        {
            return formation.CountOfUnits > 0 ? formationClasses.Contains(formation.PrimaryClass) : formationClasses.Contains(formation.InitialClass);
        }

        private static List<Formation> previousSelections = new List<Formation>();

        private static void SelectFormationsOfClasses(List<FormationClass> formationClasses)
        {
            if (!Order.MissionOrderVM.IsToggleOrderShown || !ModifierKey.IsDown())
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
            foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
            {
                bool isCorrectFormation = IsFormationOneOfFormationClasses(formation, formationClasses);
                bool wasPreviouslySelected = previousSelections.Contains(formation);
                bool shouldInvertSelection = ModifierKey.IsDown() && wasPreviouslySelected;
                if (isCorrectFormation && !shouldInvertSelection)
                {
                    selections.Add(formation);
                }
            }
            if (selections.Any())
            {
                InformationManager.DisplayMessage(new InformationMessage($"Selected " + string.Join(", ", formationClasses) + (formationClasses.Count == 1 ? " formation" : " formations"), Colors.Cyan, "FormationSorter"));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage($"There are no units to be selected in the " + string.Join(", ", formationClasses) + (formationClasses.Count == 1 ? " formation" : " formations"), Colors.Cyan, "FormationSorter"));
            }
            SetFormationSelections(selections);
            previousSelections = selections.ToList();
        }

        private static void SetFormationSelections(List<Formation> selections = null)
        {
            Order.MissionOrderVM.OrderController.ClearSelectedFormations();
            Order.MissionOrderVM.TryCloseToggleOrder();
            if (selections is null || !selections.Any() || selections.All(f => f.CountOfUnits <= 0)) return;
            Order.MissionOrderVM.OpenToggleOrder(false);
            MissionOrderTroopControllerVM troopController = Order.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = GetOrderTroopItemVM(selections.First());
            Order.MissionOrderVM.OnSelect((int)selections.First().FormationIndex);
            typeof(MissionOrderTroopControllerVM).GetMethod("SetSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(troopController, new object[] { orderTroopItemVM });
            for (int i = 1; i <= selections.Count - 1; i++)
            {
                Formation formation = selections[i];
                orderTroopItemVM = GetOrderTroopItemVM(formation);
                typeof(MissionOrderTroopControllerVM).GetMethod("AddSelectedFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(troopController, new object[] { orderTroopItemVM });
            }
            SortOrderTroopItemVMs();
        }

        private static OrderTroopItemVM GetOrderTroopItemVM(Formation formation, bool setSelected = true)
        {
            MissionOrderTroopControllerVM troopController = Order.MissionOrderVM.TroopController;
            OrderTroopItemVM orderTroopItemVM = troopController.TroopList.SingleOrDefault(t => t.Formation == formation);
            if (orderTroopItemVM is null)
            {
                orderTroopItemVM = new OrderTroopItemVM(formation,
                    new Action<OrderTroopItemVM>(item => typeof(MissionOrderTroopControllerVM).GetMethod("OnSelectFormation", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(troopController, new object[] { item })),
                    (int)Mission.Current.GetAverageMoraleOfAgentsWithIndices(formation.CollectUnitIndices()));
                orderTroopItemVM.FormationClass = (int)formation.InitialClass;
                troopController.TroopList.Add(orderTroopItemVM);
                SortOrderTroopItemVMs();
            }
            orderTroopItemVM.IsSelectable = true;
            if (setSelected)
            {
                orderTroopItemVM.IsSelected = true;
                orderTroopItemVM.IsSelectionActive = true;
            }
            return orderTroopItemVM;
        }

        private static void SortOrderTroopItemVMs()
        {
            MissionOrderTroopControllerVM troopController = Order.MissionOrderVM.TroopController;
            if (troopController.TroopList.Any()) troopController.TroopList.OrderBy(item => item.InitialFormationClass);
        }
    }
}