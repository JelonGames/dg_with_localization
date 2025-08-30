using System;
using UnityEngine;

namespace DG_with_Localization
{
    [Serializable]
    public class DGVariant
    {
        private enum VariantType
        {
            None,
            String,
            Int,
            Float,
            Bool,
            Object
        }

        [SerializeField] private string stringValue;
        [SerializeField] private int intValue;
        [SerializeField] private float floatValue;
        [SerializeField] private bool boolValue;
        [SerializeField] private UnityEngine.Object objectValue;

        [SerializeField] private VariantType type;

        public DGVariant() => type = VariantType.None;
        public DGVariant(string v) => Set(v);
        public DGVariant(int v) => Set(v);
        public DGVariant(float v) => Set(v);
        public DGVariant(bool v) => Set(v);
        public DGVariant(UnityEngine.Object v) => Set(v);

        public void Set(string v) { stringValue = v; type = VariantType.String; }
        public void Set(int v) { intValue = v; type = VariantType.Int; }
        public void Set(float v) { floatValue = v; type = VariantType.Float; }
        public void Set(bool v) { boolValue = v; type = VariantType.Bool; }
        public void Set(UnityEngine.Object v) { objectValue = v; type = VariantType.Object; }

        public T GetValue<T>()
        {
            object val = null;
            switch (type)
            {
                case VariantType.String:
                    val = stringValue;
                    break;
                case VariantType.Int:
                    val = intValue;
                    break;
                case VariantType.Float:
                    val = floatValue;
                    break;
                case VariantType.Bool:
                    val = boolValue;
                    break;
                case VariantType.Object:
                    val = objectValue;
                    break;
                default:
                    return default;
            }

            if (val is T variable)
                return variable;

            try
            {
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return default;
            }
        }
    }
}
