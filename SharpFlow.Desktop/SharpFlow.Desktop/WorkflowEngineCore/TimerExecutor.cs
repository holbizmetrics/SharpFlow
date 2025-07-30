using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpFlow.Desktop.WorkflowEngineCore
{
	public class TimerExecutor : INodeExecutor
	{
		public async Task<ExecutionResult> ExecuteAsync(ObservableDictionary<string, object> properties, ExecutionInput input)
		{
			var startTime = DateTime.UtcNow;

			try
			{
				var intervalMs = Convert.ToInt32(properties.GetValueOrDefault("IntervalMs") ?? 1000);

				await Task.Delay(intervalMs);

				return new ExecutionResult
				{
					Success = true,
					OutputData = new Dictionary<string, object>
					{
						["Timestamp"] = DateTime.UtcNow,
						["Message"] = "Timer executed"
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
