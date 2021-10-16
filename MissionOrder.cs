using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class MissionOrder
    {
        public static MissionOrderVM MissionOrderVM;
        public static OrderSetVM OrderSetVM;
        public static InputKeyItemVM InputKeyItemVM;

        public static void OnApplicationTick(float dt)
        {
            if (!IsCurrentMissionReady()) return;

            if (ctorOrderSetVM is null) ctorOrderSetVM = typeof(OrderSetVM).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
                typeof(OrderSubType), typeof(int), typeof(Action<OrderItemVM, OrderSetType, bool>), typeof(bool)
            }, null);
            if (OrderSetVM is null) OrderSetVM = (OrderSetVM)ctorOrderSetVM.Invoke(new object[] {
                OrderSubType.None, MissionOrderVM.OrderSets.Count, (Action<OrderItemVM, OrderSetType, bool>)((OrderItemVM o, OrderSetType or, bool b) => { }), false
            });

            if (ctorInputKeyItemVM is null) ctorInputKeyItemVM = typeof(InputKeyItemVM).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            if (InputKeyItemVM is null) InputKeyItemVM = (InputKeyItemVM)ctorInputKeyItemVM.Invoke(new object[0]);

            string orderKey = Settings.OrderKey.ToString();
            InputKeyItemVM.KeyID = orderKey;
            InputKeyItemVM.KeyName = orderKey;
            InputKeyItemVM.IsVisible = Settings.OrderKey.IsDefined();

            OrderSetVM.TitleOrderKey = InputKeyItemVM;
            OrderSetVM.TitleOrder.ShortcutKey = InputKeyItemVM;
            OrderSetVM.TitleOrder.IsTitle = true;
            OrderSetVM.TitleText = "Sort Troops Between Formations";
            OrderSetVM.TitleOrder.OrderIconID = "ToggleAI";
            OrderSetVM.TitleOrder.TooltipText = "Sort Troops Between Formations";
            OrderSetVM.TitleOrder.IsActive = true;
            OrderSetVM.OnFinalize(); // we have our own code to deal with key presses

            if (!MissionOrderVM.OrderSets.Contains(OrderSetVM))
            {
                if (indexFieldOrderSetVM is null) indexFieldOrderSetVM = typeof(OrderSetVM).GetField("_index", BindingFlags.NonPublic | BindingFlags.Instance);
                indexFieldOrderSetVM.SetValue(OrderSetVM, MissionOrderVM.OrderSets.Count);
                MissionOrderVM.OrderSets.Add(OrderSetVM);
            }
        }

        private static ConstructorInfo ctorOrderSetVM;
        private static FieldInfo indexFieldOrderSetVM;
        private static ConstructorInfo ctorInputKeyItemVM;

        public static void OnOrderHotkeyPressed()
        {
            if (!IsCurrentMissionReady()) return;
            if (!CanSortOrderBeUsedInCurrentMission()) return;
            List<Formation> selectedFormations = GetSelectedRegularFormations();
            if (selectedFormations is null || !selectedFormations.Any()) return;
            int numUnitsSorted = SortAgentsBetweenFormations(selectedFormations);
            if (numUnitsSorted == -1) return;
            else if (numUnitsSorted == -2)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Formations controlled by AI cannot be sorted", Colors.White, "FormationSorter"));
                return;
            }
            else if (numUnitsSorted > 0)
            {
                Mission.Current.MainAgent.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                InformationManager.DisplayMessage(new InformationMessage($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "troop" : "troops")} between the selected formations", Colors.White, "FormationSorter"));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage($"No troops need sorting between the selected formations", Colors.White, "FormationSorter"));
            }
            MissionOrderVM.TryCloseToggleOrder();
        }

        public static bool CanSortOrderBeUsedInCurrentMission()
        {
            return true;
        }

        public static bool IsCurrentMissionSiege()
        {
            if (!IsCurrentMissionReady()) return false;
            SiegeMissionController siegeMissionController = Mission.Current?.GetMissionBehaviour<SiegeMissionController>();
            if (siegeMissionController is null) return false;
            if (siegeMissionController?.IsSallyOut is true) return false;
            return true;
        }

        public static bool IsCurrentMissionReady()
        {
            if (MissionOrderVM is null) return false;
            if (Mission.Current is null) return false;
            try
            {
                if (MissionOrderVM.OrderController is null) return false;
                if (MissionOrderVM.TroopController is null) return false;
            }
            catch { }
            return true;
        }

        private static List<Formation> GetSelectedRegularFormations()
        {
            List<Formation> selectedFormations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList();
            if (selectedFormations is null || !selectedFormations.Any()) return null;
            selectedFormations.RemoveAll(f => f.PrimaryClass > FormationClass.NumberOfRegularFormations);
            return selectedFormations;
        }

        private static int SortAgentsBetweenFormations(List<Formation> formations)
        {
            if (!CanSortOrderBeUsedInCurrentMission()) return -1;
            if (formations is null || formations.Count < 2) return -1;
            if (formations.All(f => f.IsAIControlled)) return -2;
            int numAgentsSorted = 0;
            List<Agent> agents = GetAllPlayerControlledHumanAgentsInFormations(formations);
            if (Settings.EqualSortingModifierKey.IsDefinedAndDown())
            {
                Dictionary<FormationClass, List<Agent>> agentsInFormationClasses = new Dictionary<FormationClass, List<Agent>>();
                foreach (Agent agent in agents)
                {
                    FormationClass formationClass = GetBestFormationClassForAgent(agent);
                    if (agentsInFormationClasses.TryGetValue(formationClass, out List<Agent> agentsInFormationClass))
                    {
                        agentsInFormationClass.Add(agent);
                    }
                    else
                    {
                        agentsInFormationClasses.Add(formationClass, new List<Agent>() { agent });
                    }
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
                        if (numAgentsToMove < 1) break;
                        List<Agent> agentsToMove = agentsInFormationClass.GetRange(0, numAgentsToMove);
                        agentsInFormationClass.RemoveRange(0, numAgentsToMove);
                        if (!agentsToMove.Any()) break;
                        foreach (Agent agent in agentsToMove)
                        {
                            if (TrySetAgentFormation(agent, formation))
                            {
                                numAgentsSorted++;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Agent agent in agents)
                {
                    FormationClass formationClass = GetBestFormationClassForAgent(agent);
                    if (TrySetAgentFormation(agent, formations.Find(f => f.PrimaryClass == formationClass)))
                    {
                        numAgentsSorted++;
                    }
                }
            }
            return numAgentsSorted;
        }

        private static List<Agent> GetAllPlayerControlledHumanAgentsInFormations(List<Formation> formations)
        {
            List<Agent> readAgents = new List<Agent>();
            if (!CanSortOrderBeUsedInCurrentMission()) return readAgents;
            foreach (Formation formation in formations)
            {
                if (formation.IsAIControlled) continue;
                readAgents.AddRange(((List<Agent>)typeof(Formation).GetField("_detachedUnits", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(formation)).FindAll(agent => agent.IsHuman));
                readAgents.AddRange(from unit in formation.Arrangement.GetAllUnits()
                                    where !(unit as Agent is null) && (unit as Agent).IsHuman
                                    select unit as Agent);
            }
            List<Agent> agents = new List<Agent>();
            foreach (Agent agent in readAgents)
            {
                if (agent == Mission.Current.MainAgent) continue;
                if (!agents.Contains(agent))
                {
                    agents.Add(agent);
                    agent.StopUsingGameObject(true, true); // to prevent siege engine issues
                }
            }
            return agents;
        }

        private static FormationClass GetBestFormationClassForAgent(Agent agent)
        {
            Agent mount = agent.MountAgent;
            Agent rider = mount?.RiderAgent;
            if (rider == agent && mount.Health > 0 && mount.IsActive() && agent.CanReachAgent(mount))
            {
                if (AgentHasProperRangedWeaponWithAmmo(agent))
                {
                    return FormationClass.HorseArcher;
                }
                else
                {
                    return FormationClass.Cavalry;
                }
            }
            else
            {
                if (AgentHasProperRangedWeaponWithAmmo(agent))
                {
                    return FormationClass.Ranged;
                }
                else if (Settings.SkirmisherSortingModifierKey.IsDefinedAndDown() && agent.GetHasRangedWeapon(true))
                {
                    return FormationClass.Skirmisher;
                }
                else
                {
                    return FormationClass.Infantry;
                }
            }
        }

        private static bool AgentHasProperRangedWeaponWithAmmo(Agent agent)
        {
            MissionEquipment equipment = agent.Equipment;
            if (equipment is null) return false;
            bool hasBowWithArrows = equipment.HasRangedWeapon(WeaponClass.Arrow) && equipment.GetAmmoAmount(WeaponClass.Arrow) > 0;
            bool hasCrossbowWithBolts = equipment.HasRangedWeapon(WeaponClass.Bolt) && equipment.GetAmmoAmount(WeaponClass.Bolt) > 0;
            return hasBowWithArrows || hasCrossbowWithBolts;
        }

        private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation)
        {
            if (!CanSortOrderBeUsedInCurrentMission()) return false;
            if (agent is null || desiredFormation is null || agent.Formation == desiredFormation) return false;
            agent.Formation = desiredFormation;
            switch (desiredFormation.PrimaryClass) // units will yell out the formation they change to, because why not?
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
                    agent.MakeVoice(SkinVoiceManager.VoiceType.MpAffirmative, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction); // best fallback I can find
                    break;
            }
            return true;
        }
    }
}