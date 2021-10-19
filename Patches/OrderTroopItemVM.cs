using HarmonyLib;
using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace FormationSorter
{
    [HarmonyPatch(typeof(OrderTroopItemVM))]
    public static class PatchOrderTroopItemVM
    {
        [HarmonyPatch("SetFormationClassFromFormation")]
        [HarmonyPrefix]
        public static bool SetFormationClassFromFormation(OrderTroopItemVM __instance, Formation formation)
        {
            try
            {
                if (Mission.IsCurrentValid() && Mission.IsCurrentOrderable() && formation.CountOfUnits <= 0)
                {
                    switch (formation.InitialClass)
                    {
                        case FormationClass.Ranged:
                            __instance.FormationClass = 2;
                            break;

                        case FormationClass.Cavalry:
                        case FormationClass.LightCavalry:
                        case FormationClass.HeavyCavalry:
                            __instance.FormationClass = 3;
                            break;

                        case FormationClass.HorseArcher:
                            __instance.FormationClass = 4;
                            break;

                        default:
                            __instance.FormationClass = 1;
                            break;
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return true;
        }
    }
}