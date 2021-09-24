using HarmonyLib;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    [HarmonyPatch(typeof(OrderSetVM))]
    public static class PatchOrderSetVM
    {
        [HarmonyPatch("RefreshValues")]
        [HarmonyPostfix]
        public static void RefreshValues(OrderSetVM __instance, int ____index)
        {
            if (____index == Formations.OrderSetIndex)
            {
                __instance.TitleText = "Sort Units Between Formations";
                __instance.TitleOrder.OrderIconID = "ToggleAI";
                __instance.TitleOrder.TooltipText = "Sort Units Between Formations";
                __instance.TitleOrderKey = InputKeyItemVM.CreateFromGameKey(Formations.Hotkey, false);
                __instance.TitleOrder.ShortcutKey = __instance.TitleOrderKey;
            }
        }
    }
}