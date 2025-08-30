using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace DG_with_Localization.Editor
{
    [CustomEditor(typeof(DGAsset))]
    public class DGAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open"))
            {
                DGEditorWindow.Open((DGAsset)target);
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            DGAsset asset = (DGAsset)EditorUtility.InstanceIDToObject(instanceID);
            if (asset == null)
                return false;

            DGEditorWindow.Open(asset);
            return true;
        }
    }
}
