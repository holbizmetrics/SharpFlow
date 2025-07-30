using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharpFlow.Desktop.WorkflowEngineCore
{
	public class HttpRequestExecutor : INodeExecutor
	{
		private readonly HttpClient _httpClient;

		public HttpRequestExecutor(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<ExecutionResult> ExecuteAsync(ObservableDictionary<string, object> properties, ExecutionInput input)
		{
			var startTime = DateTime.UtcNow;

			try
			{
				// Extract configuration from properties
				var url = properties.GetValueOrDefault("Url")?.ToString() ?? "";
				var method = properties.GetValueOrDefault("Method")?.ToString() ?? "GET";
				var headers = properties.GetValueOrDefault("Headers") as Dictionary<string, string> ?? new();
				var body = properties.GetValueOrDefault("Body")?.ToString();

				// Build HTTP request
				var request = new HttpRequestMessage(new HttpMethod(method), url);

				// Add headers
				foreach (var header in headers)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}

				// Add body for POST/PUT
				if (!string.IsNullOrEmpty(body) && (method == "POST" || method == "PUT"))
				{
					request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
				}

				// Execute request
				var response = await _httpClient.SendAsync(request);
				var responseBody = await response.Content.ReadAsStringAsync();

				return new ExecutionResult
				{
					Success = true,
					OutputData = new Dictionary<string, object>
					{
						["StatusCode"] = (int)response.StatusCode,
						["Body"] = responseBody,
						["Headers"] = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
					},
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
