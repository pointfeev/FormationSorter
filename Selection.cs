using System.Collections.Generic;
using System.Linq;
using FormationSorter.Patches;
using FormationSorter.Utilities;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter;

public static class Selection
{
    private static List<Formation> previousSelections = new();

    internal static void UpdateTroops()
    {
        if (!Mission.IsCurrentValid())
            return;
        MissionOrderVM missionOrder = Mission.MissionOrderVM;
        OrderController orderController = missionOrder?.OrderController;
        MissionOrderTroopControllerVM troopController = missionOrder?.TroopController;
        if (orderController is null || troopController is null)
            return;
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

    public static void SelectFormations(SkinVoiceManager.SkinVoiceType voiceFeedback, IEnumerable<FormationClass> formationClasses = null,
        string feedback = null, bool uiFeedback = true)
    {
        MissionOrderVM missionOrder = Mission.MissionOrderVM;
        if (missionOrder is null)
            return;
        UpdateTroops();
        if (!missionOrder.IsToggleOrderShown || !Settings.Instance.InverseSelectKey.IsDefinedAndDown())
            previousSelections.Clear();
        SetFormationSelections();
        formationClasses = formationClasses?.ToHashSet();
        List<Formation> selections = formationClasses is null
            ? new()
            : previousSelections.Where(formation => !formation.IsOneOfFormationClasses(formationClasses)).ToList();
        Team playerTeam = Mission.Current?.PlayerTeam;
        if (playerTeam == null)
            return;
        List<Formation> invertedSelections = new();
        foreach (Formation formation in playerTeam.FormationsIncludingEmpty)
        {
            if (formation is null)
                continue;
            bool isCorrectFormation = formationClasses is null || formation.IsOneOfFormationClasses(formationClasses);
            bool wasPreviouslySelected = previousSelections.Contains(formation);
            bool shouldInvertSelection = Settings.Instance.InverseSelectKey.IsDefinedAndDown() && wasPreviouslySelected;
            if (!isCorrectFormation)
                continue;
            if (shouldInvertSelection)
                invertedSelections.Add(formation);
            else
                selections.Add(formation);
        }
        if (uiFeedback)
        {
            if (invertedSelections.Any() || selections.Any(f => f.CountOfUnits > 0))
                InformationManager.DisplayMessage(new($"{(invertedSelections.Any() ? "Unselected" : "Selected")} all {feedback}formations", Colors.White,
                    SubModule.Id));
            else
                InformationManager.DisplayMessage(new($"There are no troops to be selected in any {feedback}formations", Colors.White, SubModule.Id));
        }
        PatchInformationManager.SuppressSelectAllFormations = true;
        SetFormationSelections(selections);
        PatchInformationManager.SuppressSelectAllFormations = false;
        previousSelections = selections;
        _ = typeof(MissionOrderVM).GetCachedMethod("SetActiveOrders").Invoke(missionOrder, new object[] { });
        if (selections.Count == 0 || !Mission.IsOrderShoutingAllowed())
            return;
        Mission.PlayerAgent.MakeVoice(voiceFeedback, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
    }

    private static void SetFormationSelections(List<Formation> selections = null)
    {
        MissionOrderVM missionOrder = Mission.MissionOrderVM;
        OrderController orderController = missionOrder?.OrderController;
        MissionOrderTroopControllerVM troopController = missionOrder?.TroopController;
        if (orderController is null || troopController is null)
            return;
        orderController.ClearSelectedFormations();
        _ = missionOrder.TryCloseToggleOrder();
        if (selections?.Any(f => f.CountOfUnits > 0) != true)
            return;
        missionOrder.OpenToggleOrder(false);
        OrderTroopItemVM orderTroopItemVM = GetOrderTroopItemVM(selections.First());
        if (orderTroopItemVM is not null)
        {
            missionOrder.OnSelect(selections.First().Index);
            _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("SetSelectedFormation").Invoke(troopController, new object[] { orderTroopItemVM });
        }
        for (int i = 1; i <= selections.Count - 1; i++)
        {
            Formation formation = selections[i];
            orderTroopItemVM = GetOrderTroopItemVM(formation);
            if (orderTroopItemVM is not null)
                _ = typeof(MissionOrderTroopControllerVM).GetCachedMethod("AddSelectedFormation").Invoke(troopController, new object[] { orderTroopItemVM });
        }
    }

    private static OrderTroopItemVM GetOrderTroopItemVM(Formation formation)
        => Mission.MissionOrderVM?.TroopController?.TroopList?.SingleOrDefault(t => t.Formation == formation);
}