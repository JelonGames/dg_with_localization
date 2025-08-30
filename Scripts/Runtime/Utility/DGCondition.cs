using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DG_with_Localization.Utility
{
    [Serializable]
    public class DGCondition
    {
        [SerializeField]
        private string m_variableID;
        [SerializeField]
        private string m_variableName;
        [SerializeField]
        private string m_variableType;
        [SerializeField]
        private string m_actionName;
        [SerializeField]
        private DGVariant[] m_value;

        public string variableID => m_variableID;
        public string variableName => m_variableName;
        public string actionName => m_actionName;


        public DGCondition(string variableID, string varialbleName, string variableType)
        {
            m_variableID = variableID;
            m_variableName = varialbleName;
            m_variableType = variableType;
            m_actionName = null;
            m_value = null;
        }

        public void SetVariable(string variableID, string varialbleName, string variableType)
        {
            m_variableID = variableID;
            m_variableName = varialbleName;
            m_variableType = variableType;
            m_actionName = null;
            m_value = null;
        }

        public void SetActionName(string actionName, int variantNumber = 0)
        {
            m_actionName = actionName;
            m_value = new DGVariant[variantNumber];
        }

        public void SetValue(string value, int index = 0) => m_value[index].Set(value);
        public void SetValue(int value, int index = 0) => m_value[index].Set(value);
        public void SetValue(float value, int index = 0) => m_value[index].Set(value);
        public void SetValue(bool value, int index = 0) => m_value[index].Set(value);
        public void SetValue(UnityEngine.Object value, int index = 0) => m_value[index].Set(value);

        public T GetValue<T>(int index = 0)
        {
            if (m_value == null) return default;
            if (m_value.Length < index) return default;
            if (m_value[index] == null) return default;

            return m_value[index].GetValue<T>();
        }

        public Type GetVariableType()
        {
            Type type = Type.GetType($"System.{m_variableType}");
            if (type == null) return null;
            return type;
        }

        public bool CheckCondition(DGSerializableProperty prop)
        {
            if (string.IsNullOrEmpty(m_variableID)) return true;

            if (prop == null) return false;

            return prop.typeName switch
            {
                string t when t == typeof(string).Name => CheckStringCondition(prop),
                string t when t == typeof(int).Name => CheckIntCondition(prop),
                string t when t == typeof(float).Name => CheckFloatCondition(prop),
                string t when t == typeof(bool).Name => CheckBooleanCondition(prop),
                string t when t == typeof(object).Name => ExecuteMethod(prop),
                _ => false
            };
        }

        #region Comparison methods
        private bool CheckStringCondition(DGSerializableProperty prop)
        {
            if (string.IsNullOrEmpty(m_actionName)) return false;

            return m_actionName switch
            {
                string v when v == ComparisonOperator.Equals.ToString() => string.Equals(this.GetValue<string>(), prop.GetValue<string>()),
                string v when v == ComparisonOperator.NotEquals.ToString() => !string.Equals(this.GetValue<string>(), prop.GetValue<string>()),
                _ => false
            };
        }

        private bool CheckIntCondition(DGSerializableProperty prop)
        {
            if (string.IsNullOrEmpty(m_actionName)) return false;

            return m_actionName switch
            {
                string v when v == ComparisonOperator.Equals.ToString() => prop.GetValue<int>() == this.GetValue<int>(),
                string v when v == ComparisonOperator.NotEquals.ToString() => prop.GetValue<int>() != this.GetValue<int>(),
                string v when v == ComparisonOperator.Less.ToString() => prop.GetValue<int>() < this.GetValue<int>(),
                string v when v == ComparisonOperator.LessOrEqual.ToString() => prop.GetValue<int>() <= this.GetValue<int>(),
                string v when v == ComparisonOperator.Greater.ToString() => prop.GetValue<int>() > this.GetValue<int>(),
                string v when v == ComparisonOperator.GreaterOrEqual.ToString() => prop.GetValue<int>() >= this.GetValue<int>(),
                _ => false
            };
        }

        private bool CheckFloatCondition(DGSerializableProperty prop)
        {
            if (string.IsNullOrEmpty(m_actionName)) return false;

            return m_actionName switch
            {
                string v when v == ComparisonOperator.Equals.ToString() => prop.GetValue<float>() == this.GetValue<float>(),
                string v when v == ComparisonOperator.NotEquals.ToString() => prop.GetValue<float>() != this.GetValue<float>(),
                string v when v == ComparisonOperator.Less.ToString() => prop.GetValue<float>() < this.GetValue<float>(),
                string v when v == ComparisonOperator.LessOrEqual.ToString() => prop.GetValue<float>() <= this.GetValue<float>(),
                string v when v == ComparisonOperator.Greater.ToString() => prop.GetValue<float>() > this.GetValue<float>(),
                string v when v == ComparisonOperator.GreaterOrEqual.ToString() => prop.GetValue<float>() >= this.GetValue<float>(),
                _ => false
            };
        }

        private bool CheckBooleanCondition(DGSerializableProperty prop)
        {
            if (string.IsNullOrEmpty(m_actionName)) return false;

            return m_actionName switch
            {
                string v when v == ComparisonOperator.Equals.ToString() => prop.GetValue<bool>() == this.GetValue<bool>(),
                string v when v == ComparisonOperator.NotEquals.ToString() => prop.GetValue<bool>() != this.GetValue<bool>(),
                _ => false
            };
        }
        #endregion

        private bool ExecuteMethod(DGSerializableProperty prop)
        {
            if (prop == null) return false;
            if (prop.GetValue<UnityEngine.Object>() == null) return false;
            if (string.IsNullOrEmpty(m_actionName)) return false;

            UnityEngine.Object obj = prop.GetValue<UnityEngine.Object>();
            List<MethodInfo> methodInfos = DGObjectUtyliti.GetMethodsFromObject(obj, typeof(bool));
            int index = DGObjectUtyliti.GetNameAvaliableMethods(methodInfos).ToList().IndexOf(m_actionName);
            MethodInfo methodInfo = methodInfos[index];
            if (methodInfo == null) return false;

            List<ParameterInfo> parameters = methodInfo.GetParameters().ToList();

            if (m_value == null) return false;
            if (parameters.Count == 0) return (bool)methodInfo.Invoke(obj, null);
            object[] data = new object[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                switch (parameters[i].ParameterType)
                {
                    case Type t when t == typeof(string):
                        data[i] = GetValue<string>(i);
                        break;
                    case Type t when t == typeof(int):
                        data[i] = GetValue<int>(i);
                        break;
                    case Type t when t == typeof(float):
                        data[i] = GetValue<float>(i);
                        break;
                    case Type t when t == typeof(bool):
                        data[i] = GetValue<bool>(i);
                        break;
                    case Type t when t == typeof(object) ||
                        t == typeof(UnityEngine.Object):
                        data[i] = GetValue<UnityEngine.Object>(i);
                        break;
                }
            }

            return (bool)methodInfo.Invoke(obj, data);
        }
    }

    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual
    }
}
