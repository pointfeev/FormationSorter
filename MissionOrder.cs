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
    public static class MissionOrder
    {
        public static int OrderSetIndex;
        public static MissionOrderVM MissionOrderVM;

        public static void OnOrderHotkeyPressed()
        {
            if (!IsCurrentMissionReady()) return;
            if (!CanSortOrderBeUsedInCurrentMission()) return;
            UpdateFormations();
            List<Formation> selectedFormations = Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList();
            if (selectedFormations is null || !selectedFormations.Any()) return;
            int numUnitsSorted = SortAgentsBetweenFormations(selectedFormations);
            if (numUnitsSorted == -1) return;
            if (numUnitsSorted > 0)
            {
                Mission.Current?.PlayerTeam?.Leader?.MakeVoice(SkinVoiceManager.VoiceType.MpRegroup, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                UpdateFormations();
                InformationManager.DisplayMessage(new InformationMessage($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "troop" : "troops")} between the selected formations", Colors.Cyan, "FormationSorter"));
            }
            else if (numUnitsSorted == -2)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Formations controlled by AI cannot be sorted", Colors.Cyan, "FormationSorter"));
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage($"No troops needed sorting between the selected formations", Colors.Cyan, "FormationSorter"));
            }
            MissionOrderVM.TryCloseToggleOrder();
        }

        public static bool CanSortOrderBeUsedInCurrentMission()
        {
            return true;
        }

        public static bool IsCurrentMissionSiege()
        {
            if (Mission.Current is null) return false;
            SiegeMissionController siegeMissionController = Mission.Current?.GetMissionBehaviour<SiegeMissionController>();
            if (siegeMissionController is null) return false;
            if (siegeMissionController?.IsSallyOut is true) return false;
            return true;
        }

        public static bool IsCurrentMissionReady()
        {
            if (Mission.Current is null) return false;
            if (MissionOrderVM is null) return false;
            if (MissionOrderVM?.OrderController is null) return false;
            if (MissionOrderVM?.TroopController is null) return false;
            return true;
        }

        private static void UpdateFormations()
        {
            foreach (Formation formation in Mission.Current?.PlayerTeam.FormationsIncludingSpecialAndEmpty)
            {
                formation.ApplyActionOnEachUnit(delegate (Agent agent)
                {
                    agent.UpdateCachedAndFormationValues(false, false);
                });
                Mission.Current.SetRandomDecideTimeOfAgentsWithIndices(formation.CollectUnitIndices(), null, null);
            }
        }

        private static int SortAgentsBetweenFormations(List<Formation> formations)
        {
            if (!CanSortOrderBeUsedInCurrentMission()) return -1;
            if (formations is null || formations.Count < 2) return -1;
            if (formations.All(f => f.IsAIControlled)) return -2;
            int numUnitsSorted = 0;
            foreach (Agent agent in GetAllAgentsInFormations(formations))
            {
                if (!agent.IsHuman) continue;
                FormationClass formationClass = GetBestFormationClassForAgent(agent);
                if (TrySetAgentFormation(agent, formations.Find(f => f.PrimaryClass == formationClass)))
                {
                    numUnitsSorted++;
                }
            }
            return numUnitsSorted;
        }

        private static List<Agent> GetAllAgentsInFormations(List<Formation> formations)
        {
            List<Agent> agents = new List<Agent>();
            if (!CanSortOrderBeUsedInCurrentMission()) return agents;
            foreach (Formation formation in formations)
            {
                if (formation.IsAIControlled) continue;
                agents.AddRange(((List<Agent>)typeof(Formation).GetField("_detachedUnits", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(formation)).FindAll(agent => !agents.Contains(agent) && agent.IsHuman));
                agents.AddRange(from unit in formation.Arrangement.GetAllUnits()
                                where !(unit as Agent is null) && !agents.Contains(unit as Agent) && (unit as Agent).IsHuman
                                select unit as Agent);
            }
            agents = agents.Distinct().ToList();
            foreach (Agent agent in agents)
            {
                agent.StopUsingGameObject(true, true);
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
                else if (Hotkeys.ModifierKey.IsDown() && agent.GetHasRangedWeapon(true))
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
            switch (agent.Formation.PrimaryClass) // units will yell out the formation they change to, because why not?
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