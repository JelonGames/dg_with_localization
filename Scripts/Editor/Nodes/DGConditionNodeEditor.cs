using DG_with_Localization.Elements;
using DG_with_Localization.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGConditionNodeEditor : DGNodeEditor
    {
        private DGView m_currentView;
        private ListView m_listView;

        public override void Initial(DGNode node, string tableLocalizationName = null)
        {
            base.Initial(node, tableLocalizationName);
        }

        public void SetView(DGView view)
        {
            m_currentView = view;
            view.blackboard.onBlackboardChanged += () => RebuildList();
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
            Port outputTrue = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            Port outputFalse = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputTrue.portName = "True";
            outputFalse.portName = "False";
            OutputPorts.Add(outputTrue);
            OutputPorts.Add(outputFalse);
            outputContainer.Add(outputTrue);
            outputContainer.Add(outputFalse);

            // Extension container
            VisualElement customContainer = new VisualElement();
            PopupField<string> logicalOperatorPopup = DrawPopupLogicalOperator();
            logicalOperatorPopup.AddToClassList("dg-node__field-base");
            m_listView = DrawList();

            customContainer.Add(logicalOperatorPopup);
            customContainer.Add(m_listView);
            extensionContainer.Add(customContainer);
            RefreshExpandedState();
        }

        private PopupField<string> DrawPopupLogicalOperator()
        {
            List<string> choices = new List<string>() { "and", "or" };
            int index = choices.IndexOf((nodeData as DGConditionNode).logicalOperator);
            PopupField<string> popup = new PopupField<string>("Logical Operator:", choices, index);
            popup.RegisterValueChangedCallback((evt) =>
            {
                if (string.IsNullOrEmpty(evt.newValue)) return;
                if (evt.newValue == (nodeData as DGConditionNode).logicalOperator) return;

                (nodeData as DGConditionNode).logicalOperator = evt.newValue;
                this.RaiseOnChangeValue();
            });

            return popup;
        }

        private void RebuildList()
        {
            if (m_listView != null)
                m_listView.Rebuild();
        }

        private ListView DrawList()
        {
            ListView element = new ListView((nodeData as DGConditionNode).Conditions, 80, DrawItem, BindItem);
            element.showAddRemoveFooter = true;
            element.onAdd = (view) => OnAddElementToList(view);
            element.onRemove = (view) => OnRemoveElementFromList(view);
            element.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            element.AddToClassList("dg-node__listview");
            return element;
        }

        private VisualElement DrawItem()
        {
            Foldout container = new Foldout();
            container.style.flexGrow = 1;

            PopupField<string> variableName = new PopupField<string>();
            PopupField<string> actionName = new PopupField<string>();
            VisualElement valueContainer = new VisualElement();

            variableName.AddToClassList("variableName");
            actionName.AddToClassList("actionName");
            valueContainer.AddToClassList("valueContainer");

            variableName.AddToClassList("dg-node__field-base");
            actionName.AddToClassList("dg-node__field-base");

            container.Add(variableName);
            container.Add(actionName);
            container.Add(valueContainer);
            return container;
        }

        private void BindItem(VisualElement element, int elementIndex)
        {
            Foldout foldout = (element as Foldout);
            DGCondition condition = (nodeData as DGConditionNode).Conditions[elementIndex];

            foreach (VisualElement e in element.Children())
            {
                if (e.ClassListContains("variableName"))
                    BindItemVarriableName(foldout, e, elementIndex, condition);
                if (e.ClassListContains("actionName"))
                    BindItemActionName(e, elementIndex, condition);
                if (e.ClassListContains("valueContainer"))
                    BindItemValueContainer(e, elementIndex, condition);
            }
        }

        private void BindItemVarriableName(Foldout foldout, VisualElement inputField, int elementIndex, DGCondition condition)
        {
            (List<string>, List<string>, List<string>) blackboardVariables = m_currentView.blackboard.GetBlackboardVariables();

            PopupField<string> field = inputField as PopupField<string>;
            field.RegisterValueChangedCallback((evt) => { OnVariableNameChanged(evt, elementIndex); });
            field.choices = blackboardVariables.Item1;
            field.label = "Name:";
            if (condition != null)
                field.index = blackboardVariables.Item2.IndexOf(condition.variableID);

            if (field.index == -1)
            {
                foldout.text = $"Item {elementIndex + 1}";
                condition.SetVariable(string.Empty, string.Empty, string.Empty);
            }
            else
                foldout.text = $"Item {condition.variableName}";
        }

        private void BindItemActionName(VisualElement inputField, int elementIndex, DGCondition condition)
        {
            if (condition == null || string.IsNullOrEmpty(condition.variableID))
            {
                inputField.style.display = DisplayStyle.None;
                return;
            }

            PopupField<string> field = inputField as PopupField<string>;
            field.label = "Aciton:";

            string[] operation = new string[0];
            switch (condition.GetVariableType())
            {
                case Type v when v == typeof(bool) || v == typeof(string):
                    operation = new string[] { ComparisonOperator.Equals.ToString(), ComparisonOperator.NotEquals.ToString() };
                    break;

                case Type v when v == typeof(int) || v == typeof(float):
                    operation = Enum.GetNames(typeof(ComparisonOperator));
                    break;

                case Type v when v == typeof(object):
                    operation = DGObjectUtyliti.GetNameAvaliableMethods(m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(condition.variableID), typeof(bool));
                    break;
            }

            field.choices = operation.ToList();
            if (!string.IsNullOrEmpty(condition.actionName))
                field.index = operation.ToList().IndexOf(condition.actionName);

            field.RegisterValueChangedCallback((evt) => OnActionNameChanged(evt, elementIndex));
        }

        private void BindItemValueContainer(VisualElement element, int elementIndex, DGCondition condition)
        {
            element.Clear();
            if (condition == null) return;
            if (condition.GetVariableType() == null) return;
            if (string.IsNullOrEmpty(condition.actionName)) return;

            VisualElement field = null;
            Type conditionType = condition.GetVariableType();
            if (conditionType == typeof(string)) field = CreateFieldForType(condition.GetValue<string>(), elementIndex);
            else if (conditionType == typeof(int)) field = CreateFieldForType(condition.GetValue<int>(), elementIndex);
            else if (conditionType == typeof(float)) field = CreateFieldForType(condition.GetValue<float>(), elementIndex);
            else if (conditionType == typeof(bool)) field = CreateFieldForType(condition.GetValue<bool>(), elementIndex);
            else if (conditionType == typeof(object))
                field = SetFieldForMethodParameter(
                    m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(condition.variableID),
                    condition,
                    elementIndex
                    );

            element.Add(field);
        }
        private VisualElement SetFieldForMethodParameter(UnityEngine.Object obj, DGCondition condition, int elementIndex)
        {
            if (obj == null) return null;

            List<MethodInfo> methods = DGObjectUtyliti.GetMethodsFromObject(obj, typeof(bool));
            int index = DGObjectUtyliti.GetNameAvaliableMethods(obj, typeof(bool)).ToList().IndexOf(condition.actionName);
            if (methods == null || index < 0) return null;

            List<ParameterInfo> parameters = methods[index].GetParameters().ToList();
            if (parameters == null || parameters.Count == 0) return null;

            VisualElement container = new VisualElement();

            for (int i = 0; i < parameters.Count; i++)
            {
                int valueIndex = i;
                VisualElement field = null;

                switch (parameters[i].ParameterType)
                {
                    case Type t when t == typeof(string):
                        field = CreateFieldForType(condition.GetValue<string>(), elementIndex, valueIndex);
                        break;
                    case Type t when t == typeof(int):
                        field = CreateFieldForType(condition.GetValue<int>(), elementIndex, valueIndex);
                        break;
                    case Type t when t == typeof(float):
                        field = CreateFieldForType(condition.GetValue<float>(), elementIndex, valueIndex);
                        break;
                    case Type t when t == typeof(bool):
                        field = CreateFieldForType(condition.GetValue<bool>(), elementIndex, valueIndex);
                        break;
                    case Type t when t == typeof(object) ||
                        t == typeof(UnityEngine.Object):
                        field = CreateFieldForType(condition.GetValue<UnityEngine.Object>(), elementIndex, valueIndex);
                        break;
                }

                container.Add(field);
            }

            return container;
        }

        private VisualElement CreateFieldForType<T>(T value, int elementIndex, int valueIndex = 0)
        {
            VisualElement field = null;
            string label = "Value:";
            Type type = typeof(T);

            if (type == typeof(string))
            {
                DGVariantField<string> textField = new DGVariantField<string>(label, value as string, true);
                RegisterValueChange(textField, elementIndex, valueIndex);
                field = textField;
            }
            else if (type == typeof(int))
            {
                DGVariantField<int> intField = new DGVariantField<int>(label, Convert.ToInt32(value), true);
                RegisterValueChange(intField, elementIndex, valueIndex);
                field = intField;
            }
            else if (type == typeof(float))
            {
                DGVariantField<float> floatField = new DGVariantField<float>(label, Convert.ToSingle(value), true);
                RegisterValueChange(floatField, elementIndex, valueIndex);
                field = floatField;
            }
            else if (type == typeof(bool))
            {
                DGVariantField<bool> boolField = new DGVariantField<bool>(label, Convert.ToBoolean(value), true);
                RegisterValueChange(boolField, elementIndex, valueIndex);
                field = boolField;
            }
            else if (type == typeof(UnityEngine.Object))
            {
                DGVariantField<UnityEngine.Object> objectField = new DGVariantField<UnityEngine.Object>(label, value as UnityEngine.Object, true);
                RegisterValueChange(objectField, elementIndex, valueIndex);
                field = objectField;
            }

            return field;
        }

        private void RegisterValueChange<T>(DGVariantField<T> field, int elementIndex, int valueIndex) =>
            field.RegisterValueChangedCallback((evt) => OnValueFieldChange(evt.newValue, elementIndex, valueIndex));

        private void OnAddElementToList(BaseListView view)
        {
            int count = view.itemsSource.Count;
            view.itemsSource.Add(new DGCondition(null, null, null));
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
            DGCondition condition = (nodeData as DGConditionNode).Conditions[elementIndex];
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (string.Equals(condition.variableName, evt.newValue)) return;

            var variables = m_currentView.blackboard.GetBlackboardVariables();

            int i = variables.Item1.IndexOf(evt.newValue);

            condition.SetVariable(variables.Item2[i], evt.newValue, variables.Item3[i]);

            RebuildList();
            this.RaiseOnChangeValue();
        }

        private void OnActionNameChanged(ChangeEvent<string> evt, int elementIndex)
        {
            DGCondition condition = (nodeData as DGConditionNode).Conditions[elementIndex];
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (condition.actionName == evt.newValue) return;

            if (condition.GetVariableType() == typeof(string) ||
                    condition.GetVariableType() == typeof(int) ||
                    condition.GetVariableType() == typeof(float) ||
                    condition.GetVariableType() == typeof(bool))
                condition.SetActionName(evt.newValue, 1);
            else
            {
                int variantCount = DGObjectUtyliti.GetParametersFormMethod(m_currentView.blackboard.GetBlackboardData<UnityEngine.Object>(condition.variableID), evt.newValue, typeof(bool)).Count;
                condition.SetActionName(evt.newValue, variantCount);
            }

            RebuildList();
            this.RaiseOnChangeValue();
        }

        private void OnValueFieldChange(object newValue, int elementIndex, int valueIndex = 0)
        {
            DGCondition condition = (nodeData as DGConditionNode).Conditions[elementIndex];
            if (newValue == null) return;

            if (newValue is string s) condition.SetValue(s, valueIndex);
            else if (newValue is int i) condition.SetValue(i, valueIndex);
            else if (newValue is float f) condition.SetValue(f, valueIndex);
            else if (newValue is bool b) condition.SetValue(b, valueIndex);
            else if (newValue is UnityEngine.Object o) condition.SetValue(o, valueIndex);
            this.RaiseOnChangeValue();
        }
    }
}
