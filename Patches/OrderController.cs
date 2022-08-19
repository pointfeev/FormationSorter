using System;

using HarmonyLib;

using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    [HarmonyPatch(typeof(OrderController))]
    public static class PatchOrderController
    {
        [HarmonyPatch("IsFormationSelectable")]
        [HarmonyPatch(new Type[] { typeof(Formation), typeof(Agent) })]
        [HarmonyPrefix]
        public static bool IsFormationSelectable(Formation formation, Agent selectorAgent, ref bool __result)
        {
            try
            {
                if (Mission.IsCurrentValid() && Mission.IsCurrentOrderable())
                {
                    __result = selectorAgent == null || formation.PlayerOwner == selectorAgent;
                    return false;
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return true;
        }

        [HarmonyPatch("OnSelectedFormationsCollectionChanged")]
        [HarmonyPostfix]
        public static void OnSelectedFormationsCollectionChanged() => Selection.UpdateAllFormationOrderTroopItemVMs();
    }
}