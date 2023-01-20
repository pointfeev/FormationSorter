using System;
using System.Collections.Generic;
using System.Linq;
using FormationSorter.Utilities;
using TaleWorlds.Core;
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
                = new(OrderSubType.None, OrderSetType.None, new(OrderText), (vm, b) => { }) { ShortcutKey = Mission.InputKeyItemVM, IsTitle = true };
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
                                            .GetCachedConstructor(new[]
                                             {
                                                 typeof(OrderSubType), typeof(int), typeof(Action<OrderItemVM, OrderSetType, bool>), typeof(bool)
                                             }).Invoke(new object[]
                                             {
                                                 OrderSubType.None, 0, (Action<OrderItemVM, OrderSetType, bool>)((o, or, b) => { }), false
                                             });
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

    internal static void OnOrder(bool tierSort = false)
    {
        if (!Mission.IsCurrentValid())
            return;
        List<Formation> formations = tierSort ? GetAllRegularFormations() : GetSelectedRegularFormations();
        if (formations is null)
            return;
        int numUnitsSorted = SortAgentsBetweenFormations(formations, tierSort);
        switch (numUnitsSorted)
        {
            case -1:
                return;
            case -2:
                InformationManager.DisplayMessage(new("Formations controlled by AI cannot be sorted", Colors.White, "FormationSorter"));
                return;
            default:
            {
                if (numUnitsSorted > 0)
                {
                    //if (Mission.Current.IsOrderShoutingAllowed()) // may need replacement for v1.1.0
                    Mission.PlayerAgent.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                    InformationManager.DisplayMessage(new(
                        $"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "troop" : "troops")} between the selected formations", Colors.White,
                        "FormationSorter"));
                }
                else if (tierSort)
                    InformationManager.DisplayMessage(new("No troops need tier sorting", Colors.White, "FormationSorter"));
                else
                    InformationManager.DisplayMessage(new("No troops need sorting between the selected formations", Colors.White, "FormationSorter"));
                break;
            }
        }
        _ = Mission.MissionOrderVM.TryCloseToggleOrder();
    }

    private static List<Formation> GetAllRegularFormations()
    {
        List<Formation> formations = Mission.Current?.PlayerTeam?.FormationsIncludingEmpty?.ToList();
        return formations is null || !formations.Any() ? null : formations;
    }

    private static List<Formation> GetSelectedRegularFormations()
    {
        List<Formation> formations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList();
        if (formations is null)
            return null;
        _ = formations.RemoveAll(f => FormationClassUtils.GetFormationClass(f) > FormationClass.NumberOfRegularFormations);
        return !formations.Any() ? null : formations;
    }

    private static int SortAgentsBetweenFormations(List<Formation> formations, bool tierSort = false)
    {
        if (!Mission.IsCurrentValid() || formations is null || formations.Count < 2)
            return -1;
        if (formations.All(f => f.IsAIControlled))
            return -2;
        int numAgentsSorted = 0;
        List<Agent> agents = GetAllPlayerControlledHumanAgentsInFormations(formations);
        if (tierSort)
            foreach (Agent agent in agents)
            {
                FormationClass bestFormationClass = FormationClassUtils.GetBestFormationClassForAgent(agent);
                int tier = agent.GetTier();
                int index;
                switch (bestFormationClass)
                {
                    case FormationClass.Ranged:
                        index = 3;
                        break;
                    case FormationClass.HorseArcher:
                        index = 7;
                        break;
                    case FormationClass.Infantry:
                    case FormationClass.HeavyInfantry:
                    case FormationClass.Skirmisher:
                        index = tier <= 2
                            ? 0
                            : tier <= 4
                                ? 1
                                : 2;
                        break;
                    case FormationClass.Cavalry:
                    case FormationClass.LightCavalry:
                    case FormationClass.HeavyCavalry:
                        index = tier <= 2
                            ? 4
                            : tier <= 4
                                ? 5
                                : 6;
                        break;
                    default:
                        index = 8;
                        break;
                }
                if (TrySetAgentFormation(agent, formations.Find(f => f.Index == index)))
                    numAgentsSorted++;
            }
        else
        {
            bool useShields = Settings.Instance.ShieldSortKey.IsDefinedAndDown();
            bool useSkirmishers = Settings.Instance.SkirmisherSortKey.IsDefinedAndDown();
            const bool useCompanions = true;
            if (Settings.Instance.EqualSortKey.IsDefinedAndDown())
            {
                Dictionary<FormationClass, List<Agent>> agentsInFormationClasses = new();
                foreach (Agent agent in agents)
                {
                    FormationClass formationClass = FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers, useCompanions);
                    if (agentsInFormationClasses.TryGetValue(formationClass, out List<Agent> agentsInFormationClass))
                        agentsInFormationClass.Add(agent);
                    else
                        agentsInFormationClasses.Add(formationClass, new() { agent });
                }
                foreach (KeyValuePair<FormationClass, List<Agent>> keyValuePair in agentsInFormationClasses)
                {
                    List<Agent> agentsInFormationClass = keyValuePair.Value;
                    int numAgentsPerFormation = agentsInFormationClass.Count / formations.Count;
                    int numAgentsLeftOver = agentsInFormationClass.Count % formations.Count;
                    foreach (Formation formation in formations)
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
                        if (!agentsToMove.Any())
                            break;
                        numAgentsSorted += agentsToMove.Count(agent => TrySetAgentFormation(agent, formation));
                    }
                }
            }
            else
                foreach (Agent agent in agents)
                {
                    FormationClass formationClass = FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers, useCompanions);
                    if (TrySetAgentFormation(agent, FormationClassUtils.GetFormationForFormationClass(formations, formationClass)))
                        numAgentsSorted++;
                }
        }
        return numAgentsSorted;
    }

    private static int GetTier(this IAgent agent)
        => agent?.Character is null
            ? 0
            : MathF.Min(MathF.Max(MathF.Ceiling((agent.Character.Level - 5f) / 5f), 0), 7); // from Helpers.CharacterHelper.GetCharacterTier

    private static List<Agent> GetAllPlayerControlledHumanAgentsInFormations(List<Formation> formations)
    {
        List<Agent> readAgents = new();
        if (!Mission.IsCurrentValid())
            return readAgents;
        foreach (Formation formation in formations)
        {
            if (formation.IsAIControlled)
                continue;
            readAgents.AddRange(((List<Agent>)typeof(Formation).GetCachedField("_detachedUnits").GetValue(formation)).FindAll(agent => agent.IsHuman));
            readAgents.AddRange(from unit in formation.Arrangement.GetAllUnits() where unit is Agent agent && agent.IsHuman select unit as Agent);
        }
        List<Agent> agents = new();
        foreach (Agent agent in readAgents.Where(agent => agent != Mission.PlayerAgent).Where(agent => !agents.Contains(agent)))
        {
            agents.Add(agent);
            agent.StopUsingGameObject(); // to prevent siege engine issues
        }
        return agents;
    }

    private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation)
    {
        if (!Mission.IsCurrentValid() || agent is null || desiredFormation is null || agent.Formation == desiredFormation)
            return false;
        agent.Formation = desiredFormation;
        //if (Mission.Current.IsOrderShoutingAllowed()) // may need replacement for v1.1.0
        switch (FormationClassUtils.GetFormationClass(desiredFormation))
        {
            // units will yell out the formation they change to, because why not?
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
                // best fallback I can find
                break;
        }
        return true;
    }
}