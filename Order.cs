using System;
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
        List<Formation> formations = tierSort ? GetAllRegularFormations() : GetSelectedRegularFormations();
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

    private static List<Formation> GetAllRegularFormations()
    {
        List<Formation> formations = Mission.Current?.PlayerTeam?.FormationsIncludingEmpty?.ToList();
        return formations?.Any() != true ? null : formations;
    }

    private static List<Formation> GetSelectedRegularFormations()
    {
        List<Formation> formations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList();
        if (formations is null)
            return null;
        _ = formations.RemoveAll(f => f.GetFormationClass() > FormationClass.NumberOfRegularFormations);
        return !formations.Any() ? null : formations;
    }

    private static bool TrySetCaptainFormation(Agent agent, FormationClass formationClass, List<Formation> formations,
        ref List<Formation> captainChangedFormations, ref List<Formation> captainSetFormations)
    {
        List<Formation> allFormations = Mission.Current?.PlayerTeam?.FormationsIncludingSpecialAndEmpty;
        foreach (Formation classFormation in FormationClassUtils.GetFormationsForFormationClass(formations, formationClass))
            if (!captainSetFormations.Contains(classFormation) && agent == Mission.PlayerAgent || TrySetAgentFormation(agent, classFormation))
            {
                captainSetFormations.Add(classFormation);
                if (classFormation.Captain == agent)
                    return false;
                if (allFormations is not null)
                    foreach (Formation formation in allFormations.Where(formation => formation.Captain == agent))
                    {
                        formation.Captain = null;
                        captainChangedFormations.Add(classFormation);
                    }
                classFormation.Captain = agent;
                captainChangedFormations.Add(classFormation);
                return true;
            }
        foreach (Formation classFormation in FormationClassUtils.GetFormationsForFormationClass(formations, formationClass.FallbackClass()))
            if (!captainSetFormations.Contains(classFormation) && agent == Mission.PlayerAgent || TrySetAgentFormation(agent, classFormation))
            {
                captainSetFormations.Add(classFormation);
                if (classFormation.Captain == agent)
                    return false;
                if (allFormations is not null)
                    foreach (Formation formation in allFormations.Where(formation => formation.Captain == agent))
                    {
                        formation.Captain = null;
                        captainChangedFormations.Add(classFormation);
                    }
                classFormation.Captain = agent;
                captainChangedFormations.Add(classFormation);
                return true;
            }
        foreach (Formation classFormation in FormationClassUtils.GetFormationsForFormationClass(formations, formationClass.AlternativeClass()))
            if (!captainSetFormations.Contains(classFormation) && agent == Mission.PlayerAgent || TrySetAgentFormation(agent, classFormation))
            {
                captainSetFormations.Add(classFormation);
                if (classFormation.Captain == agent)
                    return false;
                if (allFormations is not null)
                    foreach (Formation formation in allFormations.Where(formation => formation.Captain == agent))
                    {
                        formation.Captain = null;
                        captainChangedFormations.Add(classFormation);
                    }
                classFormation.Captain = agent;
                captainChangedFormations.Add(classFormation);
                return true;
            }
        return false;
    }

    private static int SortAgentsBetweenFormations(List<Formation> formations, bool tierSort, bool equalSort, bool useShields, bool useSkirmishers)
    {
        if (formations is null || formations.Count < 2)
            return (int)SortAgentsSpecialResult.Ignored;
        if (formations.All(f => f.IsAIControlled))
            return (int)SortAgentsSpecialResult.AIControlled;
        OrderController orderController = Mission.MissionOrderVM.OrderController;
        _ = typeof(OrderController).GetCachedMethod("BeforeSetOrder").Invoke(orderController, new object[] { OrderType.Transfer });
        formations.ForEach(f => f.OnMassUnitTransferStart());
        List<string> classesWithMissingFormations = new();
        List<Formation> emptyFormations = formations.Where(f => f.CountOfUnits == 0).ToList();
        List<Formation> filledFormations = formations.Except(emptyFormations).ToList();
        List<Formation> captainSetFormations = new();
        List<Formation> captainChangedFormations = new();
        List<Agent> agents = EnumerateAgentsInFormations(formations).ToList();
        List<Agent> prospectiveCaptains = (Settings.Instance.AssignNewFormationCaptains
            ? agents.Where(a => a.IsHero)
            : formations.Where(f => f.Captain is not null).Select(f => f.Captain)).ToList();
        Team team = formations.FirstOrDefault()?.Team;
        object[] parameters = { team, prospectiveCaptains };
        _ = typeof(GeneralsAndCaptainsAssignmentLogic).GetCachedMethod("SortCaptainsByPriority")
           .Invoke(Mission.Current.MissionBehaviors.FirstOrDefault(b => b is GeneralsAndCaptainsAssignmentLogic), parameters);
        prospectiveCaptains = (List<Agent>)parameters[1];
        List<Agent> assignedCaptains = new();
        int numAgentsSorted = 0;
        foreach (Agent agent in prospectiveCaptains.Where(agent => TrySetCaptainFormation(agent,
                     FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers), formations, ref captainChangedFormations,
                     ref captainSetFormations)))
        {
            numAgentsSorted++;
            assignedCaptains.Add(agent);
        }
        if (captainChangedFormations.Any())
            foreach (OrderTroopItemVM troopItem in Mission.MissionOrderVM.TroopController.TroopList)
                if (captainChangedFormations.Contains(troopItem.Formation))
                    _ = typeof(OrderTroopItemVM).GetCachedMethod("UpdateCommanderInfo").Invoke(troopItem, new object[] { });
        if (tierSort)
            foreach (Agent agent in agents)
            {
                if (agent == Mission.PlayerAgent || assignedCaptains.Contains(agent))
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
                if (TrySetAgentFormation(agent, formations.Find(f => f.Index == index)))
                    numAgentsSorted++;
            }
        else
        {
            Dictionary<int, List<Agent>> agentsInFormationClasses = new();
            foreach (Agent agent in agents)
            {
                if (agent == Mission.PlayerAgent || assignedCaptains.Contains(agent))
                    continue;
                FormationClass formationClass = FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers);
                if (agentsInFormationClasses.TryGetValue((int)formationClass, out List<Agent> agentsInFormationClass))
                    agentsInFormationClass.Add(agent);
                else
                    agentsInFormationClasses.Add((int)formationClass, new() { agent });
            }
            foreach (KeyValuePair<int, List<Agent>> keyValuePair in agentsInFormationClasses)
            {
                List<Agent> agentsInFormationClass = keyValuePair.Value;
                List<Formation> classFormations = null;
                int numAgentsPerFormation, numAgentsLeftOver;
                if (equalSort)
                {
                    numAgentsPerFormation = agentsInFormationClass.Count / formations.Count;
                    numAgentsLeftOver = agentsInFormationClass.Count % formations.Count;
                }
                else
                {
                    FormationClass formationClass = (FormationClass)keyValuePair.Key;
                    classFormations = FormationClassUtils.GetFormationsForFormationClass(formations, formationClass).ToList();
                    if (classFormations.Count == 0)
                    {
                        classesWithMissingFormations.Add(formationClass.GetGameTextString());
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
                    numAgentsSorted += agentsToMove.Count(agent => TrySetAgentFormation(agent, formation));
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
            emptyFormations.ForEach(f =>
            {
                FormationClass formationClass = f.GetFormationClass();
                Formation forCopy = filledFormations.FirstOrDefault(f => f.GetFormationClass() == formationClass)
                                 ?? filledFormations.FirstOrDefault(f => f.PrimaryClass == formationClass)
                                 ?? filledFormations.FirstOrDefault(f => f.SecondaryClasses.Contains(formationClass))
                                 ?? filledFormations.FirstOrDefault(f => f.InitialClass == formationClass)
                                 ?? filledFormations.FirstOrDefault(f => f.GetFormationClass().FallbackClass() == formationClass.FallbackClass())
                                 ?? filledFormations.FirstOrDefault();
                if (forCopy is null)
                    return;
                _ = typeof(Formation).GetCachedMethod("CopyOrdersFrom").Invoke(f, new object[] { forCopy });
                f.SetPositioning(forCopy.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.None), forCopy.Direction, forCopy.UnitSpacing);
            });
        formations.ForEach(f =>
        {
            if (numAgentsSorted > 0)
                f.Team.TriggerOnFormationsChanged(f);
            f.OnMassUnitTransferEnd();
        });
        if (numAgentsSorted > 0)
            Mission.MissionOrderVM.OnOrderLayoutTypeChanged();
        bool? gesturesEnabled = null;
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

    private static IEnumerable<Agent> EnumerateAgentsInFormations(List<Formation> formations)
    {
        yield return Mission.PlayerAgent;
        foreach (Formation formation in formations.Where(formation => !formation.IsAIControlled))
        {
            foreach (Agent agent in formation.DetachedUnits.Where(CheckAgent))
                yield return agent;
            foreach (Agent agent in formation.Arrangement.GetAllUnits().Cast<Agent>().Where(CheckAgent))
                yield return agent;
        }
    }

    private static bool CheckAgent(Agent agent) => agent != Mission.PlayerAgent && agent.IsHuman;

    private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation)
    {
        Formation currentFormation = agent?.Formation;
        if (agent is null || desiredFormation is null || currentFormation == desiredFormation)
            return false;
        agent.StopUsingGameObject(); // to prevent siege engine issues
        agent.Formation = desiredFormation;
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