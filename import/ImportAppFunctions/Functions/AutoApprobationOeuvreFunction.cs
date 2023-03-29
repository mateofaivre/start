using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Requests;
using System.Linq;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class AutoApprobationOeuvreFunction
    {
        private readonly ILogger<AutoApprobationOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;

        public AutoApprobationOeuvreFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository)
        {
            _logger = loggerFactory.CreateLogger<AutoApprobationOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
        }

        [FunctionName("AutoApprobationOeuvreFunction")]
        public async Task Run([TimerTrigger("0 30 23 * * 5")]TimerInfo myTimer)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            var oeuvres = await _oeuvreRepository.FindAllOeuvreToApprove();

            if (oeuvres.Any())
            {
                var approveProcess = new ApprobationProcess(_logger, _oeuvreRepository);
                foreach (var oeuvre in oeuvres)
                {
                    await approveProcess.Exceute(new ApproveRequest
                    {
                        Approved = true,
                        OeuvreId = oeuvre.Marqueur
                    });
                }
            }
        }
    }
}
