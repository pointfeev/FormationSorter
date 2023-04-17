using System;
using System.Linq;
using System.Reflection;
using FormationSorter.Utilities;
using HarmonyLib;
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

    internal static bool IsOrderShoutingAllowed()
    {
        MethodInfo shoutingAllowed = typeof(TaleWorlds.MountAndBlade.Mission).GetCachedMethod("IsOrderShoutingAllowed")
                                  ?? typeof(TaleWorlds.MountAndBlade.Mission).GetCachedMethod("IsOrderGesturesEnabled");
        return Current is not null && shoutingAllowed?.Invoke(Current, new object[] { }) is true;
    }

    internal static bool IsCaptainAssignmentAllowed()
    {
        Type assignmentLogicType = AccessTools.TypeByName("GeneralsAndCaptainsAssignmentLogic") ?? AccessTools.TypeByName("AutoCaptainAssignmentLogic");
        return assignmentLogicType is not null && Current?.MissionBehaviors?.Any(b =>
        {
            try
            {
                return Convert.ChangeType(b, assignmentLogicType) is not null;
            }
            catch
            {
                return false;
            }
        }) is true;
    }

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

    public static bool IsCurrentValid(bool uiFeedback = false)
    {
        TaleWorlds.MountAndBlade.Mission current = Current;
        if (current is null || current.Mode is MissionMode.Deployment || MissionOrderVM is null)
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
        return (bool)typeof(MissionOrderVM).GetCachedMethod("CheckCanBeOpened").Invoke(MissionOrderVM, new object[] { uiFeedback });
    }
}