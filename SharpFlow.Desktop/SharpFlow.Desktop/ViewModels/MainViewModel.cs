using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpFlow.Desktop.Models;

namespace SharpFlow.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<WorkflowNode> Nodes { get; } = new();

    [ObservableProperty]
    private WorkflowNode? _selectedNode;

    [RelayCommand]
    private void AddHttpNode()
    {
        Debug.WriteLine("🌐 BUTTON WAS CLICKED!"); // ← Just this for now

        var node = new WorkflowNode
        {
            Type = "HttpRequest",
            Name = "HTTP Request",
            X = 50 + Nodes.Count * 30,
            Y = 50 + Nodes.Count * 30,
            Properties = new ObservableDictionary<string, object>
            {
                ["url"] = "https://api.example.com",
                ["method"] = "GET"
            }
        };

        Nodes.Add(node);
        Debug.WriteLine($"Added node! Total count: {Nodes.Count}");
	}

    [RelayCommand]
    private void AddTimerNode()
    {
        var node = new WorkflowNode
        {
            Type = "Timer",
            Name = "Timer",
            X = 50 + Nodes.Count * 30,
            Y = 50 + Nodes.Count * 30,
            Properties = new ObservableDictionary<string, object>
            {
                ["interval"] = 5
            }
        };

        Nodes.Add(node);
    }

    [RelayCommand]
    private void ClearNodes()
    {
        Nodes.Clear();
    }

    public void SelectNode(WorkflowNode node)
    {
        // Clear previous selection
        foreach (var n in Nodes)
            n.IsSelected = false;

        // Select new node
        node.IsSelected = true;
        SelectedNode = node;
    }
}