using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DG_with_Localization.Elements;

namespace DG_with_Localization.Editor
{
    public class DGView : GraphView
    {
        private DGAsset m_graph;
        private SerializedObject m_serializedObject;
        private DGEditorWindow m_window;
        private DGBlackboard m_blackboard;

        private List<DGNodeEditor> m_nodesEditor;
        private Dictionary<string, DGNodeEditor> m_nodeDictionary;
        private Dictionary<Edge, DGConnection> m_connectionDictionary;

        internal DGBlackboard blackboard => m_blackboard;
        internal List<DGNodeEditor> nodesEditor => m_nodesEditor;

        public object OnSelectTypeVariable { get; private set; }

        public DGView(SerializedObject serializedObject, DGEditorWindow window)
        {
            m_serializedObject = serializedObject;
            m_graph = (DGAsset)serializedObject.targetObject;
            m_window = window;

            m_nodesEditor = new List<DGNodeEditor>();
            m_nodeDictionary = new Dictionary<string, DGNodeEditor>();
            m_connectionDictionary = new Dictionary<Edge, DGConnection>();

            this.AddManipulator(AddContextualMenu("Add start node", DGNodeType.StartNode));
            this.AddManipulator(AddContextualMenu("Add dialog node", DGNodeType.DialogueNode));
            this.AddManipulator(AddContextualMenu("Add choice node", DGNodeType.ChoiceNode));
            this.AddManipulator(AddContextualMenu("Add condition node", DGNodeType.ConditionNode));
            this.AddManipulator(AddContextualMenu("Add action node", DGNodeType.ActionNode));

            AddManipulator();
            DrawBlackboard();
            DrawNodes();
            DrawConnections();
            AddBackground();

            graphViewChanged += OnGraphViewChangedEvent;
            m_window.toolbar.onHideBlackboardBtnClick += m_blackboard.HideOrUnhideBlackboard;
        }

        #region InitialMethods
        private void DrawBlackboard()
        {
            m_blackboard = new DGBlackboard(this, m_graph);
            this.Add(m_blackboard);
        }

        private void DrawNodes()
        {
            if (m_graph.Nodes.Count == 0)
                return;

            foreach (DGNode n in m_graph.Nodes)
            {
                this.AddElement(AddNodeToGraph(n));
            }
        }

        private void DrawConnections()
        {
            if (m_graph.Connections == null) return;
            if (m_graph.Connections.Count == 0) return;

            foreach (DGConnection conn in m_graph.Connections)
            {
                DrawConnection(conn);
            }
        }

        private void DrawConnection(DGConnection conn)
        {
            DGNodeEditor inputNode = GetNode(conn.inputPort.nodeID);
            DGNodeEditor outputNode = GetNode(conn.outputPort.nodeID);

            if (inputNode == null) return;
            if (outputNode == null) return;

            Port inputPort = inputNode.InputPorts[conn.inputPort.portIndex];
            Port outputPort = outputNode.OutputPorts[conn.outputPort.portIndex];
            Edge edge = inputPort.ConnectTo(outputPort);
            m_connectionDictionary.Add(edge, conn);
            AddElement(edge);
        }

