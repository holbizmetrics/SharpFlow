using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlow.Desktop
{
	public class WorkflowEngine
	{
		private Dictionary<WorkflowNode, Task<NodeResult>> _executionMap;

		// Debugging is just observing what's already happening
		public bool DebugMode { get; set; }
		private List<ExecutionStep> _executionHistory = new();

		public async Task ExecuteWorkflow()
		{
			foreach (var (node, task) in _executionMap)
			{
				if (DebugMode && HasBreakpoint(node))
				{
					await PauseExecution(); // Just pause the engine
				}

				// Execute normally
				var result = await task;

				if (DebugMode)
				{
					_executionHistory.Add(new ExecutionStep(node, result, DateTime.Now));
				}
			}
		}
	}
}
