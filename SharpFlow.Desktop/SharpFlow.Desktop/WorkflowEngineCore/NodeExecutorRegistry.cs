using SharpFlow.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpFlow.Desktop.WorkflowEngineCore
{
	// Registry to map node types to executors
	public class NodeExecutorRegistry
	{
		private readonly Dictionary<string, INodeExecutor> _executors = new();

		public void RegisterExecutor(string nodeType, INodeExecutor executor)
		{
			_executors[nodeType] = executor;
		}

		public INodeExecutor? GetExecutor(string nodeType) => _executors.GetValueOrDefault(nodeType);
	}

	// The EXECUTOR creates the result
	public interface INodeExecutor
	{
		Task<ExecutionResult> ExecuteAsync(ObservableDictionary<string, object> properties, ExecutionInput input);
	}

	public class ExecutionInput
	{
		public Dictionary<string, object> Data { get; set; } = new();
		public Port? SourcePort { get; set; } // Which port triggered this execution
	}


	// Results come from executors, not nodes
	public class ExecutionResult
	{
		public Dictionary<string, object> OutputData { get; set; }
		public bool Success { get; set; }
		public string? ErrorMessage { get; set; }
		public TimeSpan ExecutionTime { get; set; }
	}
}
