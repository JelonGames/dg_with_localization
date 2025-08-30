using System;
using UnityEngine;

namespace DG_with_Localization.Utility
{
    [Serializable]
    public class DGSerializableProperty
    {
        [SerializeField]
        private string m_id;
        [SerializeField]
        private string m_typeName;
        [SerializeField]
        private string m_argName;
        [SerializeField]
        private DGVariant m_value;

        // [SerializeField]
        // public string rawValue;
        // [SerializeField]
        // public UnityEngine.Object objectValue;

        public string id => m_id;
        public string typeName => m_typeName;
        public string argName => m_argName;

        public DGSerializableProperty(string argName, string typeName)
        {
            m_id = System.Guid.NewGuid().ToString();
            m_argName = argName;
            m_typeName = typeName;
        }

        public void SetName(string argName) => m_argName = argName;

        public void SetValue(string value) => m_value.Set(value);
        public void SetValue(int value) => m_value.Set(value);
        public void SetValue(float value) => m_value.Set(value);
        public void SetValue(bool value) => m_value.Set(value);
        public void SetValue(UnityEngine.Object value) => m_value.Set(value);

        public T GetValue<T>()
        {
            if (m_value == null)
            {
                return default;
            }
            return m_value.GetValue<T>();
        }

        public Type GetValueType()
        {
            Type type = Type.GetType($"System.{typeName}");
            if (type == null) return typeof(string);
            return Type.GetType(typeName);
        }
    }
}
