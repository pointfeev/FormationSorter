using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace FormationSorter
{
    internal static class ReflectionUtils
    {
        public static Type GetCachedNestedType(this Type fromType, string nestedTypeName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, nestedTypeName, out MemberInfo memberInfo)) return memberInfo as Type;
            Type nestedType = fromType.GetNestedType(nestedTypeName, bindingFlags);
            AddToReflectionCache(fromType, nestedTypeName, nestedType);
            return nestedType;
        }

        public static ConstructorInfo GetCachedConstructor(this Type fromType, Type[] types = null, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            string identifier = fromType.FullName + (types is null ? "()" : $"({types})");
            if (CheckReflectionCache(fromType, identifier, out MemberInfo memberInfo)) return memberInfo as ConstructorInfo;
            ConstructorInfo constructorInfo = fromType.GetConstructor(bindingFlags, null, types, null);
            AddToReflectionCache(fromType, identifier, constructorInfo);
            return constructorInfo;
        }

        public static FieldInfo GetCachedField(this Type fromType, string fieldName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, fieldName, out MemberInfo memberInfo)) return memberInfo as FieldInfo;
            FieldInfo fieldInfo = fromType.GetField(fieldName, bindingFlags);
            AddToReflectionCache(fromType, fieldName, fieldInfo);
            return fieldInfo;
        }

        public static PropertyInfo GetCachedProperty(this Type fromType, string propertyName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, propertyName, out MemberInfo memberInfo)) return memberInfo as PropertyInfo;
            PropertyInfo propertyInfo = fromType.GetProperty(propertyName, bindingFlags);
            AddToReflectionCache(fromType, propertyName, propertyInfo);
            return propertyInfo;
        }

        public static MethodInfo GetCachedGetMethod(this PropertyInfo fromPropertyInfo)
        {
            if (CheckReflectionCache(fromPropertyInfo, "GetMethod", out MemberInfo memberInfo)) return memberInfo as MethodInfo;
            MethodInfo methodInfo = fromPropertyInfo.GetGetMethod() ?? fromPropertyInfo.GetGetMethod(true);
            AddToReflectionCache(fromPropertyInfo, "GetMethod", methodInfo);
            return methodInfo;
        }

        public static MethodInfo GetCachedSetMethod(this PropertyInfo fromPropertyInfo)
        {
            if (CheckReflectionCache(fromPropertyInfo, "SetMethod", out MemberInfo memberInfo)) return memberInfo as MethodInfo;
            MethodInfo methodInfo = fromPropertyInfo.GetSetMethod() ?? fromPropertyInfo.GetSetMethod(true);
            AddToReflectionCache(fromPropertyInfo, "SetMethod", methodInfo);
            return methodInfo;
        }

        public static MethodInfo GetCachedMethod(this Type fromType, string methodName, BindingFlags bindingFlags = (BindingFlags)(-1))
        {
            if (CheckReflectionCache(fromType, methodName, out MemberInfo memberInfo)) return memberInfo as MethodInfo;
            MethodInfo methodInfo = fromType.GetMethod(methodName, bindingFlags);
            AddToReflectionCache(fromType, methodName, methodInfo);
            return methodInfo;
        }

        public static MethodInfo MakeCachedGenericMethod(this MethodInfo fromMethodInfo, Type ofType)
        {
            if (CheckReflectionCache(fromMethodInfo, ofType.FullName, out MemberInfo memberInfo)) return memberInfo as MethodInfo;
            MethodInfo genericMethodInfo = fromMethodInfo.MakeGenericMethod(ofType);
            AddToReflectionCache(fromMethodInfo, ofType.FullName, genericMethodInfo);
            return genericMethodInfo;
        }

        private static bool CheckReflectionCache(MemberInfo memberInfo, string identifier, out MemberInfo cachedMemberInfo)
        {
            if (reflectionCache.TryGetValue(memberInfo, out Dictionary<string, MemberInfo> methodInfos))
                if (methodInfos.TryGetValue(identifier, out cachedMemberInfo)) return true;
            cachedMemberInfo = null;
            return false;
        }

        private static void AddToReflectionCache(MemberInfo memberInfo, string identifier, MemberInfo cachedMemberInfo)
        {
            if (!reflectionCache.TryGetValue(memberInfo, out Dictionary<string, MemberInfo> methodInfos))
            {
                methodInfos = new Dictionary<string, MemberInfo>();
                reflectionCache.Add(memberInfo, methodInfos);
            }
            if (!methodInfos.TryGetValue(identifier, out MemberInfo _))
                methodInfos.Add(identifier, cachedMemberInfo);
        }

        private static readonly Dictionary<MemberInfo, Dictionary<string, MemberInfo>> reflectionCache = new Dictionary<MemberInfo, Dictionary<string, MemberInfo>>();

        internal static bool IsMethodInCallStack(MethodBase method)
        {
            StackFrame[] stackFrames = new StackTrace().GetFrames();
            // 0 = current method
            // 1 = calling method
            // 2+ = methods we want to check
            for (int i = 2; i <= stackFrames.Length; i++)
            {
                StackFrame stackFrame = stackFrames.ElementAtOrValue(i, null);
                if (stackFrame is null)
                {
                    continue;
                }

                MethodBase stackMethod = stackFrame.GetMethod();
                if (stackMethod is null)
                {
                    continue;
                }
                if (stackMethod == method)
                {
                    return true;
                }
            }
            return false;
        }
    }
}