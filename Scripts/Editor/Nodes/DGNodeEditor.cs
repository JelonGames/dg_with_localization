using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using DG_with_Localization.Elements;
using System;

namespace DG_with_Localization.Editor
{
    public class DGNodeEditor : Node
    {
        private List<Port> m_inputPorts;
        private List<Port> m_outputPorts;
        private DGNode m_nodeData;

        protected string m_tableLocalizationName;
        protected string m_lang;

        protected event Action OnChangedTableLocalization;
        protected event Action OnChangedLang;

        // Event to mark dirty DGAsset
        public event Action OnChangedValue;

        public DGNode nodeData => m_nodeData;
        public List<Port> InputPorts => m_inputPorts;
        public List<Port> OutputPorts => m_outputPorts;


        public virtual void Initial(DGNode node, string tableLocalizationName = null)
        {
            m_nodeData = node;
            m_inputPorts = new List<Port>();
            m_outputPorts = new List<Port>();
            m_tableLocalizationName = tableLocalizationName;
            extensionContainer.AddToClassList("dg-node__extension-container");

            this.SetPosition(node.Position);
        }

        public virtual void Draw()
        {
            // TitleContainer
            title = m_nodeData.NodeType.ToString();
        }

        internal void SavePosition() => m_nodeData.SetPosition(this.GetPosition());

        internal void SetTableLocalizaiton(string tableLocalization)
        {
            m_tableLocalizationName = tableLocalization;
            OnChangedTableLocalization?.Invoke();
        }

        internal void SetLang(string lang)
        {
            m_lang = lang;
            OnChangedLang?.Invoke();
        }

        // Event to mark dirty DGAsset
        protected void RaiseOnChangeValue() => OnChangedValue?.Invoke();
    }
}
