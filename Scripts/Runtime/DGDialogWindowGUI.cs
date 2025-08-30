using System.Collections.Generic;
using DG_with_Localization.Elements;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace DG_with_Localization
{
    public class DGDialogWindowGUI : MonoBehaviour
    {
        private static DGDialogWindowGUI m_current;
        private DGAsset m_graphAsset;
        private DGNode m_currentNode;
        private List<Button> m_choiceButtons;

        [SerializeField] private GameObject prefabChoiceButton;
        [SerializeField] private GameObject dialogWindow;
        [SerializeField] private GameObject dialogWithoutInterlocutorImageGameObject;
        [SerializeField] private GameObject dialogWithInterlocutorImageGameObject;
        [SerializeField] private GameObject choicesGameObject;

        public static DGDialogWindowGUI Current => m_current;

        private void Awake()
        {
            if (m_current == null) m_current = this;
            else Destroy(this);

            m_choiceButtons = new List<Button>();
        }

        public void OpenDialogWindow(DGAsset graphAsset)
        {
            m_graphAsset = graphAsset;
            DGNode startNode = m_graphAsset.GetStartNode();
            dialogWindow.SetActive(true);

            m_currentNode = startNode;
            ContinueDialog();
        }

        private void CloseDialogueWindow()
        {
            dialogWindow.SetActive(false);
        }

        public void ContinueDialog(int outputPortId = 0)
        {
            DGNode nextnode = GetNextNode(m_currentNode, outputPortId);

            //Diasable window
            if (nextnode == null)
            {
                Debug.Log("Next node is null");
                CloseDialogueWindow();
                return;
            }
            Debug.Log($"Next node is {nextnode.Guid}");
            m_currentNode = nextnode;

            //Switch action
            switch (nextnode.NodeType)
            {
                case DGNodeType.DialogueNode:
                    ViewDialogNode((nextnode as DGDialogueNode));
                    break;
                case DGNodeType.ChoiceNode:
                    PrepareChoiceButtons((nextnode as DGChoiceNode));
                    break;
            }
        }

        private void ViewDialogNode(DGDialogueNode node)
        {
            LocalizeStringEvent localize;
            choicesGameObject.SetActive(false);
            dialogWithInterlocutorImageGameObject.SetActive(false);
            dialogWithoutInterlocutorImageGameObject.SetActive(false);

            if (node.interlocutorImage != null)
            {
                dialogWithInterlocutorImageGameObject.SetActive(true);
                localize = dialogWithInterlocutorImageGameObject.GetComponentInChildren<LocalizeStringEvent>();
                Image interlocutorImage = dialogWithInterlocutorImageGameObject.GetComponentInChildren<Image>();

                interlocutorImage.sprite = node.interlocutorImage;
            }
            else
            {
                dialogWithoutInterlocutorImageGameObject.SetActive(true);
                localize = dialogWithoutInterlocutorImageGameObject.GetComponentInChildren<LocalizeStringEvent>();
            }

            SetLocalization(localize, node.localizationKey);
        }

        private void PrepareChoiceButtons(DGChoiceNode node)
        {
            choicesGameObject.SetActive(true);
            dialogWithoutInterlocutorImageGameObject.SetActive(false);
            dialogWithInterlocutorImageGameObject.SetActive(false);
            ScrollRect scrollRect = choicesGameObject.GetComponentInChildren<ScrollRect>();

            if (m_choiceButtons.Count != 0)
            {
                RemoveButtons();
            }

            CreateChoiceButtons(node, scrollRect.content);
        }

        private void RemoveButtons()
        {
            foreach (Button b in m_choiceButtons)
            {
                Destroy(b.gameObject);
            }
        }

        private void CreateChoiceButtons(DGChoiceNode node, RectTransform content)
        {
            Button previousButton = null;

            for (int i = 0; i < node.Choices.Count; i++)
            {
                int index = i;

                GameObject buttonGameObject = Instantiate(prefabChoiceButton, content.transform);
                Button button = buttonGameObject.GetComponent<Button>();
                LocalizeStringEvent localize = buttonGameObject.GetComponentInChildren<LocalizeStringEvent>();

                button.navigation = SetNavigation(button, previousButton);
                m_choiceButtons.Add(button);
                previousButton = button;

                SetLocalization(localize, node.Choices[index]);

                SetOnClick(button, index);
            }
        }

        private Navigation SetNavigation(Button button, Button previousButton)
        {
            Navigation nav = button.navigation;

            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = previousButton;

            if (previousButton != null)
            {
                Navigation previousNav = previousButton.navigation;
                previousNav.selectOnDown = button;
                previousButton.navigation = previousNav;
            }

            return nav;
        }

        private void SetLocalization(LocalizeStringEvent localize, string localizeEntry)
        {
            localize.SetTable(m_graphAsset.LocalizationTable);
            localize.SetEntry(localizeEntry);
            localize.RefreshString();
        }

        private void SetOnClick(Button button, int index)
        {
            button.onClick.AddListener(() =>
            {
                ContinueDialog(index);
            });
        }

        private DGNode GetNextNode(DGNode node, int outputPortId)
        {
            string nextNodeId = node.Execute(m_graphAsset, outputPortId);
            if (!string.IsNullOrEmpty(nextNodeId))
            {
                DGNode nextNode = m_graphAsset.GetNode(nextNodeId);
                return nextNode;
            }

            return null;
        }
    }
}
