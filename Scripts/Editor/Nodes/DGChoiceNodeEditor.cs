using System.Collections.Generic;
using System.Linq;
using DG_with_Localization.Elements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGChoiceNodeEditor : DGNodeEditor
    {
        private DGView m_view;

        public override void Initial(DGNode node, string tableLocalizationName = null)
        {
            base.Initial(node, tableLocalizationName);
            OnChangedTableLocalization += () => { (nodeData as DGChoiceNode).Choices.Clear(); };
            OnChangedTableLocalization += () => { outputContainer.Clear(); };

            OnChangedLang += () => { ChangeLangCallback(); };
        }

        public void SetView(DGView view) => m_view = view;

        public override void Draw()
        {
            base.Draw();

            //Input Port
            Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPorts.Add(input);
            inputContainer.Add(input);

            //Main container button
            Button addChoiceBtn = new Button();
            addChoiceBtn.text = "Add Choice";
            addChoiceBtn.clicked += AddChoiceEvent;
            mainContainer.Insert(1, addChoiceBtn);

            //Output Ports
            int choiceCount = (nodeData as DGChoiceNode).Choices.Count;
            for (int i = 0; i < choiceCount; i++)
            {
                DrawOutputsPort(i);
            }

            RefreshExpandedState();
        }

        private void DrawOutputsPort(int index)
        {
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "";
            output.style.height = StyleKeyword.Auto;
            OutputPorts.Add(output);

            VisualElement choiceContainer = new VisualElement();

            Button removeChoiceBtn = new Button();
            removeChoiceBtn.text = "REMOVE";
            removeChoiceBtn.clicked += () => RemoveChoiceEvent(output);

            PopupField<string> choicePopup = new PopupField<string>();
            SetChoisePopupSettings(choicePopup, (nodeData as DGChoiceNode).Choices[index]);
            choicePopup.RegisterValueChangedCallback((evt) => { SetChoiseCallback(evt, index); });

            TextField dialogText = new TextField();
            SetDialogTextSettings(dialogText, index);

            choiceContainer.AddToClassList("choiceContainer");
            choiceContainer.AddToClassList("dg-node__choice-container");
            choicePopup.AddToClassList("dg-node__field-base");
            dialogText.AddToClassList("dg-node__field-base");
            dialogText.AddToClassList("dg-node__textfield-readonly");

            choiceContainer.Add(removeChoiceBtn);
            choiceContainer.Add(choicePopup);
            choiceContainer.Add(dialogText);

            output.Add(choiceContainer);
            outputContainer.Add(output);
        }

        private void SetChoisePopupSettings(PopupField<string> popupField, string popupValue)
        {
            List<string> entries = GetEntriesFromTableLocalization();
            int defaultIndex = -1;
            for (int i = 0; i < entries.Count; i++)
            {
                if (string.IsNullOrEmpty(popupValue))
                    break;

                if (string.Equals(entries[i], popupValue))
                {
                    defaultIndex = i;
                    break;
                }
            }

            popupField.choices = entries;
            popupField.index = defaultIndex;
        }

        private void SetDialogTextSettings(TextField dialogText, int choiceValue)
        {
            string value = GetTranslation(choiceValue);

            dialogText.value = value;
            dialogText.multiline = true;
            dialogText.isReadOnly = true;
            dialogText.style.flexWrap = Wrap.Wrap;
            dialogText.style.whiteSpace = WhiteSpace.PreWrap;
        }

        #region Callback methods
        private void AddChoiceEvent()
        {
            (nodeData as DGChoiceNode).Choices.Add("");
            DrawOutputsPort((nodeData as DGChoiceNode).Choices.Count - 1);
            this.RaiseOnChangeValue();
        }

        private void RemoveChoiceEvent(Port port)
        {
            int index = OutputPorts.IndexOf(port);

            foreach (Port p in OutputPorts)
            {
                foreach (Edge edge in p.connections)
                {
                    m_view.RemoveEdge(edge);
                    m_view.RemoveElement(edge);
                }
            }

            (nodeData as DGChoiceNode).Choices.RemoveAt(index);
            OutputPorts.Remove(port);
            outputContainer.Remove(port);

            this.RaiseOnChangeValue();
        }

        private void SetChoiseCallback(ChangeEvent<string> evt, int index)
        {
            (nodeData as DGChoiceNode).Choices[index] = evt.newValue;
            this.RaiseOnChangeValue();

            Port output = (Port)outputContainer[index];
            VisualElement choiceContainer = output.Children().Where(e => e.ClassListContains("choiceContainer")).FirstOrDefault();
            if (choiceContainer == null)
                return;

            TextField dialogText = (TextField)choiceContainer.Children().Where(e => e is TextField).FirstOrDefault();
            if (dialogText == null)
                return;

            dialogText.value = GetTranslation(index);
        }

        private void ChangeLangCallback()
        {
            int outputPortCount = outputContainer.childCount;
            var ports = outputContainer.Children();

            for (int i = 0; i < outputPortCount; i++)
            {
                Port port = (Port)ports.ElementAt(i);

                VisualElement choiceContainer = port.Children().Where(e => e.ClassListContains("choiceContainer")).FirstOrDefault();
                if (choiceContainer == null)
                    continue;

                TextField dialogText = (TextField)choiceContainer.Children().Where(e => e is TextField).FirstOrDefault();
                if (dialogText == null)
                    continue;

                dialogText.value = GetTranslation(i);
            }
        }
        #endregion

        #region Utylity
        public List<string> GetEntriesFromTableLocalization()
        {
            List<string> entres = new List<string>();
            entres.Add(string.Empty);

            if (string.IsNullOrEmpty(m_tableLocalizationName))
                return entres;

            StringTableCollection stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(m_tableLocalizationName);

            if (stringTableCollection == null)
                return entres;

            foreach (var entry in stringTableCollection.SharedData.Entries)
            {
                entres.Add(entry.Key);
            }

            return entres;
        }

        private string GetTranslation(int index)
        {
            StringTableCollection stringTableCollection = LocalizationEditorSettings.GetStringTableCollection(m_tableLocalizationName);
            if (stringTableCollection == null)
                return "No dialog traslation found";

            StringTable table = stringTableCollection.GetTable(m_lang) as StringTable;
            if (table == null)
            {
                string locales = stringTableCollection.Tables.ToList().Where(t => t.isSet).FirstOrDefault().asset.LocaleIdentifier.Code;
                if (string.IsNullOrEmpty(locales))
                    return "No dialog traslation found";

                table = stringTableCollection.GetTable(locales) as StringTable;
            }

            if (string.IsNullOrEmpty((nodeData as DGChoiceNode).Choices[index]))
                return "No set key";

            var entry = table.GetEntry((nodeData as DGChoiceNode).Choices[index]);
            if (entry == null)
                return "No dialog translation found";

            return entry.GetLocalizedString();
        }
        #endregion
    }
}
