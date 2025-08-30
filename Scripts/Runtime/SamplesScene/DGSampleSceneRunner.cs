using UnityEngine;

namespace DG_with_Localization.Samples
{
    public class DGSampleSceneRunner : MonoBehaviour
    {
        void Start()
        {
            gameObject.GetComponent<DGActor>().StartDialog();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DGDialogWindowGUI.Current.ContinueDialog();
            }
        }
    }
}
