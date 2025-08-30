using UnityEditor.Experimental.GraphView;

namespace DG_with_Localization.Editor
{
    public class DGStartNodeEditor : DGNodeEditor
    {
        public override void Draw()
        {
            base.Draw();

            // Output Port
            Port output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            output.portName = "Output";
            OutputPorts.Add(output);
            outputContainer.Add(output);
        }
    }
}
