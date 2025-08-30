using System.Collections.Generic;
using System.Linq;
using DG_with_Localization.Utility;
using UnityEngine;

namespace DG_with_Localization.Elements
{
    public class DGActionNode : DGNode
    {
        [SerializeField]
        private List<DGActions> m_actions;

        public List<DGActions> Actions => m_actions;

        public DGActionNode()
        {
            m_actions = new List<DGActions>();
        }

        public override string Execute(DGAsset currentGraph, int outputPortId = 0)
        {
            foreach (DGActions action in Actions)
            {
                DGSerializableProperty prop = currentGraph.Properties.Where(m => string.Equals(m.id, action.variableID)).FirstOrDefault();
                if (prop == null) continue;
                action.Execute(prop);
            }

            return base.Execute(currentGraph, 0);
        }
    }
}
