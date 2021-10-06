using HarmonyLib;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    [HarmonyPatch(typeof(MissionOrderTroopControllerVM))]
    public static class PatchMissionOrderTroopControllerVM
    {
        [HarmonyPatch("UpdateTroops")]
        [HarmonyPostfix]
        public static void Update()
        {
            Selection.UpdateAllFormationOrderTroopItemVMs();
        }
    }
}