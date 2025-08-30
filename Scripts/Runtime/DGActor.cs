using UnityEngine;

namespace DG_with_Localization
{
    public class DGActor : MonoBehaviour
    {
        [SerializeField]
        private DGAsset m_graphAsset;
        private DGAsset m_currentGraphAsset;

        private void OnEnable()
        {
            ExecuteAsset(Instantiate(m_graphAsset));
        }

        private void ExecuteAsset(DGAsset dGAsset)
        {
            dGAsset.Initialize();
            m_currentGraphAsset = dGAsset;
        }

        public void StartDialog()
        {
            DGDialogWindowGUI.Current.OpenDialogWindow(m_currentGraphAsset);
        }
    }
}
