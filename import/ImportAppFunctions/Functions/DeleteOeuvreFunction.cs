using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Start.Core.Process;
using Start.Core.Repositories;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    /// <summary>
    /// Delete soft with oeuvre id
    /// </summary>
    public class DeleteOeuvreFunction
    {
        private readonly ILogger<DeleteOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;

        public DeleteOeuvreFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository)
        {
            _logger = loggerFactory.CreateLogger<DeleteOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
        }

        [FunctionName("DeleteOeuvreFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequest req)
        {
            _logger.LogInformation("DeleteOeuvreFunction function processed a request.");

            var id = req.Query["oeuvreId"].ToString();
            var deleteProcess = new DeleteProcess(_logger, _oeuvreRepository);
            await deleteProcess.Execute(id);
            
            return new OkObjectResult("");
        }
    }
}
