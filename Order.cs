﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FormationSorter.Utilities;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter;

internal static class Order
{
    private const string OrderText = "Formation Sort";
    private const string OrderIcon = "ToggleAI";

    private static void RemoveOrderVMs()
    {
        Mission.InputKeyItemVM = null;
        if (Mission.OrderSetVM is not null)
            _ = Mission.MissionOrderVM.OrderSets.Remove(Mission.OrderSetVM);
        Mission.OrderSetVM = null;
        Mission.OrderItemVM = null;
    }

    private static void RefreshOrderVMs()
    {
        InputKey orderKey = Settings.Instance.OrderKey;
        string key = orderKey.ToString();
        string keyId = "FormationSortOrderKey_" + key;
        if (Mission.InputKeyItemVM is not null && Mission.InputKeyItemVM.KeyID != keyId)
            RemoveOrderVMs();
        if (Mission.InputKeyItemVM is null)
        {
            Mission.InputKeyItemVM = InputKeyItemVM.CreateFromForcedID(keyId, new(key), false);
            Mission.InputKeyItemVM.SetForcedVisibility(true);
            Mission.InputKeyItemVM.OnFinalize();
        }
        if (Mission.OrderItemVM is null)
        {
            Mission.OrderItemVM
                = new(OrderSubType.None, OrderSetType.None, new(OrderText), (_, _) => { }) { ShortcutKey = Mission.InputKeyItemVM, IsTitle = true };
            Mission.OrderItemVM.IsTitle = true;
            Mission.OrderItemVM.TooltipText = OrderText;
            Mission.OrderItemVM.OrderIconID = OrderIcon;
            Mission.OrderItemVM.ShortcutKey = Mission.InputKeyItemVM;
            Mission.OrderItemVM.IsActive = true;
            Mission.OrderItemVM.OnFinalize();
        }
        if (Mission.OrderSetVM is null)
        {
            Mission.OrderSetVM = (OrderSetVM)typeof(OrderSetVM)
               .GetCachedConstructor(new[] { typeof(OrderSubType), typeof(int), typeof(Action<OrderItemVM, OrderSetType, bool>), typeof(bool) })
               .Invoke(new object[] { OrderSubType.None, 0, (Action<OrderItemVM, OrderSetType, bool>)((_, _, _) => { }), false });
            Mission.OrderSetVM.TitleText = OrderText;
            Mission.OrderSetVM.TitleOrderKey = Mission.InputKeyItemVM;
            Mission.OrderSetVM.TitleOrder = Mission.OrderItemVM;
            Mission.OrderSetVM.OnFinalize();
        }
        if (!Mission.MissionOrderVM.OrderSets.Contains(Mission.OrderSetVM))
            Mission.MissionOrderVM.OrderSets.Add(Mission.OrderSetVM);
    }

    internal static void OnApplicationTick()
    {
        if (!Mission.IsCurrentValid())
        {
            RemoveOrderVMs();
            return;
        }
        RefreshOrderVMs();
    }

