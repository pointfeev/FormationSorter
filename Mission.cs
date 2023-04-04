using FormationSorter.Utilities;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter;

public static class Mission
{
    public static MissionOrderVM MissionOrderVM;
    public static OrderItemVM OrderItemVM;
    public static OrderSetVM OrderSetVM;
    public static InputKeyItemVM InputKeyItemVM;

    public static TaleWorlds.MountAndBlade.Mission Current => TaleWorlds.MountAndBlade.Mission.Current;

    public static Agent PlayerAgent => Current?.MainAgent;

    private static MissionMainAgentController PlayerAgentController => Current?.GetMissionBehavior<MissionMainAgentController>();

    public static bool CanPlayerInteract()
    {
        Agent playerAgent = PlayerAgent;
        if (playerAgent is null)
            return false;
        MissionMainAgentController playerAgentController = PlayerAgentController;
        MissionMainAgentInteractionComponent interactionComponent = playerAgentController?.InteractionComponent;
        if (interactionComponent is null)
            return false;
        IFocusable currentInteractableObject = (IFocusable)typeof(MissionMainAgentInteractionComponent).GetCachedField("_currentInteractableObject")
           .GetValue(interactionComponent);
        if (currentInteractableObject is null)
            return false;
        Agent agent = currentInteractableObject as Agent;
        return agent?.IsMount != false;
    }

    public static bool IsCurrentValid()
    {
        TaleWorlds.MountAndBlade.Mission current = Current;
        if (current is null || current.Mode is not MissionMode.Battle && current.Mode is not MissionMode.Stealth || MissionOrderVM is null)
            return false;
        try
        {
            if (MissionOrderVM.OrderController is null || MissionOrderVM.TroopController is null)
                return false;
        }
        catch // to catch errors that are entirely out of my control
        {
            return false;
        }
        return true;
    }
}