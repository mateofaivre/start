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
    /// Permet de remettre en ligne une oeuvre supprimée
    /// </summary>
    public class UnDeleteOeuvreFunction
    {
        private readonly ILogger<UnDeleteOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;

        public UnDeleteOeuvreFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository)
        {
            _logger = loggerFactory.CreateLogger<UnDeleteOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
        }


        [FunctionName("UnDeleteOeuvreFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] Requests.UnDeleteRequest unDeleteRequest)
        {
            var id = unDeleteRequest.OeuvreId;
            var unDeleteProcess = new UnDeleteProcess(_logger, _oeuvreRepository);
            var currentOeuvre = await unDeleteProcess.Execute(id);           
            
            return new OkObjectResult(currentOeuvre);
        }
    }
}
