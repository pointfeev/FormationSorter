/*using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace FormationSorter
{
    [HarmonyPatch(typeof(CharacterObject))]
    public static class PatchCharacterObject
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetFormationClass")]
        public static void GetFormationClass(CharacterObject __instance, IBattleCombatant owner, ref FormationClass __result)
        {
            if (__instance.IsMounted)
            {
                if (__instance.IsArcher)
                {
                    __result = FormationClass.HorseArcher;
                }
                else
                {
                    __result = FormationClass.Cavalry;
                }
            }
            else
            {
                if (__instance.IsArcher)
                {
                    __result = FormationClass.Ranged;
                }
                else
                {
                    __result = FormationClass.Infantry;
                }
            }
        }
    }
}*/