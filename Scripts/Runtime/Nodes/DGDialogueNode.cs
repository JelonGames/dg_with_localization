using System;
using UnityEngine;

namespace DG_with_Localization.Elements
{
    [Serializable]
    public class DGDialogueNode : DGNode
    {
        [SerializeField]
        public string localizationKey;
        [SerializeField]
        public Sprite interlocutorImage;
    }
}