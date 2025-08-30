using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG_with_Localization.Elements;
using DG_with_Localization.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGActionNodeEditor : DGNodeEditor
    {
        private DGView m_currentView;
        private ListView m_list;

        public override void Initial(DGNode node, string tableLocalizationName = null)
        {
            base.Initial(node, tableLocalizationName);
        }

        public override void Draw()
        {
            base.Draw();

            // Input Port
            Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "Input";
            InputPorts.Add(input);
            inputContainer.Add(input);

            // Output Ports
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "Output";
            OutputPorts.Add(output);
            outputContainer.Add(output);

            // Extension container
            VisualElement customContainer = new VisualElement();
            m_list = DrawList();

            customContainer.Add(m_list);
            extensionContainer.Add(customContainer);
            RefreshExpandedState();
        }

        public void SetView(DGView view)
        {
            m_currentView = view;
            view.blackboard.onBlackboardChanged += () => RebuildList();
        }

        private void RebuildList()
        {
            if (m_list != null)
                m_list.Rebuild();
        }

        private ListView DrawList()
        {
            ListView element = new ListView((nodeData as DGActionNode).Actions, 80f, DrawItem, BindItem);
            element.showAddRemoveFooter = true;
            element.onAdd = (view) => OnAddElementToList(view);
            element.onRemove = (view) => OnRemoveElementFromList(view);
            element.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            element.AddToClassList("dg-node__listview");
            return element;
        }

        private VisualElement DrawItem()
        {
            Foldout listElement = new Foldout();
            listElement.style.flexGrow = 1;

            PopupField<string> variableName = new PopupField<string>();
            VisualElement methodName = new VisualElement();
            VisualElement valueContainer = new VisualElement();

            variableName.AddToClassList("variableName");
            methodName.AddToClassList("methodName");
            valueContainer.AddToClassList("valueContainer");

            variableName.AddToClassList("dg-node__field-base");

            listElement.Add(variableName);
            listElement.Add(methodName);
            listElement.Add(valueContainer);

            return listElement;
        }

        private void BindItem(VisualElement element, int elementIndex)
        {
            Foldout foldout = (element as Foldout);
            DGActions action = (nodeData as DGActionNode).Actions[elementIndex];

            foreach (VisualElement child in element.Children())
            {
                if (child.ClassListContains("variableName"))
                {
                    BindItemVariableName(foldout, child, elementIndex, action);
                }
                if (child.ClassListContains("methodName"))
                {
                    BindItemMethodName(child, elementIndex, action);
                }
                if (child.ClassListContains("valueContainer"))
                {
                    BindItemValueContainet(child, elementIndex, action);
                }
            }
        }

        private void BindItemVariableName(Foldout foldout, VisualElement inputField, int elementIndex, DGActions action)
        {
            (List<string>, List<string>, List<string>) blackboardVariables = m_currentView.blackboard.GetBlackboardVariables();

            PopupField<string> field = inputField as PopupField<string>;
            field.RegisterValueChangedCallback((evt) => { OnVariableNameChanged(evt, elementIndex); });
            field.choices = blackboardVariables.Item1;
            field.label = "Name:";
            if (action != null)
                field.index = blackboardVariables.Item2.IndexOf(action.variableID);

            if (field.index == -1)
            {
                foldout.text = $"Item {elementIndex + 1}";
                action.SetVariale(string.Empty, string.Empty, string.Empty);
            }
            else
                foldout.text = $"Item {action.variableName}";
        }

        private void BindItemMethodName(VisualElement container, int elementIndex, DGActions action)
        {
            container.Clear();
            if (action == null)
            {
                container.SetEnabled(false);
                return;
            }
            if (action.GetVariableType() == null)
            {
                container.SetEnabled(false);
                return;
            }
            if (Type.Equals(action.GetVariableType(), typeof(string)) ||
                Type.Equals(action.GetVariableType(), typeof(bool)))
            {
                container.SetEnabled(false);
                return;
            }

            PopupField<string> methodPopup = new PopupField<string>();
            methodPopup.label = "Method:";

            string[] choices = new string[0];
            if (Type.Equals(action.GetVariableType(), typeof(object)))
            {
                choices = DGObjectUtyliti.GetNameAvaliableMethods(m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(action.variableID), typeof(void));
                methodPopup.RegisterValueChangedCallback((evt) => { OnMethodNameChanged(evt, elementIndex, true); });
            }
            else
            {
                choices = new string[3] { IntFloatMethod.Append.ToString(), IntFloatMethod.Decrease.ToString(), IntFloatMethod.Set.ToString() };
                methodPopup.RegisterValueChangedCallback((evt) => { OnMethodNameChanged(evt, elementIndex, false); });
            }

            methodPopup.choices = choices.ToList();
            if (!string.IsNullOrEmpty(action.methodName))
                methodPopup.index = choices.ToList().IndexOf(action.methodName);
            else
                methodPopup.index = -1;

            methodPopup.AddToClassList("dg-node__field-base");
            container.Add(methodPopup);
        }

        private void BindItemValueContainet(VisualElement container, int elementIndex, DGActions action)
        {
            container.Clear();
            if (action == null)
            {
                container.SetEnabled(false);
                return;
            }
            if (action.GetVariableType() == null)
            {
                container.SetEnabled(false);
                return;
            }

            string label = "Value:";
            VisualElement field = null;
            switch (action.GetVariableType())
            {
                case Type t when t == typeof(string):
                    field = CreateFieldForType(label, action.GetValue<string>(), elementIndex);
                    break;
                case Type t when t == typeof(int):
                    field = CreateFieldForType(label, action.GetValue<int>(), elementIndex);
                    break;
                case Type t when t == typeof(float):
                    field = CreateFieldForType(label, action.GetValue<float>(), elementIndex);
                    break;
                case Type t when t == typeof(bool):
                    field = CreateFieldForType(label, action.GetValue<bool>(), elementIndex);
                    break;
                case Type t when t == typeof(object):
                    PreaperFields(container, action, elementIndex);
                    break;
            }

            container.Add(field);
        }

        private void PreaperFields(VisualElement container, DGActions action, int elementIndex)
        {
            object obj = m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(action.variableID);
            if (obj == null) return;

            List<ParameterInfo> parameters = DGObjectUtyliti.GetParametersFormMethod(obj, action.methodName, typeof(void));
            if (parameters == null || parameters.Count == 0) return;
            for (int i = 0; i < parameters.Count; i++)
            {
                int paramIndex = i;
                string label = parameters[i].Name;
                Type paramType = parameters[i].ParameterType;

                if (paramType == typeof(string))
                    container.Add(CreateFieldForType(label, action.GetValue<string>(), elementIndex, paramIndex));
                else if (paramType == typeof(int))
                    container.Add(CreateFieldForType(label, action.GetValue<int>(), elementIndex, paramIndex));
                else if (paramType == typeof(float))
                    container.Add(CreateFieldForType(label, action.GetValue<float>(), elementIndex, paramIndex));
                else if (paramType == typeof(bool))
                    container.Add(CreateFieldForType(label, action.GetValue<bool>(), elementIndex, paramIndex));
                else if (paramType == typeof(UnityEngine.Object))
                    container.Add(CreateFieldForType(label, action.GetValue<UnityEngine.Object>(), elementIndex, paramIndex));
            }
        }

        private VisualElement CreateFieldForType<T>(string label, T value, int elementIndex, int valueIndex = 0)
        {
            VisualElement field = null;
            Type type = typeof(T);

            if (type == typeof(string))
            {
                DGVariantField<string> textField = new DGVariantField<string>(label, value as string, true);
                RegisterValueActionChange(textField, elementIndex, valueIndex);
                field = textField;
            }
            else if (type == typeof(int))
            {
                DGVariantField<int> intField = new DGVariantField<int>(label, Convert.ToInt32(value), true);
                RegisterValueActionChange(intField, elementIndex, valueIndex);
                field = intField;
            }
            else if (type == typeof(float))
            {
                DGVariantField<float> floatField = new DGVariantField<float>(label, Convert.ToSingle(value), true);
                RegisterValueActionChange(floatField, elementIndex, valueIndex);
                field = floatField;
            }
            else if (type == typeof(bool))
            {
                DGVariantField<bool> boolField = new DGVariantField<bool>(label, Convert.ToBoolean(value), true);
                RegisterValueActionChange(boolField, elementIndex, valueIndex);
                field = boolField;
            }
            else if (type == typeof(UnityEngine.Object))
            {
                DGVariantField<UnityEngine.Object> objectField = new DGVariantField<UnityEngine.Object>(label, value as UnityEngine.Object, true);
                RegisterValueActionChange(objectField, elementIndex, valueIndex);
                field = objectField;
            }

            return field;
        }

        private void RegisterValueActionChange<T>(DGVariantField<T> field, int elementIndex, int valueIndex) =>
            field.RegisterValueChangedCallback((evt) => OnActionValueChanged(evt.newValue, elementIndex, valueIndex));

        private void OnAddElementToList(BaseListView view)
        {
            int count = view.itemsSource.Count;
            view.itemsSource.Add(new DGActions(null, null, null));
            view.RefreshItems();
            view.ScrollToItem(count);
            this.RaiseOnChangeValue();
        }

        private void OnRemoveElementFromList(BaseListView view)
        {
            int count = view.itemsSource.Count;
            view.itemsSource.Remove(view.selectedItem);
            view.RefreshItems();
            view.ScrollToItem(count - 2);
            this.RaiseOnChangeValue();
        }

        private void OnVariableNameChanged(ChangeEvent<string> evt, int elementIndex)
        {
            DGActions action = (nodeData as DGActionNode).Actions[elementIndex];
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (string.Equals(action.variableName, evt.newValue)) return;

            var blackboardVariables = m_currentView.blackboard.GetBlackboardVariables();
            int i = blackboardVariables.Item1.IndexOf(evt.newValue);

            action.SetVariale(blackboardVariables.Item2[i], evt.newValue, blackboardVariables.Item3[i]);

            RebuildList();
            this.RaiseOnChangeValue();
        }

        private void OnMethodNameChanged(ChangeEvent<string> evt, int elementIndex, bool isObjectMethod)
        {
            DGActions action = (nodeData as DGActionNode).Actions[elementIndex];
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (string.Equals(action.methodName, evt.newValue)) return;

            if (isObjectMethod)
                action.SetMethodName(evt.newValue, DGObjectUtyliti.GetParametersFormMethod(m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(action.variableID), evt.newValue, typeof(void)).Count);
            else
                action.SetMethodName(evt.newValue, 1);
            RebuildList();
            this.RaiseOnChangeValue();
        }

        private void OnActionValueChanged(object newValue, int elementIndex, int parameterIndex)
        {
            DGActions action = (nodeData as DGActionNode).Actions[elementIndex];
            if (newValue == null) return;

            if (newValue is string s) action.SetValue(s, parameterIndex);
            else if (newValue is int i) action.SetValue(i, parameterIndex);
            else if (newValue is float f) action.SetValue(f, parameterIndex);
            else if (newValue is bool b) action.SetValue(b, parameterIndex);
            else if (newValue is UnityEngine.Object o) action.SetValue(o, parameterIndex);
            this.RaiseOnChangeValue();
        }
    }
}
