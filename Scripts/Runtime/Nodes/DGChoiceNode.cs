using System;
using System.Collections.Generic;
using UnityEngine;

namespace DG_with_Localization.Elements
{
    [Serializable]
    public class DGChoiceNode : DGNode
    {
        [SerializeField]
        private List<string> m_choices;

        public List<string> Choices => m_choices;

        public DGChoiceNode()
        {
            m_choices = new List<string>();
            m_choices.Add("");
        }
    }
}
