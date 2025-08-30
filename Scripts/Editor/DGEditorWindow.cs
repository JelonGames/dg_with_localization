using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DG_with_Localization.Editor
{
    public class DGEditorWindow : EditorWindow
    {
        public static void Open(DGAsset target)
        {
            DGEditorWindow[] widnows = Resources.FindObjectsOfTypeAll<DGEditorWindow>();

            foreach (var w in widnows)
            {
                if (w.currentGraph == target)
                {
                    w.Focus();
                    return;
                }
            }

            DGEditorWindow window = CreateWindow<DGEditorWindow>(typeof(DGEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(DGAsset)).image);
            window.Load(target);
        }

        [SerializeField]
        private DGAsset m_currentGraph;
        [SerializeField]
        private SerializedObject m_serializedObject;
        [SerializeField]
        private DGView m_currentView;

        //GUI Field
        private DGEditorToolbar m_toolbar;

        public DGAsset currentGraph => m_currentGraph;
        public DGEditorToolbar toolbar => m_toolbar;

        private void OnEnable()
        {
            if (m_currentGraph != null)
                Draw();
        }

        private void OnGUI()
        {
            if (m_currentGraph != null)
            {
                if (EditorUtility.IsDirty(m_currentGraph))
                    this.hasUnsavedChanges = true;
                else
                    this.hasUnsavedChanges = false;
            }
        }

        private void Load(DGAsset target)
        {
            m_currentGraph = target;

            Draw();
        }

        private void Draw()
        {
            DrawToolbar();
            DrawGraph();
            AddStyles();
        }

        private void DrawToolbar()
        {
            m_toolbar = new DGEditorToolbar(m_currentGraph);

            m_toolbar.onLocalizationTableChanged += ChangeValueTableLocalization;
            m_toolbar.onLocalesChanged += ChangeValueLocales;

            rootVisualElement.Add(m_toolbar);
        }

        private void DrawGraph()
        {
            m_serializedObject = new SerializedObject(m_currentGraph);
            m_currentView = new DGView(m_serializedObject, this);
            m_currentView.graphViewChanged += OnChange;
            m_currentView.StretchToParentSize();
            rootVisualElement.Insert(0, m_currentView);
        }

        private void AddStyles()
        {
            string rootDirectory = "Packages/dg_with_localization/Scripts/Editor/Editor Default Resources/";

            StyleSheet rootStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{rootDirectory}RootStyle.uss");
            StyleSheet mainStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{rootDirectory}MainViewStyle.uss");
            StyleSheet nodeStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{rootDirectory}DGNodeStyle.uss");

            rootVisualElement.styleSheets.Add(rootStyle);
            rootVisualElement.styleSheets.Add(mainStyle);
            rootVisualElement.styleSheets.Add(nodeStyle);
        }

        private GraphViewChange OnChange(GraphViewChange graphViewChange)
        {
            EditorUtility.SetDirty(m_currentGraph);
            return graphViewChange;
        }

        #region Toolbar callback methods
        private void ChangeValueTableLocalization(string newValue)
        {
            foreach (DGNodeEditor node in m_currentView.nodesEditor)
            {
                node.SetTableLocalizaiton(newValue);
            }

            m_currentGraph.SetLocalizationTable(newValue);
            m_serializedObject.Update();
            EditorUtility.SetDirty(m_currentGraph);
        }

        private void ChangeValueLocales(string newValue)
        {
            foreach (DGNodeEditor node in m_currentView.nodesEditor)
            {
                node.SetLang(newValue);
            }
        }
        #endregion
    }
}
