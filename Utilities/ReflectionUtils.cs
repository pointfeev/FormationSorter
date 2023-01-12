using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace FormationSorter.Utilities
{
    internal static class ReflectionUtils
    {
        private static readonly Dictionary<MemberInfo, Dictionary<string, MemberInfo>> ReflectionCache
            = new Dictionary<MemberInfo, Dictionary<string, MemberInfo>>();

        internal static ConstructorInfo GetCachedConstructor(this Type fromType, Type[] types, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            string identifier = fromType.FullName + $"({types})";
            if (CheckReflectionCache(fromType, identifier, out MemberInfo memberInfo))
                return memberInfo as ConstructorInfo;
            ConstructorInfo constructorInfo = fromType.GetConstructor(bindingFlags, null, types, null);
            AddToReflectionCache(fromType, identifier, constructorInfo);
            return constructorInfo;
        }

        internal static FieldInfo GetCachedField(this Type fromType, string fieldName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, fieldName, out MemberInfo memberInfo))
                return memberInfo as FieldInfo;
            FieldInfo fieldInfo = fromType.GetField(fieldName, bindingFlags);
            AddToReflectionCache(fromType, fieldName, fieldInfo);
            return fieldInfo;
        }

        internal static MethodInfo GetCachedMethod(this Type fromType, string methodName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, methodName, out MemberInfo memberInfo))
                return memberInfo as MethodInfo;
            MethodInfo methodInfo = fromType.GetMethod(methodName, bindingFlags);
            AddToReflectionCache(fromType, methodName, methodInfo);
            return methodInfo;
        }

        private static bool CheckReflectionCache(MemberInfo memberInfo, string identifier, out MemberInfo cachedMemberInfo)
        {
            if (ReflectionCache.TryGetValue(memberInfo, out Dictionary<string, MemberInfo> methodInfos))
                if (methodInfos.TryGetValue(identifier, out cachedMemberInfo))
                    return true;
            cachedMemberInfo = null;
            return false;
        }

        private static void AddToReflectionCache(MemberInfo memberInfo, string identifier, MemberInfo cachedMemberInfo)
        {
            if (!ReflectionCache.TryGetValue(memberInfo, out Dictionary<string, MemberInfo> methodInfos))
            {
                methodInfos = new Dictionary<string, MemberInfo>();
                ReflectionCache.Add(memberInfo, methodInfos);
            }
            if (!methodInfos.TryGetValue(identifier, out MemberInfo _))
                methodInfos.Add(identifier, cachedMemberInfo);
        }

        internal static bool IsMethodInCallStack(MethodBase method)
        {
            StackFrame[] stackFrames = new StackTrace().GetFrames();
            if (stackFrames == null)
                return false;
            // 0 = current method
            // 1 = calling method
            // 2+ = methods we want to check
            for (int i = 2; i <= stackFrames.Length; i++)
            {
                StackFrame stackFrame = stackFrames.ElementAtOrValue(i, null);
                MethodBase stackMethod = stackFrame?.GetMethod();
                if (stackMethod is null)
                    continue;
                if (stackMethod == method)
                    return true;
            }
            return false;
        }
    }
}