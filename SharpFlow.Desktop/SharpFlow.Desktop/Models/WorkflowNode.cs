// ====================================================
// WorkflowNode.cs - Simple converters
// ====================================================

using Avalonia.Controls;
using Avalonia.LogicalTree;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SharpFlow.Desktop.Models;

public partial class WorkflowNode : WorkflowObject
{
	[ObservableProperty]
	private ObservableDictionary<string, object> _properties = new();

	[ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private List<Port> _ports = new();

    public WorkflowNode()
    {
        Type = GetType().Name;
        Name = GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? Type;
	    Properties.Add("Name", Name);
        Properties.Add("Type", Type);
	}

	public INameScope? FindNameScope()
    {
        return null;
    }
}