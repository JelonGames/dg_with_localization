using System;
using UnityEditor;
using UnityEngine;

namespace DG_with_Localization.Editor
{
    public class DGBlackboardPopupWindow : PopupWindowContent
    {
        private int m_indexResult = -1;
        public Action<string> onClose;

        public DGBlackboardPopupWindow(Action<string> onClose)
        {
            this.onClose = onClose;
        }

        public override void OnGUI(Rect rect)
        {
            //m_popupField = new PopupField<string>("Type", GetBlackboardTypesArray().ToList<string>(), 0);
            EditorGUILayout.LabelField("Type:");
            m_indexResult = EditorGUILayout.Popup(m_indexResult, GetBlackboardTypesArray(), GUILayout.ExpandWidth(true));
            if(GUILayout.Button("OK"))
            {
                if(m_indexResult < 0)
                    onClose?.Invoke(string.Empty);
                else
                    onClose?.Invoke(GetBlackboardTypesArray()[m_indexResult]);

                editorWindow.Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                onClose?.Invoke(string.Empty);
                editorWindow.Close();
            }
        }

        private string[] GetBlackboardTypesArray()
        {
            return new string[]
            {
                typeof(string).Name,
                typeof(int).Name,
                typeof(float).Name,
                typeof(bool).Name,
                typeof(object).Name
            };
        }
    }
}