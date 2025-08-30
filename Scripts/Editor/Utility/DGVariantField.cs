using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGVariantField<T> : BaseField<T>
    {
        private VisualElement m_inputElement;

        public DGVariantField(string label, bool setNodeStyle) : this(label, default(T), setNodeStyle)
        {
        }

        public DGVariantField(string label, T value, bool setNodeStyle) : base(string.Empty, CreateFieldForType(label, value, setNodeStyle))
        {
            m_inputElement = this.Q<VisualElement>(className: "unity-base-field__input");
            this.Add(m_inputElement);
        }

        private static VisualElement CreateFieldForType(string label, T value, bool setNodeStyle)
        {
            VisualElement field = null;
            Type type = typeof(T);
            if (type == typeof(string))
            {
                TextField textField = new TextField()
                {
                    label = label,
                    maxLength = 256,
                    multiline = false,
                    isPasswordField = false
                };
                textField.value = value as string;
                field = textField;
                if (setNodeStyle)
                    field.AddToClassList("dg-node__field-base");
            }
            else if (type == typeof(int))
            {
                IntegerField integerField = new IntegerField(label);
                integerField.value = Convert.ToInt32(value);
                field = integerField;
                if (setNodeStyle)
                    field.AddToClassList("dg-node__field-base");
            }
            else if (type == typeof(float))
            {
                FloatField floatField = new FloatField(label);
                floatField.value = Convert.ToSingle(value);
                field = floatField;
                if (setNodeStyle)
                    field.AddToClassList("dg-node__field-base");
            }
            else if (type == typeof(bool))
            {
                Toggle toggle = new Toggle(label);
                toggle.value = Convert.ToBoolean(value);
                field = toggle;
                if (setNodeStyle)
                    field.AddToClassList("dg-node__toggle");
            }
            else if (type == typeof(UnityEngine.Object))
            {
                ObjectField objectField = new ObjectField()
                {
                    label = label,
                    objectType = typeof(UnityEngine.Object),
                    allowSceneObjects = false,
                    value = value as UnityEngine.Object
                };
                field = objectField;
                if (setNodeStyle)
                    field.AddToClassList("dg-node__field-base");
            }

            field.style.marginLeft = 0;
            return field;
        }
    }
}