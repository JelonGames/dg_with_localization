using System;
using UnityEngine;

namespace DG_with_Localization.Elements
{
    [Serializable]
    public class DGNode
    {
        [SerializeField]
        private string m_guid;
        [SerializeField]
        private Rect m_position;
        [SerializeField]
        private DGNodeType m_nodeType;

        public string Guid => m_guid;
        public Rect Position => m_position;
        public DGNodeType NodeType => m_nodeType;

        public virtual void Initialize(DGNodeType type = DGNodeType.DialogueNode)
        {
            NewID();
            m_nodeType = type;
        }

        public virtual void Initialize(Vector2 position, DGNodeType type = DGNodeType.DialogueNode)
        {
            NewID();
            SetPosition(position);
            m_nodeType = type;
        }

        public virtual void Initialize(Rect position, DGNodeType type = DGNodeType.DialogueNode)
        {
            NewID();
            SetPosition(position);
            m_nodeType = type;
        }

        private void NewID()
        {
            m_guid = System.Guid.NewGuid().ToString();
        }

        public void SetPosition(Rect position)
        {
            m_position = position;
        }

        public void SetPosition(Vector2 position)
        {
            m_position = new Rect(position, Vector2.zero);
        }

        private protected void SetNodeType(DGNodeType type)
        {
            m_nodeType = type;
        }

        public virtual string Execute(DGAsset currentGraph, int outputPortId = 0)
        {
            DGNode nextNode = currentGraph.GetNodeFromOutput(this.Guid, outputPortId);
            if (nextNode == null) return null;

            if (nextNode.NodeType == DGNodeType.ConditionNode || nextNode.NodeType == DGNodeType.ActionNode)
            {
                return nextNode.Execute(currentGraph);
            }

            return nextNode.Guid;
        }
    }

    public enum DGNodeType
    {
        StartNode,
        DialogueNode,
        ChoiceNode,
        ConditionNode,
        ActionNode
    }
}
