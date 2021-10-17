using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    public static class ReflectionUtils
    {
        public static ConstructorInfo GetConstructor(Type memberType, Type[] types, BindingFlags bindingFlags)
        {
            if (CheckReflectionCache(memberType, types.ToString(), out MemberInfo memberInfo)) return memberInfo as ConstructorInfo;
            ConstructorInfo constructorInfo = memberType.GetConstructor(bindingFlags, null, types, null);
            AddToReflectionCache(memberType, types.ToString(), constructorInfo);
            return constructorInfo;
        }

        public static FieldInfo GetField(Type memberType, string memberName, BindingFlags bindingFlags)
        {
            if (CheckReflectionCache(memberType, memberName, out MemberInfo memberInfo)) return memberInfo as FieldInfo;
            FieldInfo fieldInfo = memberType.GetField(memberName, bindingFlags);
            AddToReflectionCache(memberType, memberName, fieldInfo);
            return fieldInfo;
        }

        public static MethodInfo GetMethod(Type memberType, string memberName, BindingFlags bindingFlags)
        {
            if (CheckReflectionCache(memberType, memberName, out MemberInfo memberInfo)) return memberInfo as MethodInfo;
            MethodInfo methodInfo = memberType.GetMethod(memberName, bindingFlags);
            AddToReflectionCache(memberType, memberName, methodInfo);
            return methodInfo;
        }

        private static bool CheckReflectionCache(Type memberType, string identifier, out MemberInfo memberInfo)
        {
            if (reflectionCache.TryGetValue(memberType, out Dictionary<string, MemberInfo> methodInfos))
                if (methodInfos.TryGetValue(identifier, out memberInfo)) return true;
            memberInfo = null;
            return false;
        }

        private static void AddToReflectionCache(Type memberType, string identifier, MemberInfo memberInfo)
        {
            if (!reflectionCache.TryGetValue(memberType, out Dictionary<string, MemberInfo> methodInfos))
            {
                methodInfos = new Dictionary<string, MemberInfo>();
                reflectionCache.Add(memberType, methodInfos);
            }
            if (!methodInfos.TryGetValue(identifier, out MemberInfo _))
                methodInfos.Add(identifier, memberInfo);
        }

        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> reflectionCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();

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