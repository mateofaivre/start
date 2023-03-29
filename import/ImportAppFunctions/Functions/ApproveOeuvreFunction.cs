using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Requests;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    /// <summary>
    /// Approve or not the last updating values or the new values of feature with this id and the approve status parameters
    /// </summary>
    public class ApproveOeuvreFunction
    {
        private readonly ILogger<ApproveOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;

        public ApproveOeuvreFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository)
        {
            _logger = loggerFactory.CreateLogger<ApproveOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
        }

        [FunctionName("ApproveOeuvreFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] ApproveRequest approveRequest)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var approveProcess = new ApprobationProcess(_logger, _oeuvreRepository);
            var oeuvre = await approveProcess.Exceute(approveRequest);

            return new OkObjectResult(oeuvre);
        }
    }
}
