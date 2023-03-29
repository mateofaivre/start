using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Start.Core.Services
{
    public class ImportService
    {
        private readonly string _connectionString;
        private readonly QueueClient _geolocalisationQueue;
        private readonly CosmosDbService _cosmosDbService;
        private const string _geolocalisationQueueName = "geolocalisation-items";

        public ImportService(string connectionString, ILogger logger, string container = "oeuvre", string database = "startDB")
        {
            _connectionString = connectionString;
            var test = new QueueServiceClient(_connectionString);
            _geolocalisationQueue =  test.GetQueueClient(_geolocalisationQueueName);
            _cosmosDbService = new CosmosDbService(logger, container, database);
        }

        public void InsertOrMerge(IEnumerable<Oeuvre> oeuvreItems)
        {
            foreach (var oeuvre in oeuvreItems)
            {
                _cosmosDbService.InsertOrMergeAsync(oeuvre).GetAwaiter().GetResult();
            }
        }

        public void InsertOrMerge(IEnumerable<Partenaire> partenaireItems)
        {
            foreach (var partenaire in partenaireItems)
            {
                _cosmosDbService.InsertOrMergeAsync(partenaire).GetAwaiter().GetResult();
            }
        }

        public void InsertOrMerge(IEnumerable<ITableEntity> entities)
        {
            var tableClient = new TableClient(_connectionString, "Oeuvre");
            tableClient.CreateIfNotExistsAsync();

            var batchOperations = new List<TableTransactionAction>();

            foreach (var entityGroup in entities.GroupBy(f => f.PartitionKey))
            {
                foreach (var entity in entityGroup)
                {
                    if (batchOperations.Count < 100)
                    {
                        batchOperations.Add(new TableTransactionAction(TableTransactionActionType.UpsertMerge, entity));
                    }
                    else
                    {
                        tableClient.SubmitTransaction(batchOperations);
                        batchOperations = new List<TableTransactionAction>
                        { 
                            new TableTransactionAction(TableTransactionActionType.UpsertMerge, entity) 
                        };
                    }
                }

                tableClient.SubmitTransaction(batchOperations);
                batchOperations = new List<TableTransactionAction>();
            }

            if (batchOperations.Count > 0)
            {
                tableClient.SubmitTransaction(batchOperations);
            }
        }

        public void PushGeolocalisationDemand(IEnumerable<Oeuvre> oeuvreItems)
        {
            foreach (var oeuvre in oeuvreItems)
            {
                if (oeuvre.Location == null)
                {
                    _geolocalisationQueue.SendMessageAsync(oeuvre.ToJson());
                }
            }
        }
    }
}
