using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    public static class Mission
    {
        public static MissionOrderVM MissionOrderVM;
        public static OrderSetVM OrderSetVM;
        public static InputKeyItemVM InputKeyItemVM;

        public static bool CanPlayerInteract()
        {
            Agent playerAgent = PlayerAgent;
            if (playerAgent is null) return false;
            MissionMainAgentController playerAgentController = PlayerAgentController;
            if (playerAgentController is null) return false;
            MissionMainAgentInteractionComponent interactionComponent = playerAgentController.InteractionComponent;
            if (interactionComponent is null) return false;
            IFocusable currentInteractableObject = (IFocusable)typeof(MissionMainAgentInteractionComponent)
                .GetCachedField("_currentInteractableObject").GetValue(interactionComponent);
            if (currentInteractableObject is null) return false;
            Agent agent = currentInteractableObject as Agent;
            return !(currentInteractableObject is null) && (agent is null || agent.IsMount);
        }

        public static bool IsCurrentValid()
        {
            TaleWorlds.MountAndBlade.Mission current = Current;
            if (current is null) return false;
            if (current.Mode != MissionMode.Battle) return false;
            if (current.MissionEnded()) return false;
            if (MissionOrderVM is null) return false;
            try
            {
                if (MissionOrderVM.OrderController is null) return false;
                if (MissionOrderVM.TroopController is null) return false;
            }
            catch // to catch errors that are entirely out of my control
            {
                return false;
            }
            return true;
        }

        public static bool IsCurrentSiege()
        {
            SiegeMissionController siegeMissionController = Current?.GetMissionBehaviour<SiegeMissionController>();
            if (siegeMissionController is null) return false;
            if (siegeMissionController?.IsSallyOut is true) return false;
            return true;
        }

        public static bool IsCurrentOrderable()
        {
            return true;
        }

        public static List<GameKey> GetCurrentGameKeys()
        {
            if (!IsCurrentValid()) return null;
            return (List<GameKey>)typeof(InputContext).GetCachedField("_registeredGameKeys").GetValue(Current.InputManager);
        }

        public static TaleWorlds.MountAndBlade.Mission Current => TaleWorlds.MountAndBlade.Mission.Current;

        public static Agent PlayerAgent => Current?.MainAgent;

        public static MissionMainAgentController PlayerAgentController => Current?.GetMissionBehaviour<MissionMainAgentController>();
    }
}