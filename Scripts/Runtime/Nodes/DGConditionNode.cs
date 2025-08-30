using DG_with_Localization.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DG_with_Localization.Elements
{
    public class DGConditionNode : DGNode
    {
        [SerializeField]
        private List<DGCondition> m_conditions;

        [SerializeField]
        public string logicalOperator = "and";

        public List<DGCondition> Conditions => m_conditions;

        public DGConditionNode()
        {
            m_conditions = new List<DGCondition>();
        }

        public override string Execute(DGAsset currentGraph, int outputPortId = 0)
        {
            DGNode nextNode = null;
            if (logicalOperator == "and")
            {
                nextNode = ExecutAndOperator(currentGraph);
            }
            else
            {
                nextNode = ExecuteOrOperator(currentGraph);
            }

            if (nextNode == null) return null;
            if (nextNode.NodeType == DGNodeType.ConditionNode || nextNode.NodeType == DGNodeType.ActionNode)
            {
                return nextNode.Execute(currentGraph);
            }

            return nextNode.Guid;
        }

        #region ExecuteMethods
        private DGNode ExecutAndOperator(DGAsset currentGraph)
        {
            foreach (DGCondition condition in m_conditions)
            {
                DGSerializableProperty prop = currentGraph.Properties.Where(p => string.Equals(p.id, condition.variableID)).FirstOrDefault();
                bool cResult = condition.CheckCondition(prop);
                Debug.Log($"Condition check return value: {cResult.ToString()}");
                if (!cResult)
                {
                    return GetNodeFromFalseOutput(currentGraph);
                }
            }

            return GetNodeFromTrueOutput(currentGraph);
        }

        private DGNode ExecuteOrOperator(DGAsset currentGraph)
        {
            foreach (DGCondition condition in m_conditions)
            {
                DGSerializableProperty prop = currentGraph.Properties.Where(p => string.Equals(p.id, condition.variableID)).FirstOrDefault();
                bool cResult = condition.CheckCondition(prop);
                Debug.Log($"Condition check return value: {cResult.ToString()}");
                if (cResult)
                {
                    return GetNodeFromTrueOutput(currentGraph);
                }
            }

            return GetNodeFromFalseOutput(currentGraph);
        }

        private DGNode GetNodeFromTrueOutput(DGAsset currentGraph)
        {
            DGNode nextNode = currentGraph.GetNodeFromOutput(this.Guid, 0);
            if (nextNode != null)
            {
                return nextNode;
            }
            return null;
        }

        private DGNode GetNodeFromFalseOutput(DGAsset currentGraph)
        {
            DGNode nextNode = currentGraph.GetNodeFromOutput(this.Guid, 1);
            if (nextNode != null)
            {
                return nextNode;
            }
            return null;
        }
        #endregion
    }
}
