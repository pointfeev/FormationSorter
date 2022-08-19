using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class Order
    {
        public static void OnApplicationTick()
        {
            if (!Mission.IsCurrentValid() || !Mission.IsCurrentOrderable()) return;
            if (Mission.OrderSetVM is null)
                Mission.OrderSetVM = (OrderSetVM)typeof(OrderSetVM).GetCachedConstructor(new Type[] {
                    typeof(OrderSubType), typeof(int), typeof(Action<OrderItemVM, OrderSetType, bool>), typeof(bool)
                }).Invoke(new object[] {
                    OrderSubType.None, 0, (Action<OrderItemVM, OrderSetType, bool>)((OrderItemVM o, OrderSetType or, bool b) => { }), false
                });

            if (Mission.InputKeyItemVM is null)
                Mission.InputKeyItemVM = (InputKeyItemVM)typeof(InputKeyItemVM).GetCachedConstructor(new Type[0]).Invoke(new object[0]);

            InputKey OrderKey = Settings.OrderKey;
            string Key = OrderKey.ToString();
            Mission.InputKeyItemVM.KeyID = Key;
            Mission.InputKeyItemVM.KeyName = Key;
            Mission.InputKeyItemVM.IsVisible = OrderKey.IsDefined();

            Mission.OrderSetVM.TitleOrderKey = Mission.InputKeyItemVM;
            Mission.OrderSetVM.TitleOrder.ShortcutKey = Mission.InputKeyItemVM;
            Mission.OrderSetVM.TitleOrder.IsTitle = true;
            Mission.OrderSetVM.TitleText = "Sort Troops Between Formations";
            Mission.OrderSetVM.TitleOrder.OrderIconID = "ToggleAI";
            Mission.OrderSetVM.TitleOrder.TooltipText = "Sort Troops Between Formations";
            Mission.OrderSetVM.TitleOrder.IsActive = true;
            Mission.OrderSetVM.OnFinalize(); // we have our own code to deal with key presses

            if (!Mission.MissionOrderVM.OrderSets.Contains(Mission.OrderSetVM))
                Mission.MissionOrderVM.OrderSets.Add(Mission.OrderSetVM);
        }

        public static void OnOrder(bool tierSort = false)
        {
            if (!Mission.IsCurrentValid() || !Mission.IsCurrentOrderable()) return;

            List<Formation> formations = tierSort ? GetAllRegularFormations() : GetSelectedRegularFormations();
            if (formations is null) return;

            int numUnitsSorted = SortAgentsBetweenFormations(formations, tierSort: tierSort);
            if (numUnitsSorted == -1) return;
            else if (numUnitsSorted == -2)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Formations controlled by AI cannot be sorted", Colors.White, "FormationSorter"));
                return;
            }
            else if (numUnitsSorted > 0)
            {
                if (Mission.Current.IsOrderShoutingAllowed())
                    Mission.PlayerAgent.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                InformationManager.DisplayMessage(new InformationMessage($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "troop" : "troops")} between the selected formations", Colors.White, "FormationSorter"));
            }
            else if (tierSort)
                InformationManager.DisplayMessage(new InformationMessage($"No troops need tier sorting", Colors.White, "FormationSorter"));
            else
                InformationManager.DisplayMessage(new InformationMessage($"No troops need sorting between the selected formations", Colors.White, "FormationSorter"));
            _ = Mission.MissionOrderVM.TryCloseToggleOrder();
        }

        private static List<Formation> GetAllRegularFormations()
        {
            List<Formation> formations = Mission.Current?.PlayerTeam?.FormationsIncludingEmpty?.ToList();
            return (formations is null || !formations.Any()) ? null : formations;
        }

        private static List<Formation> GetSelectedRegularFormations()
        {
            List<Formation> formations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList();
            if (formations is null) return null;
            _ = formations.RemoveAll(f => f.FormationIndex > FormationClass.NumberOfRegularFormations);
            return !formations.Any() ? null : formations;
        }

        private static int SortAgentsBetweenFormations(List<Formation> formations, bool tierSort = false)
        {
            if (!Mission.IsCurrentOrderable() || formations is null || formations.Count < 2)
                return -1;
            if (formations.All(f => f.IsAIControlled))
                return -2;
            int numAgentsSorted = 0;
            List<Agent> agents = GetAllPlayerControlledHumanAgentsInFormations(formations);
            if (tierSort)
            {
                foreach (Agent agent in agents)
                {
                    FormationClass bestFormationClass = GetBestFormationClassForAgent(agent);
                    int tier = agent.GetTier();
                    if (bestFormationClass == FormationClass.Cavalry)
                        bestFormationClass = tier <= 2 ? FormationClass.Cavalry : tier <= 4 ? FormationClass.LightCavalry : FormationClass.HeavyCavalry;
                    else if (bestFormationClass == FormationClass.Infantry)
                        bestFormationClass = tier <= 2 ? FormationClass.Skirmisher : tier <= 4 ? FormationClass.Infantry : FormationClass.HeavyInfantry;
                    if (TrySetAgentFormation(agent, GetBestFormationForFormationClass(formations, bestFormationClass)))
                        numAgentsSorted++;
                }
            }
            else
            {
                bool useShields = Settings.ShieldSortingModifierKey.IsDefinedAndDown();
                bool useSkirmishers = Settings.SkirmisherSortingModifierKey.IsDefinedAndDown();
                if (Settings.EqualSortingModifierKey.IsDefinedAndDown())
                {
                    Dictionary<FormationClass, List<Agent>> agentsInFormationClasses = new Dictionary<FormationClass, List<Agent>>();
                    foreach (Agent agent in agents)
                    {
                        FormationClass formationClass = GetBestFormationClassForAgent(agent, useShields: useShields, useSkirmishers: useSkirmishers);
                        if (agentsInFormationClasses.TryGetValue(formationClass, out List<Agent> agentsInFormationClass))
                            agentsInFormationClass.Add(agent);
                        else
                            agentsInFormationClasses.Add(formationClass, new List<Agent>() { agent });
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
                            foreach (Agent agent in agentsToMove)
                                if (TrySetAgentFormation(agent, formation))
                                    numAgentsSorted++;
                        }
                    }
                }
                else
                {
                    foreach (Agent agent in agents)
                    {
                        FormationClass formationClass = GetBestFormationClassForAgent(agent, useShields: useShields, useSkirmishers: useSkirmishers);
                        if (TrySetAgentFormation(agent, GetBestFormationForFormationClass(formations, formationClass)))
                            numAgentsSorted++;
                    }
                }
            }
            return numAgentsSorted;
        }

        private static int GetTier(this Agent agent) => agent is null || agent.Character is null ? 0
            : MathF.Min(MathF.Max(MathF.Ceiling((agent.Character.Level - 5f) / 5f), 0), 7); // from Helpers.CharacterHelper.GetCharacterTier

        private static List<Agent> GetAllPlayerControlledHumanAgentsInFormations(List<Formation> formations)
        {
            List<Agent> readAgents = new List<Agent>();
            if (!Mission.IsCurrentOrderable()) return readAgents;
            foreach (Formation formation in formations)
            {
                if (formation.IsAIControlled) continue;
                readAgents.AddRange(((List<Agent>)typeof(Formation).GetCachedField("_detachedUnits").GetValue(formation)).FindAll(agent => agent.IsHuman));
                readAgents.AddRange(from unit in formation.Arrangement.GetAllUnits()
                                    where !(unit as Agent is null) && (unit as Agent).IsHuman
                                    select unit as Agent);
            }
            List<Agent> agents = new List<Agent>();
            foreach (Agent agent in readAgents)
            {
                if (agent == Mission.PlayerAgent) continue;
                if (!agents.Contains(agent))
                {
                    agents.Add(agent);
                    agent.StopUsingGameObject(true, true); // to prevent siege engine issues
                }
            }
            return agents;
        }

        private static FormationClass GetBestFormationClassForAgent(Agent agent, bool useShields = false, bool useSkirmishers = false)
        {
            agent.UpdateCachedAndFormationValues(false, false);
            Agent mount = agent.MountAgent;
            return (!(mount is null) && mount.Health > 0 && mount.IsActive() && (agent.CanReachAgent(mount) || agent.GetTargetAgent() == mount))
                ? (agent.IsRangedCached ? FormationClass.HorseArcher : FormationClass.Cavalry)
                : agent.IsRangedCached ? FormationClass.Ranged
                : (useSkirmishers && agent.HasThrownCached || useShields && !agent.HasShieldCached) ? FormationClass.Skirmisher
                : FormationClass.Infantry;
        }

        private static Formation GetBestFormationForFormationClass(List<Formation> formations, FormationClass formationClass)
        {
            foreach (Formation formation in formations)
                if (formation.FormationIndex == formationClass)
                    return formation;
            return null;
        }

        private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation)
        {
            if (!Mission.IsCurrentOrderable() || agent is null || desiredFormation is null || agent.Formation == desiredFormation)
                return false;
            agent.Formation = desiredFormation;
            if (Mission.Current.IsOrderShoutingAllowed())
            {
                switch (desiredFormation.FormationIndex)
                { // units will yell out the formation they change to, because why not?
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
            }
            return true;
        }
    }
}