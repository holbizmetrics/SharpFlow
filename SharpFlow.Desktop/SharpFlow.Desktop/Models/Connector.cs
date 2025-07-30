using System;

namespace SharpFlow.Desktop.Models
{
	/// <summary>
	/// Basically the arrows to connect nodes from one port to another.
	/// </summary>
	public partial class Connector
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public Port InputPort { get; set; }
		public Port OutputPort { get; set; }
		public Connector(Port input, Port output)
		{
			InputPort = input;
			OutputPort = output;
		}
	}
}
