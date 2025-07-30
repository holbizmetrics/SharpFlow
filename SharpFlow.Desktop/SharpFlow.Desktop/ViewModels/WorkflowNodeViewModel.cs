//using CommunityToolkit.Mvvm.ComponentModel;
//using SharpFlow.Desktop.Models;
//using SharpFlow.Desktop.WorkflowEngineCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace SharpFlow.Desktop.ViewModels
//{
//	public partial class WorkflowNodeViewModel : ObservableObject
//	{
//		public WorkflowNode Node { get; set; }

//		// Debug/execution state
//		[ObservableProperty] private bool _hasBreakpoint;
//		[ObservableProperty] private bool _isExecuting;
//		[ObservableProperty] private bool _isCompleted;
//		[ObservableProperty] private bool _hasFailed;
//		[ObservableProperty] private bool _isPaused;
//		[ObservableProperty] private double _executionTime;
//		[ObservableProperty] private string _dataPreview = "";

//		// Visual positioning
//		[ObservableProperty] private double _x;
//		[ObservableProperty] private double _y;

//		// Data from execution
//		public Dictionary<string, object>? ExecutionData { get; set; }
//		public ExecutionResult? ExecutionResult { get; set; }

//		// Properties for UI binding
//		public string DisplayName => Node.Name;
//		public string NodeType => Node.Type;
//		public bool HasExecutionData => !string.IsNullOrEmpty(DataPreview);
//		public bool HasInputPorts => Node.Ports.Any(p => p.Direction == PortDirection.Input);
//		public bool HasOutputPorts => Node.Ports.Any(p => p.Direction == PortDirection.Output);

//		public WorkflowNodeViewModel(WorkflowNode node)
//		{
//			Node = node;
//		}

//		public void ResetExecutionState()
//		{
//			IsExecuting = false;
//			IsCompleted = false;
//			HasFailed = false;
//			IsPaused = false;
//			ExecutionTime = 0;
//			DataPreview = "";
//			ExecutionData = null;
//			ExecutionResult = null;
//		}

//		public void UpdateDataPreview()
//		{
//			if (ExecutionData?.Any() == true)
//			{
//				var preview = string.Join(", ", ExecutionData.Take(2).Select(kvp =>
//					$"{kvp.Key}: {kvp.Value?.ToString()?.Substring(0, Math.Min(20, kvp.Value?.ToString()?.Length ?? 0))}"));
//				DataPreview = preview;
//			}
//			else if (ExecutionResult?.OutputData?.Any() == true)
//			{
//				var preview = string.Join(", ", ExecutionResult.OutputData.Take(2).Select(kvp =>
//					$"{kvp.Key}: {kvp.Value?.ToString()?.Substring(0, Math.Min(20, kvp.Value?.ToString()?.Length ?? 0))}"));
//				DataPreview = preview;
//			}
//		}
//	}
//}
