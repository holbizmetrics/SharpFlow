using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SharpFlow.Desktop.Models;

public abstract partial class WorkflowObject : ObservableObject
{
    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Type { get; set; } = "";
	public string Name { get; set; } = "";
}

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
}

/// <summary>
/// If a node has no ports, then it couldn't connect.
/// </summary>
public partial class Port : WorkflowObject
{

}

/// <summary>
/// Basically the arrows to connect nodes from one port to another.
/// </summary>
public partial class Connector
{ 
}