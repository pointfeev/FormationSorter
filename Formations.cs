using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;

namespace FormationSorter
{
    public static class Formations
    {
        public static int UniqueId;
        public static GameKey Hotkey;
        public static int OrderSetIndex;
        public static MissionOrderVM MissionOrderVM;

        public static void OnHotkeyPressed()
        {
            if (MissionOrderVM is null) return;
            UpdateFormations();
            List<Formation> selectedFormations = GetSelectedFormations();
            if (!selectedFormations.Any()) return;
            MissionOrderVM.OrderController.ClearSelectedFormations();
            Mission.Current?.PlayerTeam?.Leader?.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
            int numUnitsSorted = SortAgentsBetweenFormations(selectedFormations);
            if (numUnitsSorted > 0)
            {
                UpdateFormations();
                InformationManager.DisplayMessage(new InformationMessage($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "unit" : "units")} between selected formations", Colors.Cyan, "FormationEdit"));
            }
            MissionOrderVM.TryCloseToggleOrder();
        }

        public static List<Formation> GetSelectedFormations()
        {
            return Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList() ?? new List<Formation>();
        }

        private static void UpdateFormations()
        {
            Mission mission = Mission.Current;
            if (mission is null) return;
            foreach (Formation formation in mission.PlayerTeam.FormationsIncludingSpecialAndEmpty)
            {
                formation.ApplyActionOnEachUnit(delegate (Agent agent)
                {
                    agent.UpdateCachedAndFormationValues(false, false);
                });
                mission?.SetRandomDecideTimeOfAgentsWithIndices(formation.CollectUnitIndices(), null, null);
            }
        }

        private static int SortAgentsBetweenFormations(List<Formation> formations)
        {
            try
            {
                if (formations is null || formations.Count < 2) return 0;
                int numUnitsSorted = 0;
                foreach (Formation formation in formations)
                {
                    if (formation.IsAIControlled) continue;
                    List<Agent> agents = ((List<Agent>)typeof(Formation).GetField("_detachedUnits", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(formation)).ToList();
                    agents.AddRange(from unit in formation.Arrangement.GetAllUnits()
                                    where !(unit as Agent is null)
                                    select unit as Agent);
                    foreach (Agent agent in agents.Distinct())
                    {
                        if (!agent.IsHuman) continue;
                        Agent mount = agent.MountAgent;
                        Agent rider = mount?.RiderAgent;
                        if (rider == agent && mount.Health > 0 && mount.IsActive() && agent.CanReachAgent(mount))
                        {
                            Formation horseArcher = formations.Find(f => f.FormationIndex == FormationClass.HorseArcher);
                            Formation cavalry = formations.Find(f => f.FormationIndex == FormationClass.Cavalry);
                            if (AgentHasProperRangedWeaponWithAmmo(agent))
                            {
                                if (TrySetAgentFormation(agent, horseArcher)) numUnitsSorted++;
                                continue;
                            }
                            else if (agent.HasMeleeWeaponCached)
                            {
                                if (TrySetAgentFormation(agent, cavalry)) numUnitsSorted++;
                                continue;
                            }
                        }
                        else
                        {
                            Formation ranged = formations.Find(f => f.FormationIndex == FormationClass.Ranged);
                            Formation infantry = formations.Find(f => f.FormationIndex == FormationClass.Infantry);
                            if (AgentHasProperRangedWeaponWithAmmo(agent))
                            {
                                if (TrySetAgentFormation(agent, ranged)) numUnitsSorted++;
                                continue;
                            }
                            else if (agent.HasMeleeWeaponCached)
                            {
                                if (TrySetAgentFormation(agent, infantry)) numUnitsSorted++;
                                continue;
                            }
                        }
                        /*if (!agent.IsRetreating()) // to retreat agents that don't have weapons? may cause unintended behaviour so it's commented out for now
                        {
                            agent.Retreat(agent.Mission.GetClosestFleePositionForAgent(agent));
                            numUnitsSorted++;
                            continue;
                        }*/
                    }
                }
                return numUnitsSorted;
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
                return 0;
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
            if (agent is null || desiredFormation is null || agent.Formation == desiredFormation) return false;
            agent.Formation = desiredFormation;
            switch (agent.Formation.InitialClass) // units will yell out the formation they change to, because why not?
            {
                case FormationClass.Infantry:
                case FormationClass.HeavyInfantry:
                    agent.MakeVoice(SkinVoiceManager.VoiceType.Infantry, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                    break;

                case FormationClass.Ranged:
                case FormationClass.NumberOfDefaultFormations:
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
                    break;
            }
            return true;
        }
    }
}