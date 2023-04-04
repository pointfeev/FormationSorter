using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
        _ = formations.RemoveAll(f => FormationClassUtils.GetFormationClass(f) > FormationClass.NumberOfRegularFormations);
        return !formations.Any() ? null : formations;
    }

    private static int SortAgentsBetweenFormations(List<Formation> formations, bool tierSort = false)
    {
        if (!Mission.IsCurrentValid() || formations is null || formations.Count < 2)
            return (int)SortAgentsSpecialResult.Ignored;
        if (formations.All(f => f.IsAIControlled))
            return (int)SortAgentsSpecialResult.AIControlled;
        formations.ForEach(f => f.OnMassUnitTransferStart());
        int numAgentsSorted = 0;
        IEnumerable<Agent> agents = EnumerateAgentsInFormations(formations);
        if (tierSort)
            foreach (Agent agent in agents)
            {
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
            bool useShields = Settings.Instance.ShieldSortKey.IsDefinedAndDown();
            bool useSkirmishers = Settings.Instance.SkirmisherSortKey.IsDefinedAndDown();
            const bool useCompanions = true;
            Dictionary<int, List<Agent>> agentsInFormationClasses = new();
            foreach (Agent agent in agents)
            {
                FormationClass formationClass = FormationClassUtils.GetBestFormationClassForAgent(agent, useShields, useSkirmishers, useCompanions);
                if (agentsInFormationClasses.TryGetValue((int)formationClass, out List<Agent> agentsInFormationClass))
                    agentsInFormationClass.Add(agent);
                else
                    agentsInFormationClasses.Add((int)formationClass, new() { agent });
            }
            bool equalSort = Settings.Instance.EqualSortKey.IsDefinedAndDown();
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
                        _ = MessageBox.Show(
                            $"You are missing a set formation for the formation class '{formationClass.GetGameTextString()}' within mod settings! Sorting will not act as expected!",
                            "Formation Sorter encountered an issue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        formations.ForEach(f => f.OnMassUnitTransferEnd());
        return numAgentsSorted;
    }

    private static int GetTier(this IAgent agent)
        => agent?.Character is null
            ? 0
            : MathF.Min(MathF.Max(MathF.Ceiling((agent.Character.Level - 5f) / 5f), 0), 7); // from Helpers.CharacterHelper.GetCharacterTier

    private static IEnumerable<Agent> EnumerateAgentsInFormations(IEnumerable<Formation> formations)
    {
        if (!Mission.IsCurrentValid())
            yield break;
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
        if (!Mission.IsCurrentValid() || agent is null || desiredFormation is null || currentFormation == desiredFormation)
            return false;
        agent.StopUsingGameObject(); // to prevent siege engine issues
        agent.Formation = desiredFormation;
        if (!Mission.Current.IsOrderGesturesEnabled())
            return true;
        switch (FormationClassUtils.GetFormationClass(desiredFormation)) // units will yell out the formation they change to, because why not?
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