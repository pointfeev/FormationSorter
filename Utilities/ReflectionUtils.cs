using System.Diagnostics;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public static class ReflectionUtils
    {
        public static bool IsMethodInCallStack(MethodBase method)
        {
            StackFrame[] stackFrames = new StackTrace().GetFrames();
            // 0 = current method
            // 1 = calling method
            // 2+ = methods we want to check
            for (int i = 2; i < stackFrames.Length; i++)
            {
                StackFrame stackFrame = stackFrames.ElementAtOrValue(i, null);
                if (stackFrame is null)
                {
                    continue;
                }
                MethodBase stackMethod = stackFrame.GetMethod();
                if (stackMethod == method)
                {
                    return true;
                }
            }
            return false;
        }
    }
}