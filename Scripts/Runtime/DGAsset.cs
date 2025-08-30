using UnityEngine;
using System.Collections.Generic;
using DG_with_Localization.Elements;
using System.Linq;
using DG_with_Localization.Utility;

namespace DG_with_Localization
{
    [CreateAssetMenu(fileName = "Dialogue Graph", menuName = "Dialog Graph/DialogueGraphAsset")]
    public class DGAsset : ScriptableObject
    {
        // #if UNITY_2019_3_OR_NEWER
        [SerializeField, SerializeReference]
        // #else
        //         [SerializeField]
        // #endif
        private List<DGNode> m_nodes;
        [SerializeField]
        private List<DGConnection> m_connections;
        [SerializeField]
        private List<DGSerializableProperty> m_properties;
        [SerializeField]
        private string m_localizationTable;

        private Dictionary<string, DGNode> m_nodeDictionary;

        public List<DGNode> Nodes => m_nodes;
        public List<DGConnection> Connections => m_connections;
        public List<DGSerializableProperty> Properties => m_properties;
        public string LocalizationTable => m_localizationTable;

        public DGAsset()
        {
            m_nodes = new List<DGNode>();
            m_connections = new List<DGConnection>();
            m_properties = new List<DGSerializableProperty>();
            m_nodeDictionary = new Dictionary<string, DGNode>();
        }

        public void Initialize()
        {
            foreach (DGNode node in m_nodes)
            {
                m_nodeDictionary.Add(node.Guid, node);
            }
        }

        public void SetLocalizationTable(string tableName)
        {
            m_localizationTable = tableName;
        }

        public DGNode GetStartNode()
        {
            DGStartNode[] startNodes = m_nodes.OfType<DGStartNode>().ToArray();
            if (startNodes.Length == 0) return null;

            return startNodes[0];
        }

        public DGNode GetNode(string nodeGuid) => m_nodeDictionary.TryGetValue(nodeGuid, out DGNode node) ? node : null;
        public DGNode GetNodeFromOutput(string outputNodeID, int indexOutputPort)
        {
            foreach (DGConnection conn in m_connections)
            {
                if (conn.outputPort.nodeID == outputNodeID && conn.outputPort.portIndex == indexOutputPort)
                {
                    string nodeID = conn.inputPort.nodeID;
                    DGNode inputNode = m_nodeDictionary[nodeID];
                    return inputNode;
                }
            }
            return null;
        }
    }
}
