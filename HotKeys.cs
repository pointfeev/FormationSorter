using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class HotKeys
    {
        public static int UniqueId;

        public static GameKey OrderGameKey;
        public static InputKey OrderKey = InputKey.X;

        public static InputKey ModifierKey = InputKey.LeftControl;

        public static InputKey SelectAllKey = InputKey.F;
        public static InputKey SelectAllGroundedKey = InputKey.N;
        public static InputKey SelectAllMountedKey = InputKey.M;

        public static InputKey SelectInfantryKey = InputKey.H;
        public static InputKey SelectRangedKey = InputKey.J;
        public static InputKey SelectCavalryKey = InputKey.C;
        public static InputKey SelectHorseArchersKey = InputKey.V;

        private static Dictionary<InputKey, bool> pressedLastTick = new Dictionary<InputKey, bool>();

        public static void Initialize()
        {
            UniqueId = 'F' + 'o' + 'r' + 'm' + 'a' + 't' + 'i' + 'o' + 'n' + 'E' + 'd' + 'i' + 't'; // 1333
            OrderGameKey = new GameKey(UniqueId, "FormationSorterOrderHotKey", "FormationSorterHotKeyGroup", OrderKey);
        }

        private static List<FormationClass> previousSelections = new List<FormationClass>();

        private static void SelectFormationsOfClasses(List<FormationClass> formationClasses)
        {
            typeof(OrderController).GetMethod("UpdateTroops", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM.OrderController, new object[0]);
            if (!Order.MissionOrderVM.IsToggleOrderShown)
            {
                previousSelections.Clear();
            }
            Order.MissionOrderVM.OrderController.ClearSelectedFormations();
            foreach (OrderTroopItemVM orderTroopItemVM in Order.MissionOrderVM?.TroopController?.TroopList)
            {
                orderTroopItemVM.IsSelected = false;
            }
            Order.MissionOrderVM.TryCloseToggleOrder();
            Order.MissionOrderVM.OpenToggleOrder(false);
            typeof(MissionOrderVM).GetMethod("SetActiveOrders", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM, new object[0]);
            typeof(OrderController).GetMethod("UpdateTroops", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM.OrderController, new object[0]);
            typeof(OrderController).GetMethod("OnSelectedFormationsCollectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM.OrderController, new object[0]);
            List<FormationClass> selections = new List<FormationClass>();
            foreach (Formation formation in Mission.Current.PlayerTeam.FormationsIncludingEmpty)
            {
                bool wasSelectedPreviously = previousSelections.Contains(formation.InitialClass);
                bool shouldDoOpposite = ModifierKey.IsDown() && wasSelectedPreviously;
                if (formationClasses.Contains(formation.InitialClass))
                {
                    SetFormationSelected(formation, !shouldDoOpposite);
                    if (!shouldDoOpposite)
                    {
                        selections.Add(formation.InitialClass);
                    }
                }
                else
                {
                    SetFormationSelected(formation, shouldDoOpposite);
                    if (shouldDoOpposite)
                    {
                        selections.Add(formation.InitialClass);
                    }
                }
            }
            typeof(MissionOrderVM).GetMethod("SetActiveOrders", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM, new object[0]);
            typeof(OrderController).GetMethod("UpdateTroops", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM.OrderController, new object[0]);
            typeof(OrderController).GetMethod("OnSelectedFormationsCollectionChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(Order.MissionOrderVM.OrderController, new object[0]);
            previousSelections = selections;
        }

        private static void SetFormationSelected(Formation formation, bool selected = true)
        {
            List<Formation> selectedFormations = (List<Formation>)typeof(OrderController).GetField("_selectedFormations", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Order.MissionOrderVM.OrderController);
            if (selected)
            {
                if (!selectedFormations.Contains(formation))
                {
                    selectedFormations.Add(formation);
                }
            }
            else
            {
                if (selectedFormations.Contains(formation))
                {
                    selectedFormations.Remove(formation);
                }
            }
            MBBindingList<OrderTroopItemVM> troopList = Order.MissionOrderVM.TroopController.TroopList;
            OrderTroopItemVM orderTroopItemVM = troopList.SingleOrDefault(t => t.InitialFormationClass == formation.InitialClass);
            if (!(orderTroopItemVM is null)) orderTroopItemVM.IsSelected = selected;
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

        public static void HotKeysTick(float dt)
        {
            if (Mission.Current is null) return;
            if (Mission.Current.PlayerTeam is null) return;
            if (Order.MissionOrderVM is null) return;
            if (Order.MissionOrderVM.OrderController is null) return;
            if (Order.MissionOrderVM.TroopController is null) return;
            if (Order.MissionOrderVM.DeploymentController is null) return;
            ProcessKey(OrderKey, () => Order.OnOrderHotKeyPressed());
            ProcessKey(SelectAllKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher, FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }));
            ProcessKey(SelectAllGroundedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry, FormationClass.Ranged, FormationClass.Skirmisher }));
            ProcessKey(SelectAllMountedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry, FormationClass.HorseArcher }));
            ProcessKey(SelectInfantryKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Infantry, FormationClass.HeavyInfantry }));
            ProcessKey(SelectRangedKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Ranged, FormationClass.Skirmisher }));
            ProcessKey(SelectCavalryKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.Cavalry, FormationClass.LightCavalry, FormationClass.HeavyCavalry }));
            ProcessKey(SelectHorseArchersKey, () => SelectFormationsOfClasses(new List<FormationClass>() { FormationClass.HorseArcher }));
        }
    }
}