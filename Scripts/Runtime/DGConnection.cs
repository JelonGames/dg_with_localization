namespace DG_with_Localization
{
    [System.Serializable]
    public class DGConnection
    {
        public DGConnectionPort inputPort;
        public DGConnectionPort outputPort;

        public DGConnection(DGConnectionPort input, DGConnectionPort output)
        {
            inputPort = input;
            outputPort = output;
        }

        public DGConnection(string inputNodeID, int inputPortId, string outputNodeId, int outputPortId)
        {
            inputPort = new DGConnectionPort(inputNodeID, inputPortId);
            outputPort = new DGConnectionPort(outputNodeId, outputPortId);
        }
    }

    [System.Serializable]
    public struct DGConnectionPort
    {
        public string nodeID;
        public int portIndex;

        public DGConnectionPort(string nodeID, int portIndex)
        {
            this.nodeID = nodeID;
            this.portIndex = portIndex;
        }
    }
}
