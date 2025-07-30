using NUnit.Framework;
using SharpFlow.Desktop;
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

		[Test]
		public async Task TestTwoConnectedNodes()
		{
			// Setup registry
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));
			registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			// Create nodes
			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");
			var scriptNode = TestNodeFactory.CreateCSharpScriptNode(
@"// Extract the name from GitHub API response
var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(input[""Body""]);
return $""Hello, {json.name}!"";
");

			// Create connection
			var httpOutputPort = httpNode.Ports.First(p => p.Direction == PortDirection.Output && p.Name == "response");
			var scriptInputPort = scriptNode.Ports.First(p => p.Direction == PortDirection.Input);

			var connector = new Connector(httpOutputPort, scriptInputPort);

			// Create workflow definition
			var workflow = new WorkflowDefinition();
			workflow.Nodes.Add(httpNode);
			workflow.Nodes.Add(scriptNode);
			workflow.Connectors.Add(connector);

			// Execute manually (simulating workflow engine)

			// 1. Execute HTTP node first
			var httpExecutor = registry.GetExecutor(httpNode.Type);
			var httpResult = await httpExecutor.ExecuteAsync(httpNode.Properties, new ExecutionInput());

			Assert.That(httpResult.Success, Is.True, "HTTP request should succeed");

			// 2. Pass HTTP result to script node
			var scriptInput = new ExecutionInput();
			scriptInput.Data = httpResult.OutputData; // Pass the HTTP response

			var scriptExecutor = registry.GetExecutor(scriptNode.Type);
			var scriptResult = await scriptExecutor.ExecuteAsync(scriptNode.Properties, scriptInput);

			Assert.That(scriptResult.Success, Is.True, "Script should succeed");

			// Verify the data flowed correctly
			Console.WriteLine($"HTTP Status: {httpResult.OutputData["StatusCode"]}");
			Console.WriteLine($"Script Result: {scriptResult.OutputData["result"]}");

			// Should contain "Hello, The Octocat!" or similar
			Assert.That(scriptResult.OutputData["result"].ToString(), Does.Contain("Hello"));
		}

		[Test]
		public async Task TestThreeConnectedNodes()
		{
			// Setup
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("Timer", new TimerExecutor());
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));
			registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			// Create workflow: Timer → HTTP → Script
			var timerNode = TestNodeFactory.CreateTimerNode(100); // Fast timer for testing
			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");
			var scriptNode = TestNodeFactory.CreateCSharpScriptNode(@"
        var json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(input[""Body""]);
        return $""User {json.login} has {json.public_repos} public repos"";
    ");

			// Create connections
			var timerToHttp = new Connector(
				timerNode.Ports.First(p => p.Direction == PortDirection.Output),
				httpNode.Ports.First(p => p.Direction == PortDirection.Input)
			);

			var httpToScript = new Connector(
				httpNode.Ports.First(p => p.Direction == PortDirection.Output && p.Name == "response"),
				scriptNode.Ports.First(p => p.Direction == PortDirection.Input)
			);

			// Execute workflow chain manually

			// 1. Timer
			var timerResult = await registry.GetExecutor("Timer").ExecuteAsync(timerNode.Properties, new ExecutionInput());
			Assert.That(timerResult.Success, Is.True);

			// 2. HTTP (triggered by timer)
			var httpInput = new ExecutionInput { Data = timerResult.OutputData };
			var httpResult = await registry.GetExecutor("HttpRequest").ExecuteAsync(httpNode.Properties, httpInput);
			Assert.That(httpResult.Success, Is.True);

			// 3. Script (processes HTTP response)
			var scriptInput = new ExecutionInput { Data = httpResult.OutputData };
			var scriptResult = await registry.GetExecutor("CSharpScript").ExecuteAsync(scriptNode.Properties, scriptInput);
			Assert.That(scriptResult.Success, Is.True);

			Console.WriteLine($"Final result: {scriptResult.OutputData["result"]}");
			Assert.That(scriptResult.OutputData["result"].ToString(), Does.Contain("repos"));
		}

		[Test]
		public async Task TestRealWorkflowExecution()
		{
			// Setup
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("Timer", new TimerExecutor());
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));
			registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			var engine = new WorkflowEngine(registry);

			// Create workflow nodes
			var timerNode = TestNodeFactory.CreateTimerNode(100);
			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");

			// Fix the C# script with proper type casting
			var scriptNode = TestNodeFactory.CreateCSharpScriptNode(@"
			     // Cast to string first
			     var bodyString = input[""Body""].ToString();

			     // Parse JSON using Newtonsoft.Json
			     var json = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(bodyString);
			     var name = json[""name""].ToString();
			     var login = json[""login""].ToString();
			     var repos = json[""public_repos""].ToObject<int>();

			     return $""GitHub user: {name} ({login}) has {repos} repos"";
			 ");

			// Build workflow
			var workflow = new WorkflowDefinition();
			workflow.Nodes.AddRange(new[] { timerNode, httpNode, scriptNode });

			// Create connections
			workflow.Connectors.Add(new Connector(
				timerNode.Ports.First(p => p.Direction == PortDirection.Output),
				httpNode.Ports.First(p => p.Direction == PortDirection.Input)
			));

			workflow.Connectors.Add(new Connector(
				httpNode.Ports.First(p => p.Direction == PortDirection.Output && p.Name == "response"),
				scriptNode.Ports.First(p => p.Direction == PortDirection.Input)
			));

			// Execute the workflow
			var result = await engine.ExecuteWorkflowAsync(workflow);

			// Verify results
			Assert.That(result.Success, Is.True, $"Workflow should succeed. Error: {result.ErrorMessage}");
			Assert.That(result.NodeResults.Count, Is.EqualTo(3), "All three nodes should execute");

			// Get final result
			var scriptResult = result.NodeResults[scriptNode];
			var finalOutput = scriptResult.OutputData["result"].ToString();

			Console.WriteLine($"Workflow completed in {result.ExecutionTime.TotalMilliseconds}ms");
			Console.WriteLine($"Final result: {finalOutput}");

			Assert.That(finalOutput, Does.Contain("GitHub user"));
			Assert.That(finalOutput, Does.Contain("repos"));
		}



		[Test]
		public async Task DebugHttpNodeOutput()
		{
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));

			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");
			var executor = registry.GetExecutor("HttpRequest");
			var result = await executor.ExecuteAsync(httpNode.Properties, new ExecutionInput());

			Console.WriteLine($"HTTP Success: {result.Success}");
			Console.WriteLine($"HTTP Error: {result.ErrorMessage}");
			Console.WriteLine($"Output keys: {string.Join(", ", result.OutputData.Keys)}");

			foreach (var kvp in result.OutputData)
			{
				Console.WriteLine($"{kvp.Key}: {kvp.Value?.GetType().Name} = '{kvp.Value}'");
			}
		}

		[Test]
		public async Task TestWorkflowDebugging()
		{
			// Setup
			var httpClient = new HttpClient();
			var registry = new NodeExecutorRegistry();
			registry.RegisterExecutor("Timer", new TimerExecutor());
			registry.RegisterExecutor("HttpRequest", new HttpRequestExecutor(httpClient));
			registry.RegisterExecutor("CSharpScript", new CSharpScriptExecutor());

			var engine = new WorkflowEngine(registry);

			// Enable debug mode and set breakpoints
			engine.DebugMode = true;

			var timerNode = TestNodeFactory.CreateTimerNode(100);
			var httpNode = TestNodeFactory.CreateHttpRequestNode("https://api.github.com/users/octocat");
			var scriptNode = TestNodeFactory.CreateCSharpScriptNode(@"
        var bodyString = input[""Body""].ToString();
        var json = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(bodyString);
        return $""GitHub user: {json[""name""]} has {json[""public_repos""]} repos"";
    ");

			// Set breakpoint on HTTP node
			engine.Breakpoints.Add(httpNode);

			// Create workflow
			var workflow = new WorkflowDefinition();
			workflow.Nodes.AddRange(new[] { timerNode, httpNode, scriptNode });
			workflow.Connectors.Add(new Connector(
				timerNode.Ports.First(p => p.Direction == PortDirection.Output),
				httpNode.Ports.First(p => p.Direction == PortDirection.Input)
			));
			workflow.Connectors.Add(new Connector(
				httpNode.Ports.First(p => p.Direction == PortDirection.Output && p.Name == "response"),
				scriptNode.Ports.First(p => p.Direction == PortDirection.Input)
			));

			// Subscribe to debug events
			engine.OnNodeStarting += (node, input) => Console.WriteLine($"🟡 Starting: {node.Name}");
			engine.OnBreakpointHit += (node) => {
				Console.WriteLine($"🔴 BREAKPOINT HIT: {node.Name}");
				// Auto-continue after 1 second (simulating user clicking continue)
				Task.Delay(1000).ContinueWith(_ => engine.Continue());
			};
			engine.OnNodeCompleted += (node, result) => Console.WriteLine($"✅ Completed: {node.Name} (Success: {result.Success})");

			// Execute with debugging
			Console.WriteLine("🚀 Starting workflow with debugging...");
			var result = await engine.ExecuteWorkflowAsync(workflow);

			Assert.That(result.Success, Is.True);
			Console.WriteLine($"🎉 Workflow debugging test completed!");
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

				// Add ports with proper IDs
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Input,
					Name = "trigger",
					Type = "HttpRequest"
				});
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Output,
					Name = "response",
					Type = "HttpRequest"
				});
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Output,
					Name = "error",
					Type = "HttpRequest"
				});

				return node;
			}

			public static WorkflowNode CreateCSharpScriptNode(string code)
			{
				var node = new WorkflowNode();

				node.Properties["Code"] = code;
				node.Type = "CSharpScript";
				node.Name = "C# Script";

				// Add ports
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Input,
					Name = "input",
					Type = "CSharpScript"
				});
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Output,
					Name = "output",
					Type = "CSharpScript"
				});

				return node;
			}

			public static WorkflowNode CreateTimerNode(int intervalMs = 1000)
			{
				var node = new WorkflowNode();

				node.Properties["IntervalMs"] = intervalMs;
				node.Type = "Timer";
				node.Name = "Timer";

				// Timer only has output (no input needed)
				node.Ports.Add(new Port
				{
					Id = Guid.NewGuid().ToString(),
					Direction = PortDirection.Output,
					Name = "tick",
					Type = "Timer"
				});

				return node;
			}
		}
	}
}
