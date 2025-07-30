using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.WorkflowEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpFlow.Desktop
{
	public class WorkflowEngine
	{
		private readonly NodeExecutorRegistry _registry;
		private readonly Dictionary<WorkflowNode, ExecutionResult> _executionResults = new();

		// Debugging infrastructure
		public bool DebugMode { get; set; } = false;
		public HashSet<WorkflowNode> Breakpoints { get; set; } = new();

		// Debug events for UI to subscribe to
		public event Action<WorkflowNode, ExecutionInput>? OnNodeStarting;
		public event Action<WorkflowNode, ExecutionResult>? OnNodeCompleted;
		public event Action<WorkflowNode>? OnBreakpointHit;
		public event Action<WorkflowDefinition>? OnWorkflowStarting;
		public event Action<WorkflowExecutionResult>? OnWorkflowCompleted;

		// Debug control
		private TaskCompletionSource<bool>? _debugContinueSignal;
		private WorkflowNode? _currentPausedNode;

		public WorkflowEngine(NodeExecutorRegistry registry)
		{
			_registry = registry;
		}

		// Debug control methods
		public void Continue()
		{
			_debugContinueSignal?.SetResult(true);
		}

		public void Step()
		{
			_debugContinueSignal?.SetResult(false); // Continue but pause at next breakpoint
		}

		public Dictionary<WorkflowNode, ExecutionResult> GetExecutionResults() =>
			new Dictionary<WorkflowNode, ExecutionResult>(_executionResults);

		public WorkflowNode? GetCurrentPausedNode() => _currentPausedNode;

		public async Task<WorkflowExecutionResult> ExecuteWorkflowAsync(WorkflowDefinition workflow)
		{
			_executionResults.Clear();
			var startTime = DateTime.UtcNow;

			// Notify workflow starting
			OnWorkflowStarting?.Invoke(workflow);

			try
			{
				// Find nodes with no input connections (starting points)
				var startingNodes = FindStartingNodes(workflow);

				// Execute workflow using topological sort
				var executedNodes = new HashSet<WorkflowNode>();
				var nodesToExecute = new Queue<WorkflowNode>(startingNodes);

				while (nodesToExecute.Any())
				{
					var currentNode = nodesToExecute.Dequeue();

					if (executedNodes.Contains(currentNode))
						continue;

					// Check if all input dependencies are satisfied
					if (!AreInputDependenciesSatisfied(currentNode, workflow, executedNodes))
					{
						// Re-queue for later execution
						nodesToExecute.Enqueue(currentNode);
						continue;
					}

					// Prepare input data from connected nodes
					var input = PrepareNodeInput(currentNode, workflow);

					// DEBUGGING: Notify node starting and check for breakpoint
					OnNodeStarting?.Invoke(currentNode, input);

					if (DebugMode && Breakpoints.Contains(currentNode))
					{
						// Pause execution at breakpoint
						_currentPausedNode = currentNode;
						OnBreakpointHit?.Invoke(currentNode);

						// Wait for continue signal
						_debugContinueSignal = new TaskCompletionSource<bool>();
						await _debugContinueSignal.Task;

						_currentPausedNode = null;
					}

					// Execute the node
					var executor = _registry.GetExecutor(currentNode.Type);
					if (executor == null)
						throw new InvalidOperationException($"No executor found for node type: {currentNode.Type}");

					var result = await executor.ExecuteAsync(currentNode.Properties, input);
					_executionResults[currentNode] = result;
					executedNodes.Add(currentNode);

					// DEBUGGING: Notify node completed
					OnNodeCompleted?.Invoke(currentNode, result);

					// Queue downstream nodes for execution
					var downstreamNodes = GetDownstreamNodes(currentNode, workflow);
					foreach (var downstream in downstreamNodes)
					{
						if (!executedNodes.Contains(downstream) && !nodesToExecute.Contains(downstream))
						{
							nodesToExecute.Enqueue(downstream);
						}
					}

					// Stop if any node failed (optional - could continue with error handling)
					if (!result.Success)
					{
						var failureResult = new WorkflowExecutionResult
						{
							Success = false,
							ErrorMessage = $"Node {currentNode.Name} failed: {result.ErrorMessage}",
							ExecutionTime = DateTime.UtcNow - startTime,
							NodeResults = _executionResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
						};

						// Notify workflow completed
						OnWorkflowCompleted?.Invoke(failureResult);
						return failureResult;
					}
				}

				var successResult = new WorkflowExecutionResult
				{
					Success = true,
					ExecutionTime = DateTime.UtcNow - startTime,
					NodeResults = _executionResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
				};

				// Notify workflow completed
				OnWorkflowCompleted?.Invoke(successResult);
				return successResult;
			}
			catch (Exception ex)
			{
				var errorResult = new WorkflowExecutionResult
				{
					Success = false,
					ErrorMessage = ex.Message,
					ExecutionTime = DateTime.UtcNow - startTime,
					NodeResults = _executionResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
				};

				// Notify workflow completed
				OnWorkflowCompleted?.Invoke(errorResult);
				return errorResult;
			}
		}

		private List<WorkflowNode> FindStartingNodes(WorkflowDefinition workflow)
		{
			var nodesWithInputs = workflow.Connectors
				.Select(c => GetNodeByPort(c.OutputPort, workflow))
				.Where(n => n != null)
				.ToHashSet();

			return workflow.Nodes.Where(n => !nodesWithInputs.Contains(n)).ToList();
		}

		private bool AreInputDependenciesSatisfied(WorkflowNode node, WorkflowDefinition workflow, HashSet<WorkflowNode> executedNodes)
		{
			var inputConnections = workflow.Connectors
				.Where(c => GetNodeByPort(c.OutputPort, workflow) == node)
				.ToList();

			if (!inputConnections.Any())
				return true; // No dependencies

			return inputConnections.All(c =>
			{
				var sourceNode = GetNodeByPort(c.InputPort, workflow);
				return sourceNode != null && executedNodes.Contains(sourceNode);
			});
		}

		private ExecutionInput PrepareNodeInput(WorkflowNode node, WorkflowDefinition workflow)
		{
			var input = new ExecutionInput();

			var inputConnections = workflow.Connectors
				.Where(c => GetNodeByPort(c.OutputPort, workflow) == node)
				.ToList();

			foreach (var connection in inputConnections)
			{
				var sourceNode = GetNodeByPort(connection.InputPort, workflow);
				if (sourceNode != null && _executionResults.ContainsKey(sourceNode))
				{
					var sourceResult = _executionResults[sourceNode];
					// Merge source output data into input
					foreach (var kvp in sourceResult.OutputData)
					{
						input.Data[kvp.Key] = kvp.Value;
					}
				}
			}

			return input;
		}

		private List<WorkflowNode> GetDownstreamNodes(WorkflowNode node, WorkflowDefinition workflow)
		{
			return workflow.Connectors
				.Where(c => GetNodeByPort(c.InputPort, workflow) == node)
				.Select(c => GetNodeByPort(c.OutputPort, workflow))
				.Where(n => n != null)
				.ToList();
		}

		private WorkflowNode? GetNodeByPort(Port port, WorkflowDefinition workflow)
		{
			return workflow.Nodes.FirstOrDefault(n => n.Ports.Contains(port));
		}
	}

	public class WorkflowExecutionResult
	{
		public bool Success { get; set; }
		public string? ErrorMessage { get; set; }
		public TimeSpan ExecutionTime { get; set; }
		public Dictionary<WorkflowNode, ExecutionResult> NodeResults { get; set; } = new();
	}
}