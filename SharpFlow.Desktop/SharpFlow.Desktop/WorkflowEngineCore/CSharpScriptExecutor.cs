using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SharpFlow.Desktop.WorkflowEngineCore
{
	public class CSharpScriptExecutor : INodeExecutor
	{
		private readonly CSharpCompilation _baseCompilation;

		public CSharpScriptExecutor()
		{
			// Setup base compilation with common references
			var references = new[]
			{
			// Core runtime assemblies
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).Assembly.Location),
            
            // Add System.Runtime explicitly
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
			MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
			MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),

			};

			_baseCompilation = CSharpCompilation.Create("ScriptAssembly")
				.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
				.WithReferences(references);
		}

		public async Task<ExecutionResult> ExecuteAsync(ObservableDictionary<string, object> properties, ExecutionInput input)
		{
			var startTime = DateTime.UtcNow;

			try
			{
				var csharpCode = properties.GetValueOrDefault("Code")?.ToString() ?? "";

				// Wrap user code in a class structure
				var wrappedCode = $@"
                using System;
                using System.Collections.Generic;
                using System.Threading.Tasks;
                
			    using Newtonsoft.Json;
				using Newtonsoft.Json.Linq;

                public class UserScript
                {{
                    public static object Execute(Dictionary<string, object> input)
                    {{
                        {csharpCode}
                    }}
                }}";

				// Compile the code
				var syntaxTree = CSharpSyntaxTree.ParseText(wrappedCode);
				var compilation = _baseCompilation.AddSyntaxTrees(syntaxTree);

				using var memoryStream = new MemoryStream();
				var result = compilation.Emit(memoryStream);

				if (!result.Success)
				{
					var errors = string.Join("\n", result.Diagnostics.Select(d => d.GetMessage()));
					return new ExecutionResult
					{
						Success = false,
						ErrorMessage = $"Compilation failed: {errors}",
						ExecutionTime = DateTime.UtcNow - startTime
					};
				}

				// Execute the compiled code
				memoryStream.Seek(0, SeekOrigin.Begin);
				var assembly = Assembly.Load(memoryStream.ToArray());
				var type = assembly.GetType("UserScript");
				var method = type.GetMethod("Execute");

				var inputDictionary = input.Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
				var output = method.Invoke(null, new object[] { inputDictionary });

				return new ExecutionResult
				{
					Success = true,
					OutputData = new Dictionary<string, object> { ["result"] = output },
					ExecutionTime = DateTime.UtcNow - startTime
				};
			}
			catch (Exception ex)
			{
				return new ExecutionResult
				{
					Success = false,
					ErrorMessage = ex.Message,
					ExecutionTime = DateTime.UtcNow - startTime
				};
			}
		}
	}
}