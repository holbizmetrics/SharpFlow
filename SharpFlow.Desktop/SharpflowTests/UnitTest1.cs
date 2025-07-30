using NUnit.Framework;
using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.WorkflowEngineCore;

namespace SharpflowTests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public async Task TestHttpRequest()
		{
			// Setup
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));

			// Create test node
			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");

			// Execute manually
			var executor = registry.GetExecutor("HttpRequest");
			var result = await executor.ExecuteAsync(httpNode.Properties, new ExecutionInput());


			Assert.That(result.Success,Is.True, "HTTP request should succeed");
			// Check results
			Console.WriteLine($"Success: {result.Success}");
			Console.WriteLine($"Status Code: {result.OutputData["StatusCode"]}");
			Console.WriteLine($"Body: {result.OutputData["Body"]}");
		}

		[Test]
		public async Task TestSingleNodeExecution()
		{
			// 1. Setup the executor registry
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));

			// 2. Create a test node manually
			var httpNode = new WorkflowNode();
			httpNode.Type = "HttpRequest";
			httpNode.Name = "Test HTTP Node";
			httpNode.Properties["Url"] = "https://api.github.com/users/octocat";
			httpNode.Properties["Method"] = "GET";

			// Add ports (even though we're not using connections yet)
			httpNode.Ports.Add(new Port { Direction = PortDirection.Input, Name = "trigger" });
			httpNode.Ports.Add(new Port { Direction = PortDirection.Output, Name = "response" });

			// 3. Execute the node directly
			var executor = registry.GetExecutor(httpNode.Type);
			Assert.That(executor, Is.Not.Null, "Should find registered executor");

			var input = new ExecutionInput(); // Empty input for now
			var result = await executor.ExecuteAsync(httpNode.Properties, input);

			// 4. Verify results
			Assert.That(result.Success, Is.True, "HTTP request should succeed");
			Assert.That(result.OutputData.ContainsKey("StatusCode"), Is.True);
			Assert.That(result.OutputData.ContainsKey("Body"), Is.True);

			Console.WriteLine($"Success: {result.Success}");
			Console.WriteLine($"Status Code: {result.OutputData["StatusCode"]}");
			Console.WriteLine($"Execution Time: {result.ExecutionTime.TotalMilliseconds}ms");
			Console.WriteLine($"Body Preview: {result.OutputData["Body"].ToString().Substring(0, Math.Min(200, result.OutputData["Body"].ToString().Length))}...");
		}

		[Test]
		public async Task TestCSharpScriptNode()
		{
			// Setup
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			// Create script node
			var scriptNode = new WorkflowNode();
			scriptNode.Type = "CSharpScript";
			scriptNode.Name = "Math Script";
			scriptNode.Properties["Code"] = @"
        var x = input.ContainsKey(""number"") ? Convert.ToInt32(input[""number""]) : 10;
        return x * x; // Square the number
    ";

			// Execute with input data
			var input = new ExecutionInput();
			input.Data["number"] = 7;

			var executor = registry.GetExecutor(scriptNode.Type);
			var result = await executor.ExecuteAsync(scriptNode.Properties, input);

			// Verify
			Assert.That(result.Success, Is.True);
			Assert.That(result.OutputData["result"], Is.EqualTo(49));

			Console.WriteLine($"Script result: {result.OutputData["result"]}");
		}

		[Test]
		public async Task TestTimerNode()
		{
			// Setup
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("Timer", new TimerExecutor());

			// Create timer node
			var timerNode = new WorkflowNode();
			timerNode.Type = "Timer";
			timerNode.Name = "Test Timer";
			timerNode.Properties["IntervalMs"] = 500; // 500ms delay

			// Add ports
			timerNode.Ports.Add(new Port { Direction = PortDirection.Output, Name = "tick" });

			// Execute and measure time
			var startTime = DateTime.UtcNow;

			var executor = registry.GetExecutor(timerNode.Type);
			Assert.That(executor, Is.Not.Null, "Should find timer executor");

			var input = new ExecutionInput(); // Empty input - timers don't need input
			var result = await executor.ExecuteAsync(timerNode.Properties, input);

			var actualDelay = DateTime.UtcNow - startTime;

			// Verify results
			Assert.That(result.Success, Is.True, "Timer should execute successfully");
			Assert.That(result.OutputData.ContainsKey("Timestamp"), Is.True);
			Assert.That(result.OutputData.ContainsKey("Message"), Is.True);
			Assert.That(actualDelay.TotalMilliseconds, Is.GreaterThan(450), "Should wait at least 450ms");
			Assert.That(actualDelay.TotalMilliseconds, Is.LessThan(600), "Should not wait more than 600ms");

			Console.WriteLine($"Success: {result.Success}");
			Console.WriteLine($"Message: {result.OutputData["Message"]}");
			Console.WriteLine($"Timestamp: {result.OutputData["Timestamp"]}");
			Console.WriteLine($"Actual delay: {actualDelay.TotalMilliseconds}ms");
			Console.WriteLine($"Execution time: {result.ExecutionTime.TotalMilliseconds}ms");
		}

		[TearDown]
		public void TearDown()
		{

		}

		public static class TestNodeFactory
		{
			public static WorkflowNode CreateHttpRequestNode(string url, string method = "GET")
			{
				var node = new WorkflowNode();

				// Configure the node
				node.Properties["Url"] = url;
				node.Properties["Method"] = method;
				node.Properties["Headers"] = new Dictionary<string, string>();
				node.Type = "HttpRequest";
				node.Name = "HTTP Request";

				// Add ports
				node.Ports.Add(new Port { Direction = PortDirection.Input, Name = "trigger" });
				node.Ports.Add(new Port { Direction = PortDirection.Output, Name = "response" });
				node.Ports.Add(new Port { Direction = PortDirection.Output, Name = "error" });

				return node;
			}
		}
	}
}
