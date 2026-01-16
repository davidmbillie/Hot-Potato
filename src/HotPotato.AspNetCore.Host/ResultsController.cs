using HotPotato.OpenApi.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HotPotato.AspNetCore.Host
{
	public class ResultsController : Controller
	{
		private readonly IResultCollector _resultCollector;
		private readonly ILogger _logger;

		public ResultsController(IResultCollector rc, ILogger<ResultsController> logger)
		{
			_resultCollector = rc;
			_logger = logger;
		}

		[HttpGet]
		[Route("/results")]
		public IActionResult Get()
		{
			_logger.LogDebug("Getting Results...");

			HttpContext.Response.Headers.Append("X-Status", _resultCollector.OverallResult.ToString());

			return Ok(_resultCollector.Results);
		}

		[HttpDelete]
		[Route("/results")]
		public IActionResult ClearResults()
		{
			_resultCollector.Results.Clear();

			return Ok(_resultCollector.Results);
		}
	}
}
