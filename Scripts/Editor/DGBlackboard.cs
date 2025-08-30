using System;
using System.Collections.Generic;
using System.Linq;
using DG_with_Localization.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGBlackboard : Blackboard
    {
        private DGView m_view;
        private DGAsset m_graph;

        private Dictionary<BlackboardRow, string> m_blackboardRows;
        private Dictionary<VisualElement, string> m_blackboardInputs;
        private Dictionary<BlackboardField, BlackboardRow> m_blackboardField;


        internal event Action onBlackboardChanged;

        public DGBlackboard(DGView associatedGraphView, DGAsset graph)
        {
            m_blackboardRows = new();
            m_blackboardInputs = new();
            m_blackboardField = new();

            m_graph = graph;
            m_view = associatedGraphView;
            this.graphView = associatedGraphView;
            this.SetPosition(new Rect(10, 30, 200, 300));

            this.title = "Variables";
            this.style.visibility = Visibility.Visible;

            this.addItemRequested = OnAddPropertyRequested;
            this.editTextRequested = OnEditPropertyNameRequeseted;

            DrawBlackboardProperties();
        }

        private void DrawBlackboardProperties()
        {
            if (m_graph.Properties == null) return;
            if (m_graph.Properties.Count == 0) return;

            foreach (DGSerializableProperty prop in m_graph.Properties)
            {
                CreateBlackboardVariable(prop);
            }
        }

        internal void RemoveBlackboardField(BlackboardField f)
        {
            if (m_blackboardField.TryGetValue(f, out BlackboardRow row))
            {
                if (m_blackboardRows.TryGetValue(row, out string id))
                {
                    DGSerializableProperty prop = m_graph.Properties.FirstOrDefault(x => x.id == id);
                    m_graph.Properties.Remove(prop);
                    m_blackboardRows.Remove(row);
                    m_blackboardField.Remove(f);
                    this.Remove(row);
                    onBlackboardChanged?.Invoke();
                }
            }
        }

        internal void HideOrUnhideBlackboard()
        {
            if (this.style.visibility == Visibility.Visible)
                this.style.visibility = Visibility.Hidden;
            else
                this.style.visibility = Visibility.Visible;
        }

        public (List<string>, List<string>, List<string>) GetBlackboardVariables()
        {
            List<string> props = new List<string>();
            List<string> idProps = new List<string>();
            List<string> propType = new List<string>();
            foreach (DGSerializableProperty prop in m_graph.Properties)
            {
                props.Add(prop.argName);
                idProps.Add(prop.id);
                propType.Add(prop.typeName);
            }
            return (props, idProps, propType);
        }

        public T GetBlackboardData<T>(string idVariable) => m_graph.Properties.FirstOrDefault(x => x.id == idVariable).GetValue<T>();
        
        #region Addting varriable to blackboard
        private void OnAddPropertyRequested(Blackboard blackboard)
        {
            UnityEditor.PopupWindow.Show(
                new Rect(
                    new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y - 75),
                    new Vector2(200, 100)
                ),
                new DGBlackboardPopupWindow(result => OnCloseBlackboardPopupWindow(result))
            );
        }

        private void OnCloseBlackboardPopupWindow(string result)
        {
            if (string.IsNullOrEmpty(result)) return;

            DGSerializableProperty prop = new DGSerializableProperty(System.Guid.NewGuid().ToString(), result);
            m_graph.Properties.Add(prop);

            CreateBlackboardVariable(prop);
            onBlackboardChanged?.Invoke();
            EditorUtility.SetDirty(m_graph);
        }

        private void CreateBlackboardVariable(DGSerializableProperty prop)
        {
            BlackboardField field = new BlackboardField() { text = prop.argName, typeText = prop.typeName };
            VisualElement data = new VisualElement();

            VisualElement inputField = CreateInputFiled(prop);
            m_blackboardInputs.Add(inputField, prop.id);
            data.Add(inputField);

            BlackboardRow row = new BlackboardRow(field, data);
            m_blackboardField.Add(field, row);
            m_blackboardRows.Add(row, prop.id);
            this.Add(row);
        }

        private VisualElement CreateInputFiled(DGSerializableProperty prop)
        {
            VisualElement element = null;
            int indexProperty = m_graph.Properties.IndexOf(prop);

            switch (prop.typeName)
            {
                case string v when v == typeof(string).Name:
                    DGVariantField<string> textField = new DGVariantField<string>("Value:", prop.GetValue<string>(), false);
                    RegisterValueChangedCallback(textField, prop);
                    element = textField;
                    break;
                case string v when v == typeof(int).Name:
                    DGVariantField<int> intField = new DGVariantField<int>("Value:", prop.GetValue<int>(), false);
                    RegisterValueChangedCallback(intField, prop);
                    element = intField;
                    break;
                case string v when v == typeof(float).Name:
                    DGVariantField<float> floatField = new DGVariantField<float>("Value:", prop.GetValue<float>(), false);
                    RegisterValueChangedCallback(floatField, prop);
                    element = floatField;
                    break;
                case string v when v == typeof(bool).Name:
                    DGVariantField<bool> boolField = new DGVariantField<bool>("Value:", prop.GetValue<bool>(), false);
                    RegisterValueChangedCallback(boolField, prop);
                    element = boolField;
                    break;
                case string v when v == typeof(object).Name:
                    DGVariantField<UnityEngine.Object> objectField = new DGVariantField<UnityEngine.Object>("Value:", prop.GetValue<UnityEngine.Object>(), false);
                    RegisterValueChangedCallback(objectField, prop);
                    element = objectField;
                    break;
            }
            return element;
        }

        private void RegisterValueChangedCallback<T>(DGVariantField<T> field, DGSerializableProperty prop) =>
            field.RegisterValueChangedCallback((evt) => { OnBlackboardValueChanged(evt.newValue, prop); });

        private void OnBlackboardValueChanged(object newValue, DGSerializableProperty prop)
        {
            if (newValue == null) return;

            if (newValue is string s) prop.SetValue(s);
            else if (newValue is int i) prop.SetValue(i);
            else if (newValue is float f) prop.SetValue(f);
            else if (newValue is bool b) prop.SetValue(b);
            else if (newValue is UnityEngine.Object o) prop.SetValue(o);

            onBlackboardChanged?.Invoke();
            EditorUtility.SetDirty(m_graph);
        }
        #endregion
    
        #region Editing varriable in blackboard 
        private void OnEditPropertyNameRequeseted(Blackboard blackboard, VisualElement element, string newValue)
        {
            BlackboardField field = element as BlackboardField;
            if (field == null) return;

            DGSerializableProperty prop = m_graph.Properties.FirstOrDefault(x => x.argName == field.text);
            if (prop.argName == newValue) return;
            if (m_graph.Properties.Exists(x => x.argName == newValue))
            {
                Debug.LogWarning($"Blackboard change variable name is exists\nNew Value: ${newValue} is exists");
                return;
            }

            prop.SetName(newValue);
            field.text = newValue;
            EditorUtility.SetDirty(m_graph);
            onBlackboardChanged?.Invoke();
        }
        #endregion
    }
}
