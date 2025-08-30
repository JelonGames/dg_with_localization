using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DG_with_Localization.Utility
{
    [Serializable]
    public class DGActions
    {
        [SerializeField]
        private string m_variableID;
        [SerializeField]
        private string m_variableName;
        [SerializeField]
        private string m_variableType;
        [SerializeField]
        private string m_methodName;
        [SerializeField]
        private DGVariant[] m_value;

        public string variableID => m_variableID;
        public string variableName => m_variableName;
        // public string variableType => m_variableType;
        public string methodName => m_methodName;
        // public string value => m_value;

        public DGActions(string variableID, string variableName, string variableType)
        {
            m_variableID = variableID;
            m_variableName = variableName;
            m_variableType = variableType;
        }

        public void SetVariale(string variableID, string variableName, string variableType)
        {
            m_variableID = variableID;
            m_variableName = variableName;
            m_variableType = variableType;
            m_methodName = null;
            m_value = null;

            if (!string.IsNullOrEmpty(variableID) &&
                !string.IsNullOrEmpty(variableName) &&
                !string.IsNullOrEmpty(variableType))
                m_value = new DGVariant[1];
        }

        public void SetMethodName(string methodName, int numberParameters = 1)
        {
            m_methodName = methodName;
            m_value = new DGVariant[numberParameters];
        }

        public void SetValue(string value, int parameterIndex = 0) => m_value[parameterIndex] = new DGVariant(value);
        public void SetValue(int value, int parameterIndex = 0) => m_value[parameterIndex] = new DGVariant(value);
        public void SetValue(float value, int parameterIndex = 0) => m_value[parameterIndex] = new DGVariant(value);
        public void SetValue(bool value, int parameterIndex = 0) => m_value[parameterIndex] = new DGVariant(value);
        public void SetValue(UnityEngine.Object value, int parameterIndex = 0) => m_value[parameterIndex] = new DGVariant(value);

        public T GetValue<T>(int index = 0)
        {
            if (m_value == null) return default;
            if (m_value.Length <= index) return default;
            if (m_value[index] == null) return default;

            return m_value[index].GetValue<T>();
        }

        public Type GetVariableType() => Type.GetType($"System.{m_variableType}");

        public void Execute(DGSerializableProperty prop)
        {
            switch (GetVariableType())
            {
                case Type t when t == typeof(string):
                    prop.SetValue(GetValue<string>());
                    break;
                case Type t when t == typeof(int):
                    ExecuteInt(prop);
                    break;
                case Type t when t == typeof(float):
                    ExecuteFloat(prop);
                    break;
                case Type t when t == typeof(bool):
                    prop.SetValue(GetValue<bool>());
                    break;
                case Type t when t == typeof(object):
                    ExecuteMethod(prop.GetValue<UnityEngine.Object>());
                    break;
            }
        }

        private void ExecuteInt(DGSerializableProperty prop)
        {
            int oldValue = prop.GetValue<int>();
            int newValue = 0;
            if (String.Equals(methodName, IntFloatMethod.Append.ToString()))
                newValue = oldValue + GetValue<int>();
            else if (String.Equals(methodName, IntFloatMethod.Decrease.ToString()))
                newValue = oldValue - GetValue<int>();
            else if (String.Equals(methodName, IntFloatMethod.Set.ToString()))
                newValue = GetValue<int>();

            prop.SetValue(newValue);
        }

        private void ExecuteFloat(DGSerializableProperty prop)
        {
            float oldValue = prop.GetValue<float>();
            float newValue = 0;
            if (String.Equals(methodName, IntFloatMethod.Append.ToString()))
                newValue = oldValue + GetValue<float>();
            else if (String.Equals(methodName, IntFloatMethod.Decrease.ToString()))
                newValue = oldValue - GetValue<float>();
            else if (String.Equals(methodName, IntFloatMethod.Set.ToString()))
                newValue = GetValue<float>();

            prop.SetValue(newValue);
        }

        private void ExecuteMethod(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (string.IsNullOrEmpty(m_methodName))
            {
                Debug.Log($"Action {variableName} have null method name");
                return;
            }

            List<MethodInfo> methods = DGObjectUtyliti.GetMethodsFromObject(obj, typeof(void));
            int index = DGObjectUtyliti.GetNameAvaliableMethods(methods).ToList().IndexOf(m_methodName);
            if (index < 0)
            {
                Debug.Log($"Action {variableName} don't found method info where method have name {m_methodName}");
                return;
            }
            MethodInfo methodInfo = methods[index];

            List<ParameterInfo> parameters = DGObjectUtyliti.GetParametersFormMethod(obj, m_methodName, typeof(void));
            if (parameters.Count == 0)
            {
                methodInfo.Invoke(obj, new object[0]);
                return;
            }

            object[] parametersArray = new object[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                Type type = parameters[i].ParameterType;
                switch (type)
                {
                    case Type t when t == typeof(string):
                        parametersArray[i] = GetValue<string>(i);
                        break;
                    case Type t when t == typeof(int):
                        parametersArray[i] = GetValue<int>(i);
                        break;
                    case Type t when t == typeof(float):
                        parametersArray[i] = GetValue<float>(i);
                        break;
                    case Type t when t == typeof(bool):
                        parametersArray[i] = GetValue<bool>(i);
                        break;
                    case Type t when t == typeof(object) ||
                        t == typeof(UnityEngine.Object):
                        parametersArray[i] = GetValue<UnityEngine.Object>(i);
                        break;
                }
            }

            methodInfo.Invoke(obj, parametersArray);
        }
    }

    public enum IntFloatMethod
    {
        Append,
        Decrease,
        Set
    }
}
