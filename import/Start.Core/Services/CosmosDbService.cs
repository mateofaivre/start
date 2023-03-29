using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class CosmosDbService
    {
        // The Azure Cosmos DB endpoint for running this GetStarted sample.
        private string _endpointUrl = "https://start-azure-cosmos-db.documents.azure.com:443/" /*Environment.GetEnvironmentVariable("EndpointUrl")*/;

        /// The primary key for the Azure DocumentDB account.
        private string _primaryKey = "4zONb9mLKqNIcBps96bFPqqRA47TeVRdcPJH9alZ9SXdFWflMgruDvrTjkvFBa68ZcmTi7Mytrlh6gOJcIxiFQ=="/*Environment.GetEnvironmentVariable("PrimaryKey")*/;

        private string _databaseId = "startDB";
        private string _containerId = "oeuvre";

        private CosmosClient _client;
        private Container _container;

        private ILogger _logger;

        public CosmosDbService(ILogger logger, string containerId = "oeuvre", string databaseId = "startDB")
        {
            _logger = logger;
            _client = new CosmosClient(_endpointUrl, _primaryKey, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway });

            _containerId = containerId;
            _databaseId = databaseId;
            _container = _client.GetDatabase(_databaseId).GetContainer(_containerId);
        }

        public async Task InsertOrMergeAsync(Oeuvre item)
        {
            try
            {
                var response = await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        public async Task InsertOrMergeAsync(Partenaire item)
        {
            try
            {
                var response = await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        /// <summary>
        /// Delete oeuvre item in this current partition
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task Delete(Oeuvre item)
        {
            try
            {
                var response = await _container.DeleteItemAsync<Oeuvre>(item.Marqueur, new PartitionKey(item.PartitionKey));
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }

        /// <summary>
        /// Rercherche les oeuvres présentes dans une zone géographique
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<OeuvreQueryResult> SearchOeuvreInLocation(string location)
        {
            //var point = (JObject)JsonConvert.DeserializeObject(query);

            //var longString = point["lng"].Value<string>();
            //var latString = point["lat"].Value<string>();
            //var command = "SELECT * FROM c WHERE ST_DISTANCE(c.location, { 'type': 'Point', 'coordinates':["+ latString + ","+ longString + "]}) < 1000";


            var command = $"SELECT * FROM c WHERE ST_WITHIN(c.location, {location})";
            //if (!string.IsNullOrEmpty(userId))
            //{
            //    command = command + $" AND c.utlisisateurId = {userId} OR c.utlisisateurIdModification = {userId}";
            //}

            //command = "SELECT* FROM c where c.id = '10506032-155367896052-IMG_20190529_190217_018'";

            //command = "SELECT* FROM c where c.status = 2";

            return await RunOeuvreQuery(command);
        }

        /// <summary>
        /// Rercherche les oeuvres présentes dans une zone géographique
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<PartenaireQueryResult> SearchPartenaireInLocation(string location)
        {
            //var point = (JObject)JsonConvert.DeserializeObject(query);

            //var longString = point["lng"].Value<string>();
            //var latString = point["lat"].Value<string>();
            //var command = "SELECT * FROM c WHERE ST_DISTANCE(c.location, { 'type': 'Point', 'coordinates':["+ latString + ","+ longString + "]}) < 1000";


            var command = $"SELECT * FROM c WHERE ST_WITHIN(c.location, { location})";


            //command = "SELECT* FROM c where c.id = '10506032-155367896052-IMG_20190529_190217_018'";

            //command = "SELECT* FROM c where c.status = 2";

            return await RunPartenaireQuery(command);
        }

        /// <summary>
        /// Recherche une oeuvre en fonction de son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Oeuvre> FindOeuvre(string id, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var command = $"SELECT * FROM c WHERE c.id = '{id}'";
            var datas = await RunQuery<Oeuvre>(command, partitionKey);
            return datas.FirstOrDefault();
        }

        /// <summary>
        /// Recherche une oeuvre supprimée en fonction de son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Oeuvre> FindDeletedOeuvre(string id, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var command = $"SELECT * FROM c WHERE c.id = '{id}' and c.status = {(int)OeuvreStatus.Deleted}";
            var datas = await RunQuery<Oeuvre>(command, partitionKey);
            return datas.FirstOrDefault();
        }

        /// <summary>
        /// Recherche toutes les oeuvres à approuver
        /// </summary>
        /// <returns></returns>
        public async Task<List<Oeuvre>> FindAllOeuvreToApprove()
        {
            var oeuvres = new List<Oeuvre>();
            var query = $"SELECT * FROM c WHERE c.status = {(int)OeuvreStatus.WaitingValidation}";
            var datas = await RunQuery<Oeuvre>(query, OeuvrePartitionKeys.Item);

            if (datas.Any())
            {
                oeuvres = datas.Where(o => !o.DateApprobation.HasValue &&
                                        (o.DateModification.HasValue && o.DateModification.Value.Date <= DateTime.Now.Date
                                        || o.DateCreation.Date <= DateTime.Now.Date)
                                      )
                              .ToList();
            }

            return oeuvres;
        }

        /// <summary>
        /// Recherche toutes les oeuvres
        /// </summary>
        /// <returns></returns>
        public async Task<OeuvreQueryResult> FindAllOeuvre()
        {
            var oeuvres = new List<Oeuvre>();
            var query = $"SELECT * FROM c";
            return await RunOeuvreQuery(query, OeuvrePartitionKeys.Item);
        }

        /// <summary>
        /// Recherche toutes les oeuvres par status
        /// </summary>
        /// <returns></returns>
        public async Task<OeuvreQueryResult> FindAllOeuvreByStatus(OeuvreStatus status)
        {
            var oeuvres = new List<Oeuvre>();
            var query = $"SELECT * FROM c where c.status = {(int)status} and c.date > DateTimeAdd('DD', -14, GetCurrentDateTime())";
            return await RunOeuvreQuery(query, OeuvrePartitionKeys.Item);
        }

        /// <summary>
        /// Exécute une requête cosmos db et retourne le résultat en Json
        /// </summary>
        /// <param name="query">La requête</param>
        /// <returns>Le json obtenu</returns>
        private async Task<OeuvreQueryResult> RunOeuvreQuery(string query, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var oeuvres = new List<OeuvreGeoJson>();
            double rq = 0;
            var queryDefinition = new QueryDefinition(query);
            var queryResult = _container.GetItemQueryIterator<Oeuvre>(queryDefinition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            });

            while (queryResult.HasMoreResults)
            {
                var currentResultSet = await queryResult.ReadNextAsync();
                rq += currentResultSet.RequestCharge;
                foreach (var oeuvre in currentResultSet)
                {
                    var oeuvreJson = new OeuvreGeoJson(oeuvre);
                    oeuvres.Add(oeuvreJson);
                }
            }

            var result = new OeuvreQueryResult(oeuvres)
            {
                Count = oeuvres.Count,
                RequestUnit = rq
            };

            return result;
        }

        /// <summary>
        /// Exécute une requête cosmos db et retourne le résultat en Json
        /// </summary>
        /// <param name="query">La requête</param>
        /// <returns>Le json obtenu</returns>
        private async Task<PartenaireQueryResult> RunPartenaireQuery(string query, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var partenaires = new List<PartenaireGeoJson>();
            double rq = 0;
            var queryDefinition = new QueryDefinition(query);
            var queryResult = _container.GetItemQueryIterator<Partenaire>(queryDefinition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            });

            while (queryResult.HasMoreResults)
            {
                var currentResultSet = await queryResult.ReadNextAsync();
                rq += currentResultSet.RequestCharge;
                foreach (var partenaire in currentResultSet)
                {
                    partenaires.Add(new PartenaireGeoJson(partenaire));
                }
            }

            var result = new PartenaireQueryResult(partenaires)
            {
                Count = partenaires.Count,
                RequestUnit = rq
            };

            return result;
        }

        /// <summary>
        /// Exécute une requête sur le container demandé
        /// </summary>
        /// <typeparam name="T">Type de container</typeparam>
        /// <param name="query">Requête à exécuter</param>
        /// <returns>Les éléments obtenus</returns>
        public async Task<List<T>> RunQuery<T>(string query, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var result = new List<T>();
            var queryDefinition = new QueryDefinition(query);
            var queryResult = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            });

            while (queryResult.HasMoreResults)
            {
                var currentResultSet = await queryResult.ReadNextAsync();
                foreach (var item in currentResultSet)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}
