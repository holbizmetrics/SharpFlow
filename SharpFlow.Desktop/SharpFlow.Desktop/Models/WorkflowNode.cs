using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SharpFlow.Desktop.Models;

public partial class WorkflowNode : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    public Dictionary<string, object> Properties { get; set; } = new();

    [ObservableProperty]
    private bool _isSelected;
}