        private void AddManipulator()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        private ContextualMenuManipulator AddContextualMenu(string title, DGNodeType type)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(
                    title,
                    actionEvent => AddElement(
                        CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))
                        )
                    )
                );

            return contextualMenuManipulator;
        }

        private void AddBackground()
        {
            GridBackground grid = new GridBackground();
            grid.StretchToParentSize();
            grid.name = "GridBackground";
            Insert(0, grid);
        }

        private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                foreach (DGNodeEditor n in graphViewChange.movedElements.OfType<DGNodeEditor>())
                {
                    n.SavePosition();
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                List<DGNodeEditor> nodes = graphViewChange.elementsToRemove.OfType<DGNodeEditor>().ToList();
                if (nodes.Count > 0)
                {
                    Undo.RecordObject(m_serializedObject.targetObject, "Remove Nodes");

                    foreach (DGNodeEditor n in nodes)
                    {
                        RemoveNode(n);
                    }
                }

                List<Edge> edges = graphViewChange.elementsToRemove.OfType<Edge>().ToList();
                if (edges.Count > 0)
                {
                    Undo.RecordObject(m_serializedObject.targetObject, "Remove Edges");

                    foreach (Edge e in edges)
                    {
                        RemoveEdge(e);
                    }
                }

                List<BlackboardField> bFilds = graphViewChange.elementsToRemove.OfType<BlackboardField>().ToList();
                if (bFilds.Count > 0)
                {
                    Undo.RecordObject(m_serializedObject.targetObject, "Remove Blackboard Variable");

                    foreach (BlackboardField f in bFilds)
                    {
                        m_blackboard.RemoveBlackboardField(f);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    CreateEdge(edge);
                }
            }

            return graphViewChange;
        }
        #endregion

        #region graphViewChanged Methods
        private void RemoveNode(DGNodeEditor nodeEditor)
        {
            m_nodesEditor.Remove(nodeEditor);
            m_nodeDictionary.Remove(nodeEditor.nodeData.Guid);
            m_graph.Nodes.Remove(nodeEditor.nodeData);
            m_serializedObject.Update();
        }

        private void CreateEdge(Edge edge)
        {
            DGNodeEditor inputNode = (DGNodeEditor)edge.input.node;
            int inputIndexPort = inputNode.InputPorts.IndexOf(edge.input);

            DGNodeEditor outputNode = (DGNodeEditor)edge.output.node;
            int outputIndexPort = outputNode.OutputPorts.IndexOf(edge.output);

            DGConnection connection = new DGConnection(inputNode.nodeData.Guid, inputIndexPort, outputNode.nodeData.Guid, outputIndexPort);
            m_graph.Connections.Add(connection);
            m_connectionDictionary.Add(edge, connection);
        }

        public void RemoveEdge(Edge e)
        {
            if (m_connectionDictionary.TryGetValue(e, out DGConnection conn))
            {
                m_graph.Connections.Remove(conn);
                m_connectionDictionary.Remove(e);
            }
        }
        #endregion

        public DGNodeEditor CreateNode(DGNodeType type, Vector2 pos)
        {
            string assemblyName = typeof(DGNode).Assembly.FullName;
            Type nodeEditorType = Type.GetType($"DG_with_Localization.Elements.DG{type}, {assemblyName}");
            DGNode node = (DGNode)Activator.CreateInstance(nodeEditorType);
            node.Initialize(pos, type);

            Undo.RecordObject(m_serializedObject.targetObject, "Added Node");
            m_graph.Nodes.Add(node);
            m_serializedObject.Update();

            return AddNodeToGraph(node);
        }

        private DGNodeEditor AddNodeToGraph(DGNode node)
        {
            string assemblyName = typeof(DGNodeEditor).Assembly.FullName;
            Type nodeEditorType = Type.GetType($"DG_with_Localization.Editor.DG{node.NodeType}Editor, {assemblyName}");
            DGNodeEditor nodeEditor = (DGNodeEditor)Activator.CreateInstance(nodeEditorType);
            nodeEditor.OnChangedValue += () =>
            {
                EditorUtility.SetDirty(m_window.currentGraph);
                m_serializedObject.ApplyModifiedProperties();
            };

            nodeEditor.Initial(node, m_graph.LocalizationTable);
            nodeEditor.Draw();

            //Set this view to the node that requires
            switch (node.NodeType)
            {
                case DGNodeType.ConditionNode:
                    (nodeEditor as DGConditionNodeEditor).SetView(this);
                    break;
                case DGNodeType.ActionNode:
                    (nodeEditor as DGActionNodeEditor).SetView(this);
                    break;
                case DGNodeType.ChoiceNode:
                    (nodeEditor as DGChoiceNodeEditor).SetView(this);
                    break;
                default:
                    break;
            }

            m_nodesEditor.Add(nodeEditor);
            m_nodeDictionary.Add(nodeEditor.nodeData.Guid, nodeEditor);

            return nodeEditor;
        }

        #region Utility
        public Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            Vector2 worldMousePosition = mousePosition;
            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> allPorts = new List<Port>();
            List<Port> ports = new List<Port>();

            foreach (DGNodeEditor node in nodesEditor)
            {
                allPorts.AddRange(node.InputPorts);
                allPorts.AddRange(node.OutputPorts);
            }

            foreach (Port p in allPorts)
            {
                if (p == startPort) continue;
                if (p.node == startPort.node) continue;
                if (p.direction == startPort.direction) continue;
                if (p.portType == startPort.portType)
                {
                    ports.Add(p);
                }
            }

            return ports;
        }

        private DGNodeEditor GetNode(string nodeID) => m_nodeDictionary.TryGetValue(nodeID, out DGNodeEditor node) ? node : null;

        #endregion
    }
}
