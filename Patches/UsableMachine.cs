using System.Reflection;
using FormationSorter.Utilities;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FormationSorter.Patches
{
    [HarmonyPatch(typeof(UsableMachine))]
    public static class PatchUsableMachine
    {
        [HarmonyPatch("TaleWorlds.MountAndBlade.IDetachment.GetDetachmentWeight")]
        [HarmonyPrefix]
        public static bool GetDetachmentWeight(UsableMachine __instance, BattleSideEnum side, ref float __result)
        {
            // this issue should be fixed as of v1.3.0, however I'm still patching it just in case
            if (!ReflectionUtils.IsMethodInCallStack(MethodBase.GetCurrentMethod()))
            {
                try
                {
                    if (Mission.IsCurrentValid())
                        __result = (float)typeof(UsableMachine)
                                         .GetCachedMethod("TaleWorlds.MountAndBlade.IDetachment.GetDetachmentWeight")
                                         .Invoke(__instance, new object[] { side });
                }
                catch
                {
                    __result = 0.01f;
                }
                return false;
            }
            return true;
        }
    }
}