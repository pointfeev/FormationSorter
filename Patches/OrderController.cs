using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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

        [HarmonyPatch("SetOrderWithFormationAndNumber")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetOrderWithFormationAndNumber(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            bool patched = false;
            IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
                if (!patched && enumerator.Current.opcode == OpCodes.Callvirt
                    && !(enumerator.Current.operand is null)
                    && enumerator.Current.operand.ToString().Contains("get_CountOfUnits")
                    && enumerator.MoveNext())
                {
                    Label end = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brfalse_S, end);
                    for (int i = 0; i < 6; i++)
                        if (enumerator.MoveNext())
                            yield return enumerator.Current;
                    if (enumerator.MoveNext())
                        yield return enumerator.Current.WithLabels(end);
                    patched = true;
                }
            }
            if (!patched)
                throw new Exception("Transpiler for OrderController.SetOrderWithFormationAndNumber failed!");
        }
    }
}