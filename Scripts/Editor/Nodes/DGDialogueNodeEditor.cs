using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization;
using UnityEngine.UIElements;
using DG_with_Localization.Elements;
using UnityEngine.Localization.Tables;
using UnityEditor.UIElements;
using UnityEngine;
using System.Linq;

namespace DG_with_Localization.Editor
{
    public class DGDialogueNodeEditor : DGNodeEditor
    {
        private Foldout m_foldoutDialogText;
        private PopupField<string> m_keyLocalizationPopup;
        private TextField m_dialogText;

        public override void Initial(DGNode node, string tableLocalizationName = null)
        {
            base.Initial(node, tableLocalizationName);
            OnChangedTableLocalization += () => { (nodeData as DGDialogueNode).localizationKey = null; };
            OnChangedTableLocalization += () => { UpdateField(); };

            OnChangedLang += () => { DrawDialogText(); };
        }

        public override void Draw()
        {
            base.Draw();
            DGDialogueNode nodeDialogueNode = nodeData as DGDialogueNode;

            // Input Port
            Port input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "Input";
            InputPorts.Add(input);
            inputContainer.Add(input);

            // Output Port
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "Output";
            OutputPorts.Add(output);
            outputContainer.Add(output);

            // Extension container
            VisualElement customContainer = new VisualElement();

            m_foldoutDialogText = new Foldout() { text = "Dialogue text" };
            DrawKeyLocalizationPopup();
            DrawDialogText();
            customContainer.Add(m_foldoutDialogText);

            Foldout foldoutInterlocutor = new Foldout() { text = "Interlocutor Image" };
            ObjectField interlocutorField = new ObjectField(null);
            interlocutorField.objectType = typeof(UnityEngine.Sprite);
            interlocutorField.allowSceneObjects = false;
            interlocutorField.value = (nodeData as DGDialogueNode).interlocutorImage;
            interlocutorField.RegisterValueChangedCallback(InterlocutorValueChange);
            foldoutInterlocutor.Add(interlocutorField);
            customContainer.Add(foldoutInterlocutor);

            interlocutorField.AddToClassList("dg-node__field-base");
            m_foldoutDialogText.AddToClassList("dg-node__foldout-data-container");
            foldoutInterlocutor.AddToClassList("dg-node__foldout-data-container");

            extensionContainer.Add(customContainer);
            RefreshExpandedState();
        }

        #region Draw Property Field
        private void UpdateField()
        {
            DrawKeyLocalizationPopup();
            DrawDialogText();
        }

        private void DrawKeyLocalizationPopup()
        {
            if (m_keyLocalizationPopup != null)
                m_foldoutDialogText.Remove(m_keyLocalizationPopup);

            List<string> entries = GetEntriesFromTableLocalization();
            entries.Insert(0, string.Empty);

            int defaultIndex = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                if (string.IsNullOrEmpty((nodeData as DGDialogueNode).localizationKey))
                    break;

                if (string.Equals(entries[i], (nodeData as DGDialogueNode).localizationKey))
                {
                    defaultIndex = i;
                    break;
                }
            }

            m_keyLocalizationPopup = new PopupField<string>(entries, defaultIndex);
            m_keyLocalizationPopup.RegisterValueChangedCallback(ChangKeyLocalization);
            m_foldoutDialogText.Add(m_keyLocalizationPopup);

            m_keyLocalizationPopup.AddToClassList("dg-node__field-base");
        }

        private void DrawDialogText()
        {
            if (m_dialogText != null)
                m_foldoutDialogText.Remove(m_dialogText);

            string value = GetTranslation();

            m_dialogText = new TextField();
            m_dialogText.value = value;
            m_dialogText.multiline = true;
            m_dialogText.isReadOnly = true;
            m_dialogText.style.flexWrap = Wrap.Wrap;
            m_dialogText.style.whiteSpace = WhiteSpace.PreWrap;

            m_foldoutDialogText.Add(m_dialogText);

            m_dialogText.AddToClassList("dg-node__field-base");
            m_dialogText.AddToClassList("dg-node__textfield-readonly");
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

        private string GetTranslation()
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

            if (string.IsNullOrEmpty((nodeData as DGDialogueNode).localizationKey))
                return "No set key";

            var entry = table.GetEntry((nodeData as DGDialogueNode).localizationKey);
            if (entry == null)
                return "No dialog translation found";

            return entry.GetLocalizedString();
        }
        #endregion

        #region Callback Methods
        private void ChangKeyLocalization(ChangeEvent<string> evt)
        {
            (nodeData as DGDialogueNode).localizationKey = evt.newValue;
            RaiseOnChangeValue();
            DrawDialogText();
        }

        private void InterlocutorValueChange(ChangeEvent<UnityEngine.Object> evt)
        {
            (nodeData as DGDialogueNode).interlocutorImage = evt.newValue as Sprite;
            RaiseOnChangeValue();
        }
        #endregion
    }
}
