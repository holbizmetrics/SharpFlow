using CommunityToolkit.Mvvm.Input;
using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.WorkflowEngineCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows.Input;

namespace SharpFlow.Desktop.ViewModels
{
	public class WorkflowCanvasViewModel : ViewModelBase
	{
		private readonly WorkflowEngine _engine;
		private readonly NodeExecutorRegistry _registry;

		public ObservableCollection<WorkflowNodeViewModel> Nodes { get; set; } = new();
		public ObservableCollection<ConnectorViewModel> Connectors { get; set; } = new();

		// Debug state
		public bool IsDebugging { get; set; }
		public bool IsPaused { get; set; }
		public bool HasBreakpoints => Nodes.Any(n => n.HasBreakpoint);
		public int BreakpointCount => Nodes.Count(n => n.HasBreakpoint);
		public bool ExecutionStatus { get; set; } = true; // True means running, false means stopped
		public WorkflowNodeViewModel? CurrentPausedNode { get; set; }
		public TimeSpan ExecutionTime { get; set; } = TimeSpan.Zero;
		// Commands
		public ICommand StartDebuggingCommand { get; }
		public ICommand StopDebuggingCommand { get; }
		public ICommand ContinueCommand { get; }
		public ICommand StepCommand { get; }
		public ICommand ToggleBreakpointCommand { get; }

		public WorkflowCanvasViewModel()
		{
			// Setup engine with debugging
			_registry = new NodeExecutorRegistry();
			_registry.RegisterExecutor("Timer", new TimerExecutor());
			_registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(new HttpClient()));
			_registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			_engine = new WorkflowEngine(_registry);
			_engine.DebugMode = true;

			// Subscribe to debug events
			_engine.OnNodeStarting += OnNodeStarting;
			_engine.OnNodeCompleted += OnNodeCompleted;
			_engine.OnBreakpointHit += OnBreakpointHit;

			// Commands
			//StartDebuggingCommand = new RelayCommand(async () => await StartDebugging());
			ContinueCommand = new RelayCommand(() => _engine.Continue());
			StepCommand = new RelayCommand(() => _engine.Step());
			//ToggleBreakpointCommand = new RelayCommand<WorkflowNodeViewModel>(ToggleBreakpoint);
		}

		private void OnNodeStarting(WorkflowNode node, ExecutionInput input)
		{
			var nodeVM = Nodes.FirstOrDefault(n => n.Node == node);
			if (nodeVM != null)
			{
				nodeVM.IsExecuting = true;
				nodeVM.ExecutionData = input.Data;
			}
		}

		private void OnNodeCompleted(WorkflowNode node, ExecutionResult result)
		{
			var nodeVM = Nodes.FirstOrDefault(n => n.Node == node);
			if (nodeVM != null)
			{
				nodeVM.IsExecuting = false;
				nodeVM.IsCompleted = true;
				nodeVM.ExecutionResult = result;
			}
		}

		private void OnBreakpointHit(WorkflowNode node)
		{
			var nodeVM = Nodes.FirstOrDefault(n => n.Node == node);
			if (nodeVM != null)
			{
				CurrentPausedNode = nodeVM;
				IsPaused = true;
			}
		}
	}

	public class WorkflowNodeViewModel : ViewModelBase
	{
		public WorkflowNode Node { get; set; }
		public bool HasBreakpoint { get; set; }
		public bool IsExecuting { get; set; }
		public bool IsCompleted { get; set; }
		public Dictionary<string, object>? ExecutionData { get; set; }
		public ExecutionResult? ExecutionResult { get; set; }
		public bool IsPaused { get; set; }
		public string DataPreview => ExecutionData != null && ExecutionData.Any()
			? string.Join(", ", ExecutionData.Take(2).Select(kvp => $"{kvp.Key}: {kvp.Value?.ToString()?.Substring(0, Math.Min(20, kvp.Value?.ToString()?.Length ?? 0))}"))
			: "No data";

		public bool HasFailed => ExecutionResult?.Success == false;
		public TimeSpan ExecutionTime { get; set; } = TimeSpan.Zero;
		// Visual properties
		public double X { get; set; }
		public double Y { get; set; }
		public string DisplayName => Node.Name;
		public string NodeType => Node.Type;
	}
}
