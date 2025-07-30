using SharpFlow.Desktop.Models;
using System.Collections.Generic;

namespace SharpFlow.Desktop.WorkflowEngineCore
{
	public enum PortDirection
	{
		Input,
		Output
	}

	/// <summary>
	/// Workflow Engine can iterate through WorkflowDefinition to understand the graph.
	/// </summary>
	public class WorkflowDefinition
	{
		public List<WorkflowNode> Nodes { get; set; } = new();
		public List<Connector> Connectors { get; set; } = new();
	}
}
