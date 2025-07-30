using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace SharpFlow.Desktop.Models
{
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
}
