using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.DataSourceContexts
{
    public class CosmosDbContext
    {
        private readonly CosmosDbConfig _dbConfig;
        private readonly ILogger _logger;
        private readonly CosmosClient _client;
        private readonly Database _database;
        public CosmosDbContext(
            IOptions<CosmosDbConfig> dbConfig, 
            ILogger<CosmosDbContext> logger)
        {
            _dbConfig = dbConfig.Value;
            _logger = logger;

            _client = new CosmosClient(
                _dbConfig.EndpointUrl, 
                _dbConfig.PrimaryKey, 
                new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });

            _database = _client.GetDatabase(_dbConfig.DatabaseId);
        }

        public Container GetContainer(string containerId)
        {
            return _database.GetContainer(containerId);
        }

        /// <summary>
        /// Exécute une requête sur le container demandé
        /// </summary>
        /// <typeparam name="T">Type de container</typeparam>
        /// <param name="query">Requête à exécuter</param>
        /// <returns>Les éléments obtenus</returns>
        public async Task<CosmosQueryResponse<T>> ExecuteQuery<T>(Container container, string query, string partitionKey)
        {
            var results = new List<T>();
            double rq = 0;
            var queryDefinition = new QueryDefinition(query);
            var queryResult = container.GetItemQueryIterator<T>(queryDefinition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            });

            while (queryResult.HasMoreResults)
            {
                var currentResultSet = await queryResult.ReadNextAsync();
                rq += currentResultSet.RequestCharge;
                foreach (var item in currentResultSet)
                {
                    results.Add(item);
                }
            }

            return new CosmosQueryResponse<T>(results, rq);
        }
    }
}
