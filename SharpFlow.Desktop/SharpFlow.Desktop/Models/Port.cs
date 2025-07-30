using SharpFlow.Desktop.WorkflowEngineCore;
using System.Drawing;

namespace SharpFlow.Desktop.Models
{
	/// <summary>
	/// If a node has no ports, then it couldn't connect.
	/// </summary>
	public partial class Port : WorkflowObject
	{
		public PortDirection Direction { get; set; } // Input/Output
		public Point RelativePosition { get; set; } // Where on the node it appears
	}
}
