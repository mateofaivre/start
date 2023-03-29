using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using Start.Core.Entities;
using Start.Core.Services;
using System.Threading.Tasks;

namespace Start.UnitTest
{
    public class AzureMapServiceTest
    {
        private readonly AzureMapService _azureMapService;
        private readonly ILogger<AzureMapServiceTest> _logger;
        public AzureMapServiceTest(ILoggerFactory loggerFactory)
        {
            IOptions<AzureMapConfig> azureMapConfigOptions = new TestAzureMapConfigOptions();
            _azureMapService = new AzureMapService(azureMapConfigOptions);

            _logger = loggerFactory.CreateLogger<AzureMapServiceTest>();
        }
        public async Task TestGetPositionAsync()
        {
            var adresse = new Adresse("rue des rotondes, Dijon")
            {
                /*FreeformAddress = "15127 NE 24th Street, Redmond, WA 98052"*/
            };

            var position = await _azureMapService.GetPositionAsync(adresse);
            _logger.LogInformation($"lat:{position.Position.Latitude}, lon:{position.Position.Longitude}");
        }

        public async Task TestGetAdresseAsync()
        {
            //var location = new Point(-122.1385, 47.6308);
            var location = new Point(5.03586, 47.30346);
            var adresse = await _azureMapService.GetAddressAsync(location);
            _logger.LogInformation($"adresse:{adresse}  freeAdresse:{adresse.FreeformAddress}");
        }
    }
}
