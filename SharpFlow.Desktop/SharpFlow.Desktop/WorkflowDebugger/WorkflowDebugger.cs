using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.WorkflowEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlow.Desktop.WorkflowDebugger
{
	public class WorkflowDebugger
{
    private readonly WorkflowEngine _engine;
    private TaskCompletionSource<bool>? _continueSignal;
    private WorkflowNode? _currentPausedNode;
    
    public bool DebugMode { get; set; } = false;
    public HashSet<WorkflowNode> Breakpoints { get; set; } = new();
    
    // Debugger events
    public event Action<WorkflowNode>? OnBreakpointHit;
    public event Action<WorkflowNode, ExecutionInput>? OnNodeDebugging;
    public event Action<WorkflowNode, ExecutionResult>? OnNodeInspection;
    
    public WorkflowDebugger(WorkflowEngine engine)
    {
        _engine = engine;
        
        // Subscribe to engine events
        _engine.OnNodeStarting += HandleNodeStarting;
        _engine.OnNodeCompleted += HandleNodeCompleted;
    }
    
    private async void HandleNodeStarting(WorkflowNode node, ExecutionInput input)
    {
        OnNodeDebugging?.Invoke(node, input);
        
        if (DebugMode && Breakpoints.Contains(node))
        {
            _currentPausedNode = node;
            OnBreakpointHit?.Invoke(node);
            
            // Pause execution - wait for continue signal
            _continueSignal = new TaskCompletionSource<bool>();
            await _continueSignal.Task;
            
            _currentPausedNode = null;
        }
    }
    
    private void HandleNodeCompleted(WorkflowNode node, ExecutionResult result)
    {
        OnNodeInspection?.Invoke(node, result);
    }
    
    public void Continue()
    {
        _continueSignal?.SetResult(true);
    }
    
    public void Step()
    {
        _continueSignal?.SetResult(true);
        // Could add step-specific logic here
    }
    
    public WorkflowNode? GetCurrentPausedNode() => _currentPausedNode;
}
}
