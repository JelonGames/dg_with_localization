using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.Localization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGEditorToolbar : Toolbar
    {
        private DGAsset m_graph;
        private ToolbarMenu m_localesMenu;

        public event Action onHideBlackboardBtnClick;
        public event Action<string> onLocalizationTableChanged;
        public event Action<string> onLocalesChanged;

        public DGEditorToolbar(DGAsset graph)
        {
            m_graph = graph;

            Draw();
        }

        private void Draw()
        {
            this.Add(DrawBlackboardButton());
            this.Add(DrawLocalizationTabelMenu());
            this.Add(DrawLocalesMenu());
        }

        // private void UpdateToolkit()
        // {
        //     if (m_langDropdownField != null)
        //         m_toolbar.Remove(m_langDropdownField);


        //     m_langDropdownField = new DropdownField("Language", GetLangFromTableLocalization(), -1);
        //     m_langDropdownField.RegisterValueChangedCallback(ChangeValueLang);

        //     m_toolbar.Add(m_langDropdownField);
        // }


        #region Hide/Unhide blackboard button
        private ToolbarButton DrawBlackboardButton()
        {
            ToolbarButton blackboardButton = new ToolbarButton();

            string startButtonText = "Hide Blackboard";
            onHideBlackboardBtnClick += () => ChangeBlackboardButtonText(blackboardButton, startButtonText);

            blackboardButton.text = startButtonText;
            blackboardButton.clicked += () => onHideBlackboardBtnClick?.Invoke();

            return blackboardButton;
        }

        private void ChangeBlackboardButtonText(ToolbarButton btn, string defaultText)
        {
            if (string.Equals(btn.text, defaultText))
                btn.text = "Unhide Blackboard";
            else
                btn.text = defaultText;
        }
        #endregion

        #region DropFields
        private ToolbarMenu DrawLocalizationTabelMenu()
        {
            ToolbarMenu localizationTableMenu = new ToolbarMenu();

            localizationTableMenu.text = "Localization Table";
            DrawLocalizationTableMenuItems(localizationTableMenu);
            onLocalizationTableChanged += UpdateLocalesMenuItems;

            return localizationTableMenu;
        }

        private void DrawLocalizationTableMenuItems(ToolbarMenu localizationTableMenu)
        {
            (List<string>, int) tablesAndDefaultIndexItem = GetLocalizationTablesAndDefaultIndexItem();
            string selectedItem = string.Empty;

            if (tablesAndDefaultIndexItem.Item2 >= 0) selectedItem = tablesAndDefaultIndexItem.Item1[tablesAndDefaultIndexItem.Item2];

            foreach (string item in tablesAndDefaultIndexItem.Item1)
            {
                localizationTableMenu.menu.AppendAction(item, action =>
                {
                    if (string.Equals(action.name, selectedItem)) return;

                    selectedItem = action.name;
                    onLocalizationTableChanged?.Invoke(action.name);
                },
                action => string.Equals(action.name, selectedItem) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
        }

        private void UpdateLocalesMenuItems(string localizationTable)
        {
            if (m_localesMenu == null)
                return;

            m_localesMenu.menu.ClearItems();
            DrawLocalesMenuItems(m_localesMenu, localizationTable);
        }

        private ToolbarMenu DrawLocalesMenu()
        {
            m_localesMenu = new ToolbarMenu();

            m_localesMenu.text = "Locales";
            DrawLocalesMenuItems(m_localesMenu);

            return m_localesMenu;
        }

        private void DrawLocalesMenuItems(ToolbarMenu localesMenu, string localizationTabel = "")
        {
            List<string> locales = GetLocalesFromTableLocalization(localizationTabel);
            string selectedItem = locales.FirstOrDefault();

            foreach (string locale in locales)
            {
                localesMenu.menu.AppendAction(locale, action =>
                {
                    if (string.Equals(action.name, selectedItem)) return;

                    selectedItem = action.name;
                    onLocalesChanged?.Invoke(action.name);
                }, action => string.Equals(action.name, selectedItem) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
        }
        #endregion

        #region Utility
        private (List<string>, int) GetLocalizationTablesAndDefaultIndexItem()
        {
            List<string> localizationTablesName = new List<string>();
            int defaultIndexItem = -1;
            ReadOnlyCollection<StringTableCollection> tables = LocalizationEditorSettings.GetStringTableCollections();
            if (tables != null && tables.Count > 0)
            {
                foreach (StringTableCollection table in tables)
                {
                    if (table.Group == "String Table")
                    {
                        if (table.TableCollectionName.ToString().StartsWith("DG_"))
                        {
                            localizationTablesName.Add(table.TableCollectionNameReference);
                            if (m_graph.LocalizationTable == table.TableCollectionNameReference)
                                defaultIndexItem = localizationTablesName.IndexOf(table.TableCollectionNameReference);
                        }
                    }
                }
            }

            return (localizationTablesName, defaultIndexItem);
        }

        private List<string> GetLocalesFromTableLocalization(string localizationTabel)
        {
            List<string> locales = new List<string>();
            //langs.Add(string.Empty);

            StringTableCollection stringTableCollection = string.IsNullOrEmpty(localizationTabel) ?
                LocalizationEditorSettings.GetStringTableCollection(m_graph.LocalizationTable) : LocalizationEditorSettings.GetStringTableCollection(localizationTabel);

            if (stringTableCollection == null)
                return locales;

            ReadOnlyCollection<LazyLoadReference<LocalizationTable>> tables = stringTableCollection.Tables;
            foreach (var t in tables)
            {
                if (!t.isSet)
                    continue;

                locales.Add(t.asset.LocaleIdentifier.Code);
            }

            return locales;
        }
        #endregion
    }
}