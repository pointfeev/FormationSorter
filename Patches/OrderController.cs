using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
                if (!Mission.IsCurrentReady() || !Mission.IsCurrentOrderable()) return true;
                __result = selectorAgent == null || formation.PlayerOwner == selectorAgent;
                return false;
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
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction instruction = codes[i];
                yield return instruction;
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand.ToString() == "Int32 get_CountOfUnits()")
                {
                    char[] digits = codes[i + 1].operand.ToString().Where(char.IsDigit).ToArray();
                    digits = digits.Reverse().Take(digits.Length - 2).Reverse().ToArray();
                    if (int.TryParse(new string(digits), out int varIndex))
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

                        yield return new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(noUnitsInFormation);

                        yield return new CodeInstruction(OpCodes.Stloc_S, varIndex + 1).WithLabels(storeValue);
                    }
                }
            }
        }

        [HarmonyPatch("OnSelectedFormationsCollectionChanged")]
        [HarmonyPostfix]
        public static void Update()
        {
            Selection.UpdateAllFormationOrderTroopItemVMs();
        }
    }
}