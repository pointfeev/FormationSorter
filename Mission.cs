using System.Collections.Generic;
using System.Reflection;
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
            IFocusable currentInteractableObject = (IFocusable)ReflectionUtils
                .GetField(typeof(MissionMainAgentInteractionComponent), "_currentInteractableObject", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(interactionComponent);
            if (currentInteractableObject is null) return false;
            Agent agent = currentInteractableObject as Agent;
            return !(currentInteractableObject is null) && (agent is null || agent.IsMount);
        }

        public static bool IsCurrentReady()
        {
            if (Current is null) return false;
            if (Current.MissionEnded()) return false;
            if (MissionOrderVM is null) return false;
            try
            {
                if (MissionOrderVM.OrderController is null) return false;
                if (MissionOrderVM.TroopController is null) return false;
            }
            catch { }
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
            if (!IsCurrentReady()) return null;
            return (List<GameKey>)ReflectionUtils.GetField(typeof(InputContext), "_registeredGameKeys", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(Current.InputManager);
        }

        public static TaleWorlds.MountAndBlade.Mission Current => TaleWorlds.MountAndBlade.Mission.Current;

        public static Agent PlayerAgent => Current?.MainAgent;

        public static MissionMainAgentController PlayerAgentController => Current?.GetMissionBehaviour<MissionMainAgentController>();
    }
}