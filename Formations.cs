using System.Collections.Generic;
using System.Linq;
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
            if (!(MissionOrderVM is null))
            {
                MissionOrderVM.TryCloseToggleOrder();
            }
            int numUnitsSorted = SortAgentsBetweenFormations(GetSelectedFormations());
            if (numUnitsSorted > 0)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Sorted {numUnitsSorted} {(numUnitsSorted == 1 ? "unit" : "units")} between selected formations", Colors.Cyan, "FormationEdit"));
            }
        }

        public static List<Formation> GetSelectedFormations()
        {
            return Mission.Current?.PlayerTeam?.PlayerOrderController?.SelectedFormations?.ToList() ?? new List<Formation>();
        }

        private static int SortAgentsBetweenFormations(List<Formation> formations)
        {
            if (formations is null || formations.Count < 2) return 0;
            int numUnitsSorted = 0;
            foreach (Formation formation in formations)
            {
                foreach (Agent agent in formation.Arrangement.GetAllUnits())
                {
                    if (!agent.IsPlayerControlled)
                    {
                        continue;
                    }
                    Agent mount = agent.MountAgent;
                    if (!(mount is null) && !(mount.RiderAgent is null) && mount.Health > 0 && mount.IsActive() && agent.CanReachAgent(mount))
                    {
                        Formation horseArcher = formations.Find(f => f.FormationIndex == FormationClass.HorseArcher);
                        Formation cavalry = formations.Find(f => f.FormationIndex == FormationClass.Cavalry);
                        if (agent.GetHasRangedWeapon(true))
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
                        if (agent.GetHasRangedWeapon(true))
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

        private static bool TrySetAgentFormation(Agent agent, Formation desiredFormation)
        {
            if (desiredFormation is null || agent.Formation == desiredFormation) return false;
            agent.Formation = desiredFormation;
            agent.SetShouldCatchUpWithFormation(true);
            agent.UpdateFormationOrders();
            return true;
        }
    }
}