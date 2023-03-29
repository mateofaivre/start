using Microsoft.Extensions.Options;
using Start.Core.Configurations;

namespace Start.UnitTest
{
    public class TestCosmosDbConfigOptions : IOptions<CosmosDbConfig>
    {
        public CosmosDbConfig Value { get; private set; }

        public TestCosmosDbConfigOptions()
        {
            //Value = new CosmosDbConfig
            //{
            //    DatabaseId = "startDB-Dev",
            //    EndpointUrl = "https://start-azure-cosmos-db.documents.azure.com:443/",
            //    PrimaryKey = "4zONb9mLKqNIcBps96bFPqqRA47TeVRdcPJH9alZ9SXdFWflMgruDvrTjkvFBa68ZcmTi7Mytrlh6gOJcIxiFQ=="
            //};

            Value = new CosmosDbConfig
            {
                DatabaseId = "start-prod",
                EndpointUrl = "https://startazurecosmosdb.documents.azure.com:443/",
                PrimaryKey = "pe6K4XuM1Uikrre0lFetdikyQ4aqf2gHt8Jd7THFoJZn6u9NcijP7YQgdkzbjJOTONyvV3RDGO1f2lZcl8XwxQ=="
            };
        }
    }

    public class TestAzureStorageConfigOptions : IOptions<AzureStorageConfig>
    {
        public AzureStorageConfig Value { get; private set; }

        public TestAzureStorageConfigOptions()
        {
            Value = new AzureStorageConfig
            {
                //AzureWebJobsStorage = "DefaultEndpointsProtocol=https;AccountName=startstorageaccount;AccountKey=a5o2dFTz51ufyv6RgL9xb9hI9deekEq/fiNhq1Xb5cKoiVBIQBx04B/zIuLZpfcNrTz/Lps2iDsjBkk0vL4rkw==;EndpointSuffix=core.windows.net"
                AzureWebJobsStorage = "DefaultEndpointsProtocol=https;AccountName=startressourcestorage;AccountKey=e6q5LWNb0zu7VB69rZlrhDzLumdy/9y9BnughB72vEfEDRM07EiK+MgOy8VRFlyossqCClIN4F2U+AStsakttA==;EndpointSuffix=core.windows.net"
            };
        }
    }

    public class TestAzureMapConfigOptions : IOptions<AzureMapConfig>
    {
        public AzureMapConfig Value { get; private set; }

        public TestAzureMapConfigOptions()
        {
            Value = new AzureMapConfig
            {
                SubscriptionKey = "E3WsSlkHhq-QBSEpjhWEJqmrEPC8O4xD8Gq07hqGHhk"
            };
        }
    }
}
