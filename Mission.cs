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

        public static bool IsPlayerInteracting()
        {
            Agent playerAgent = PlayerAgent;
            if (playerAgent is null) return false;
            MissionMainAgentController playerAgentController = PlayerAgentController;
            if (playerAgentController is null) return false;
            MissionMainAgentInteractionComponent interactionComponent = playerAgentController.InteractionComponent;
            if (interactionComponent is null) return false;
            return !(interactionComponent.CurrentFocusedObject is null) || !(interactionComponent.CurrentFocusedMachine is null);
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
            if (fieldRegisteredGameKeys is null)
            {
                fieldRegisteredGameKeys = typeof(InputContext).GetField("_registeredGameKeys", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            if (!IsCurrentReady()) return null;
            return (List<GameKey>)fieldRegisteredGameKeys.GetValue(Current.InputManager);
        }

        private static FieldInfo fieldRegisteredGameKeys;

        public static TaleWorlds.MountAndBlade.Mission Current => TaleWorlds.MountAndBlade.Mission.Current;

        public static Agent PlayerAgent => Current?.MainAgent;

        public static MissionMainAgentController PlayerAgentController => Current?.GetMissionBehaviour<MissionMainAgentController>();
    }
}