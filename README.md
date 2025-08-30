# Dialog Graph with Localization

![Dialog Graph Logo](./DialogGraphLogo.png)

Unity tool for creating dialogue graphs with support for Unity's Localization package. Enables building branching dialogues with localized text, simplifying multi-language game development.

## Contents

- [Features](#features)  
- [Requirements](#requirements)  
- [Installation](#installation)  
- [Getting Started](#getting-started)  
- [Example](#example)  
- [License](#license)  

## Features

- Integration with Unity Localization package  
- Easy visual dialogue graph creation  
- Support for branching dialogues based on conditionals  
- Actions to modify game data (strings, floats, ints, bools)  
- Player choice support with customizable dialogue options  

## Requirements

- Unity Localization package - [Documentation and License](https://docs.unity3d.com/Packages/com.unity.localization@1.0/manual/index.html)  

## Installation

### Via Package Manager  
1. Open Package Manager  
2. Click "+" and choose "Install package from git URL..."  
3. Paste `https://github.com/JelonGames/dg_with_localization.git`  

### Via Git Clone  
1. Download or clone repository  
2. Move repository to `Packages` directory in your Unity project  
3. Ensure you have the Unity Localization package installed via Package Manager  

## Getting Started

1. Create a localization table using the Unity Localization package (table name should start with `DG_`)  
2. Right-click in Project Explorer and select `Create > Dialog Graph > DialogGraphAsset`  
3. In the Inspector, click the "Open" button or double-click the asset  
4. In the dialog graph editor, select the localization table in the toolbar  
5. Create a Start Node and Dialogue Nodes, then connect them  
6. Add the `DG Dialog Window GUI` component to your dialogue UI  
7. Prepare your own script or use the `DG Actor` component and call the `StartDialog` method  

## Example

Example scene is localized and can be found at:  
`Packages > DialogGraph with Localization > Scenes > SampleScene`  

Example dialog object:  
`Packages > DialogGraph with Localization > ScriptableObject > EmilliAndJamesDialog`  

### Example Dialog Window Script
```C#
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
```

### Example Actor
```C#
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
```

## License

This project is licensed under the MIT License.  
It also depends on the [Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@1.0/license/LICENSE.html), which has its own license terms.