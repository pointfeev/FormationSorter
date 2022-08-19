using System;
using System.Collections.Generic;
using System.Linq;
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

        [HarmonyPatch("SetOrderWithFormationAndNumber")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SetOrderWithFormationAndNumber(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction instruction = codes[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand.ToString() == "Int32 get_CountOfUnits()")
                {
                    string operand = codes[i + 1].operand.ToString();
                    int l = operand.LastIndexOf('('), r = operand.LastIndexOf(')');
                    if (l >= 0 && r >= 0 && int.TryParse(operand.Substring(l + 1, r - l - 1), out int varIndex))
                    {
                        Label noUnitsInFormation = generator.DefineLabel();
                        Label storeValue = generator.DefineLabel();
                        i += 7;

                        yield return new CodeInstruction(OpCodes.Stloc_S, varIndex);

                        yield return new CodeInstruction(OpCodes.Ldloc_S, varIndex);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, noUnitsInFormation); // to prevent division by zero

                        yield return new CodeInstruction(OpCodes.Ldloc_2);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, varIndex);
                        yield return new CodeInstruction(OpCodes.Mul);
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        yield return new CodeInstruction(OpCodes.Div);
                        yield return new CodeInstruction(OpCodes.Br_S, storeValue);

                        yield return new CodeInstruction(OpCodes.Pop).WithLabels(noUnitsInFormation);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);

                        yield return new CodeInstruction(OpCodes.Stloc_S, varIndex + 1).WithLabels(storeValue);
                    }
                }
            }
        }

        [HarmonyPatch("OnSelectedFormationsCollectionChanged")]
        [HarmonyPostfix]
        public static void OnSelectedFormationsCollectionChanged() => Selection.UpdateAllFormationOrderTroopItemVMs();
    }
}