    internal static void OnOrder(bool tierSort = false, bool equalSort = false, bool useShields = false, bool useSkirmishers = false, bool uiFeedback = true)
    {
        HashSet<Formation> formations = tierSort ? GetAllRegularFormations() : GetSelectedRegularFormations();
        if (formations is null)
            return;
        int numUnitsSorted = SortAgentsBetweenFormations(formations, tierSort, equalSort, useShields, useSkirmishers);
        if (!uiFeedback)
            return;
        switch (numUnitsSorted)
        {
            case (int)SortAgentsSpecialResult.Ignored:
                return;
            case (int)SortAgentsSpecialResult.AIControlled:
                InformationManager.DisplayMessage(new("Formations controlled by AI cannot be sorted", Colors.White, "FormationSorter"));
                return;
            case 0:
                if (tierSort)
                    InformationManager.DisplayMessage(new("No troops need tier sorting", Colors.White, "FormationSorter"));
                else
                    InformationManager.DisplayMessage(new("No troops need sorting between the selected formations", Colors.White, "FormationSorter"));
                return;
            default:
            {
                if (Mission.Current.IsOrderGesturesEnabled())
                    Mission.PlayerAgent.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                InformationManager.DisplayMessage(new($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "troop" : "troops")} between the selected formations",
                    Colors.White, "FormationSorter"));
                break;
            }
        }
        _ = Mission.MissionOrderVM.TryCloseToggleOrder();
    }

    private static HashSet<Formation> GetAllRegularFormations()
    {
        HashSet<Formation> formations = Mission.Current?.PlayerTeam?.FormationsIncludingEmpty?.ToHashSet();
        return formations?.Any() != true ? null : formations;
    }

    private static HashSet<Formation> GetSelectedRegularFormations()
    {
        HashSet<Formation> formations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToHashSet();
        if (formations is null)
            return null;
        _ = formations.RemoveWhere(f => f.GetFormationClass() > FormationClass.NumberOfRegularFormations);
        return !formations.Any() ? null : formations;
    }

    private static bool TrySetCaptainFormation(Agent agent, FormationClass formationClass, HashSet<Formation> formations,
        ref HashSet<Formation> changedFormations, List<Formation> allFormations, ref HashSet<Formation> captainChangedFormations,
        ref HashSet<Formation> captainSetFormations)
    {
        foreach (Formation classFormation in FormationClassUtils.GetFormationsForFormationClass(formations, formationClass, true))
            if (classFormation.CountOfUnits > 0 && (agent == Mission.PlayerAgent || TrySetAgentFormation(agent, classFormation, ref changedFormations))
                                                && !captainSetFormations.Contains(classFormation))
            {
                _ = captainSetFormations.Add(classFormation);
                if (classFormation.Captain == agent)
                    return false;
                if (allFormations is not null)
                    foreach (Formation formation in allFormations.Where(formation => formation.Captain == agent))
                    {
                        formation.Captain = null;
                        _ = captainChangedFormations.Add(classFormation);
                    }
                classFormation.Captain = agent;
                _ = captainChangedFormations.Add(classFormation);
                return true;
            }
        return false;
    }

    private static int SortAgentsBetweenFormations(HashSet<Formation> formations, bool tierSort, bool equalSort, bool useShields, bool useSkirmishers)
    {
        if (formations is null || formations.Count < 2)
            return (int)SortAgentsSpecialResult.Ignored;
        if (formations.All(f => f.IsAIControlled))
            return (int)SortAgentsSpecialResult.AIControlled;
        OrderController orderController = Mission.MissionOrderVM.OrderController;
        if (orderController is not null)
            _ = typeof(OrderController).GetCachedMethod("BeforeSetOrder").Invoke(orderController, new object[] { OrderType.Transfer });
        foreach (Formation formation in formations)
            formation.OnMassUnitTransferStart();
        HashSet<string> classesWithMissingFormations = new();
        HashSet<Formation> emptyFormations = formations.Where(f => f.CountOfUnits == 0).ToHashSet();
        HashSet<Formation> filledFormations = formations.Except(emptyFormations).ToHashSet();
        HashSet<Agent> agents = EnumerateAgentsInFormations(formations).ToHashSet();
        List<Formation> allFormations = null;
        HashSet<Agent> prospectiveCaptainSet = null;
        if (Settings.Instance.AssignFormationCaptains
         && Mission.Current?.MissionBehaviors?.FirstOrDefault(b => b is GeneralsAndCaptainsAssignmentLogic) is not null)
        {
            allFormations = Mission.Current.PlayerTeam.FormationsIncludingSpecialAndEmpty;
            IEnumerable<Agent> prospectiveCaptainsEnumerable = Settings.Instance.AssignNewFormationCaptains
                ? agents.Where(a => a.IsHero)
                : formations.Where(f => f.Captain is not null).Select(f => f.Captain);
            if (Settings.Instance.AssignPlayerFormationCaptain && (formations.Any(f => f.Captain == Mission.PlayerAgent)
                                                                || Settings.Instance.AssignNewFormationCaptains
                                                                && allFormations.All(f => f.Captain != Mission.PlayerAgent)))
                prospectiveCaptainsEnumerable = prospectiveCaptainsEnumerable.Append(Mission.PlayerAgent);
            prospectiveCaptainsEnumerable = prospectiveCaptainsEnumerable.OrderByDescending(c => c.Character.GetPower());
            prospectiveCaptainSet = prospectiveCaptainsEnumerable.ToHashSet();
        }
        HashSet<Formation> changedFormations = new();
        int numAgentsSorted = 0;
        if (tierSort)
            foreach (Agent agent in agents)
            {
                if (prospectiveCaptainSet is not null && prospectiveCaptainSet.Contains(agent))
                    continue;
                FormationClass bestFormationClass = FormationClassUtils.GetBestFormationClassForAgent(agent);
                int tier = agent.GetTier();
                int index = bestFormationClass switch
                {
                    FormationClass.Ranged => 3, FormationClass.HorseArcher => 7, FormationClass.Infantry => tier <= 2
                        ? 0
                        : tier <= 4
                            ? 1
                            : 2,
                    FormationClass.HeavyInfantry => tier <= 2
                        ? 0
                        : tier <= 4
                            ? 1
                            : 2,
                    FormationClass.Skirmisher => tier <= 2
                        ? 0
                        : tier <= 4
                            ? 1
                            : 2,
                    FormationClass.Cavalry => tier <= 2
                        ? 4
                        : tier <= 4
                            ? 5
                            : 6,
                    FormationClass.LightCavalry => tier <= 2
                        ? 4
                        : tier <= 4
                            ? 5
                            : 6,
                    FormationClass.HeavyCavalry => tier <= 2
                        ? 4
                        : tier <= 4
                            ? 5
                            : 6,
                    _ => 8
                };
                if (TrySetAgentFormation(agent, formations.FirstOrDefault(f => f.Index == index), ref changedFormations))
                    numAgentsSorted++;
            }
        else
        {
            Dictionary<int, HashSet<Agent>> agentsInFormationClasses = new();
            foreach (Agent agent in agents)
            {
                if (prospectiveCaptainSet is not null && prospectiveCaptainSet.Contains(agent))
                    continue;
                FormationClass formationClass = FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers);
                if (agentsInFormationClasses.TryGetValue((int)formationClass, out HashSet<Agent> agentsInFormationClass))
                    _ = agentsInFormationClass.Add(agent);
                else
                    agentsInFormationClasses.Add((int)formationClass, new() { agent });
            }
            foreach (KeyValuePair<int, HashSet<Agent>> keyValuePair in agentsInFormationClasses)
            {
                List<Agent> agentsInFormationClass = keyValuePair.Value.ToList();
                HashSet<Formation> classFormations = null;
                int numAgentsPerFormation, numAgentsLeftOver;
                if (equalSort)
                {
                    numAgentsPerFormation = agentsInFormationClass.Count / formations.Count;
                    numAgentsLeftOver = agentsInFormationClass.Count % formations.Count;
                }
                else
                {
                    FormationClass formationClass = (FormationClass)keyValuePair.Key;
                    classFormations = FormationClassUtils.GetFormationsForFormationClass(formations, formationClass).ToHashSet();
                    if (classFormations.Count == 0)
                    {
                        _ = classesWithMissingFormations.Add(formationClass.GetGameTextString());
                        continue;
                    }
                    numAgentsPerFormation = agentsInFormationClass.Count / classFormations.Count;
                    numAgentsLeftOver = agentsInFormationClass.Count % classFormations.Count;
                }
                foreach (Formation formation in classFormations ?? formations)
                {
                    int numAgentsToMove = numAgentsPerFormation;
                    if (numAgentsLeftOver > 0)
                    {
                        numAgentsToMove++;
                        numAgentsLeftOver--;
                    }
                    if (numAgentsToMove < 1)
                        break;
                    List<Agent> agentsToMove = agentsInFormationClass.GetRange(0, numAgentsToMove);
                    agentsInFormationClass.RemoveRange(0, numAgentsToMove);
                    if (agentsToMove.Count < 1)
                        break;
                    numAgentsSorted += agentsToMove.Count(agent => TrySetAgentFormation(agent, formation, ref changedFormations));
                }
            }
        }
        if (classesWithMissingFormations.Count > 0)
            _ = MessageBox.Show(
                $"You are missing {(classesWithMissingFormations.Count > 1 ? "set formations" : "a set formation")}"
              + $" for the formation {(classesWithMissingFormations.Count > 1 ? "classes" : "class")}"
              + $" '{string.Join(", ", classesWithMissingFormations)}' within mod settings; sorting will not act as expected!",
                "Formation Sorter encountered an issue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        if (filledFormations.Count > 0)
            foreach (Formation formation in emptyFormations)
            {
                FormationClass formationClass = formation.GetFormationClass();
                Formation forCopy = filledFormations.FirstOrDefault(f => f.CountOfUnits > 0 && f.GetFormationClass() == formationClass)
                                 ?? filledFormations.FirstOrDefault(f => f.CountOfUnits > 0 && f.GetFormationClass() == formationClass.FallbackClass())
                                 ?? filledFormations.FirstOrDefault(f => f.CountOfUnits > 0 && f.GetFormationClass() == formationClass.AlternativeClass())
                                 ?? filledFormations.FirstOrDefault(f => f.CountOfUnits > 0);
                if (forCopy is null)
                    continue;
                _ = typeof(Formation).GetCachedMethod("CopyOrdersFrom").Invoke(formation, new object[] { forCopy });
                formation.SetPositioning(forCopy.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None), forCopy.Direction,
                    forCopy.UnitSpacing);
            }
        if (Settings.Instance.AssignFormationCaptains && prospectiveCaptainSet.Count > 0)
        {
            int filledFormationCount = formations.Count(f => f.CountOfUnits > 0);
            List<Agent> prospectiveCaptains = prospectiveCaptainSet.ToList();
            for (int i = prospectiveCaptains.Count - 1; i >= filledFormationCount; i--)
                prospectiveCaptains.RemoveAt(i);
            HashSet<Formation> captainSetFormations = new();
            HashSet<Formation> captainChangedFormations = new();
            numAgentsSorted += prospectiveCaptains.Count(agent => TrySetCaptainFormation(agent,
                FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers, true), formations, ref changedFormations, allFormations,
                ref captainChangedFormations, ref captainSetFormations));
            if (captainChangedFormations.Any())
                foreach (OrderTroopItemVM troopItem in Mission.MissionOrderVM.TroopController.TroopList)
                    if (captainChangedFormations.Contains(troopItem.Formation))
                        _ = typeof(OrderTroopItemVM).GetCachedMethod("UpdateCommanderInfo").Invoke(troopItem, new object[] { });
        }
        foreach (Formation formation in changedFormations)
            formation.Team.TriggerOnFormationsChanged(formation);
        foreach (Formation formation in formations)
            formation.OnMassUnitTransferEnd();
        if (numAgentsSorted > 0)
            Mission.MissionOrderVM.OnOrderLayoutTypeChanged();
        bool? gesturesEnabled = null;
        if (orderController is null)
            return numAgentsSorted;
        if (numAgentsSorted == 0)
            gesturesEnabled = orderController.BackupAndDisableGesturesEnabled();
        _ = typeof(OrderController).GetCachedMethod("AfterSetOrder").Invoke(orderController, new object[] { OrderType.Transfer });
        if (gesturesEnabled.HasValue)
            orderController.RestoreGesturesEnabled(gesturesEnabled.Value);
        return numAgentsSorted;
    }

    private static int GetTier(this IAgent agent)
        => agent?.Character is null
            ? 0
            : MathF.Min(MathF.Max(MathF.Ceiling((agent.Character.Level - 5f) / 5f), 0), 7); // from Helpers.CharacterHelper.GetCharacterTier

    private static IEnumerable<Agent> EnumerateAgentsInFormations(IEnumerable<Formation> formations)
    {
        foreach (Formation formation in formations.Where(formation => !formation.IsAIControlled))
        {
            foreach (Agent agent in formation.DetachedUnits.Where(CheckAgent))
                yield return agent;
            foreach (Agent agent in formation.Arrangement.GetAllUnits().Cast<Agent>().Where(CheckAgent))
                yield return agent;
        }
    }

    private static bool CheckAgent(Agent agent) => agent != Mission.PlayerAgent && agent.IsHuman;

    private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation, ref HashSet<Formation> changedFormations)
    {
        if (agent is null || desiredFormation is null)
            return false;
        Formation currentFormation = agent.Formation;
        if (currentFormation == desiredFormation)
            return false;
        agent.StopUsingGameObject(); // to prevent siege engine issues
        agent.Formation = desiredFormation;
        _ = changedFormations.Add(currentFormation);
        _ = changedFormations.Add(desiredFormation);
        if (!Mission.Current.IsOrderGesturesEnabled())
            return true;
        switch (desiredFormation.GetFormationClass()) // units will yell out the formation they change to, because why not?
        {
            case FormationClass.Infantry:
            case FormationClass.HeavyInfantry:
                agent.MakeVoice(SkinVoiceManager.VoiceType.Infantry, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                break;
            case FormationClass.Ranged:
            case FormationClass.Skirmisher:
                agent.MakeVoice(SkinVoiceManager.VoiceType.Archers, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                break;
            case FormationClass.Cavalry:
            case FormationClass.LightCavalry:
            case FormationClass.HeavyCavalry:
                agent.MakeVoice(SkinVoiceManager.VoiceType.Cavalry, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                break;
            case FormationClass.HorseArcher:
                agent.MakeVoice(SkinVoiceManager.VoiceType.HorseArchers, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                break;
            default:
                agent.MakeVoice(SkinVoiceManager.VoiceType.MpAffirmative, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                break;
        }
        return true;
    }

    private enum SortAgentsSpecialResult { AIControlled = -2, Ignored = -1 }
}