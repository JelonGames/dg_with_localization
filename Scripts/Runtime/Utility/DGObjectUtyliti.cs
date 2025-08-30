
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DG_with_Localization.Utility
{
    public static class DGObjectUtyliti
    {
        public static string[] GetNameAvaliableMethods(object obj, Type methodRetunType, int maxParrameters = 999)
        {
            List<MethodInfo> methodInfos = GetMethodsFromObject(obj, methodRetunType, maxParrameters);
            return PrepareDisplayNameMethods(methodInfos);
        }

        public static string[] GetNameAvaliableMethods(List<MethodInfo> methodInfos) => PrepareDisplayNameMethods(methodInfos);

        public static List<MethodInfo> GetMethodsFromObject(object obj, Type methodReturnType, int maxParameters = 999)
        {
            if (obj == null) return null;
            List<MethodInfo> methodInfos = obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.ReturnType == methodReturnType)
                .Where(m => m.GetParameters().Length <= maxParameters)
                .Where(m => m.GetParameters().All(p =>
                    p.ParameterType == typeof(string) ||
                    p.ParameterType == typeof(int) ||
                    p.ParameterType == typeof(float) ||
                    p.ParameterType == typeof(bool) ||
                    p.ParameterType == typeof(UnityEngine.Object)))
                .ToList();

            return methodInfos;
        }

        public static List<ParameterInfo> GetParametersFormMethod(object obj, string methodName, Type methodReturnType)
        {
            List<MethodInfo> methods = GetMethodsFromObject(obj, methodReturnType, 999);
            int index = GetNameAvaliableMethods(obj, methodReturnType, 999).ToList().IndexOf(methodName);

            if (methods == null || methods.Count == 0) return null;
            if (index == -1) return null;
            return methods[index].GetParameters().ToList();
        }

        private static string[] PrepareDisplayNameMethods(List<MethodInfo> methodInfos)
        {
            List<string> value = new List<string>();
            foreach (var method in methodInfos)
            {
                List<ParameterInfo> pInfo = method.GetParameters().ToList();
                if (pInfo.Count == 0)
                {
                    value.Add($"{method.Name}()");
                    continue;
                }

                string parameterString = string.Empty;
                foreach (ParameterInfo p in pInfo)
                {
                    if (!string.IsNullOrEmpty(parameterString)) parameterString += ", ";
                    if (p.ParameterType == typeof(string)) parameterString += $"string {p.Name}";
                    else if (p.ParameterType == typeof(int)) parameterString += $"int {p.Name}";
                    else if (p.ParameterType == typeof(float)) parameterString += $"float {p.Name}";
                    else if (p.ParameterType == typeof(bool)) parameterString += $"bool {p.Name}";
                    else if (p.ParameterType == typeof(UnityEngine.Object)) parameterString += $"{p.ParameterType} {p.Name}";
                }

                value.Add($"{method.Name}({parameterString})");

            }

            return value.ToArray();
        }
    }
}
