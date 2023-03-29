using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Logging;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Repositories
{
    public class OeuvreRepository : Repository<Oeuvre>
    {
        //private readonly CosmosDbContext _dbContext;
        //private readonly ILogger _logger;
        //private readonly Container _container;
        //private const string _containerId = "oeuvre";
        public OeuvreRepository(
            CosmosDbContext dbContext,
            ILogger<OeuvreRepository> logger): base("oeuvre", dbContext, logger)
        {
            //_dbContext = dbContext;
            //_logger = logger;
            //_container = _dbContext.GetContainer(_containerId);
        }

        //public async Task<Oeuvre> InsertOrMergeAsync(Oeuvre item)
        //{
        //    Oeuvre result = null;
        //    try
        //    {
        //        var response = await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
        //        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //        {
        //            result = response.Resource;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, e.Message);
        //    }

        //    return result;
        //}

        //public async Task<bool> Delete(Oeuvre item)
        //{
        //    bool result = false;
        //    try
        //    {
        //        var response = await _container.DeleteItemAsync<Oeuvre>(item.Marqueur, new PartitionKey(item.PartitionKey));
        //        result = response.StatusCode == System.Net.HttpStatusCode.OK;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, e.Message);
        //    }

        //    return result;
        //}

        /// <summary>
        /// Rercherche les oeuvres présentes dans une zone géographique
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<OeuvreQueryResult> SearchOeuvreInLocation(string location)
        {
            var query = $"SELECT * FROM c WHERE ST_WITHIN(c.location, {location})";
            return await ExecuteQuery(query);
        }

        /// <summary>
        /// Recherche une oeuvre en fonction de son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Oeuvre> FindOeuvre(string id, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var command = $"SELECT * FROM c WHERE c.id = '{id}'";
            var datas = await _dbContext.ExecuteQuery<Oeuvre>(_container, command, partitionKey);
            return datas.Results.FirstOrDefault();
        }

        /// <summary>
        /// Recherche une oeuvre en fonction de sa position
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Oeuvre> FindOeuvre(Point position, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var location = Helpers.ParserHelper.ConvertToString(position);
            var command = $"SELECT * FROM c WHERE ST_DISTANCE(c.location, {{'type': 'Point', 'coordinates':[{location}]}}) = 0";
            var datas = await _dbContext.ExecuteQuery<Oeuvre>(_container, command, partitionKey);
            return datas.Results.FirstOrDefault();
        }

        /// <summary>
        /// Recherche une oeuvre supprimée en fonction de son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Oeuvre> FindDeletedOeuvre(string id, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var command = $"SELECT * FROM c WHERE c.id = '{id}' and c.status = {(int)OeuvreStatus.Deleted}";
            var datas = await _dbContext.ExecuteQuery<Oeuvre>(_container, command, partitionKey);
            return datas.Results.FirstOrDefault();
        }

        /// <summary>
        /// Recherche toutes les oeuvres à approuver
        /// </summary>
        /// <returns></returns>
        public async Task<List<Oeuvre>> FindAllOeuvreToApprove()
        {
            var oeuvres = new List<Oeuvre>();
            var query = $"SELECT * FROM c WHERE c.status = {(int)OeuvreStatus.WaitingValidation}";
            var datas = await _dbContext.ExecuteQuery<Oeuvre>(_container, query, OeuvrePartitionKeys.Item);

            if (datas.Results.Any())
            {
                oeuvres = datas.Results.Where(o => !o.DateApprobation.HasValue &&
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
        public async Task<OeuvreQueryResult> FindAllOeuvre(string partitionKeys = OeuvrePartitionKeys.Item)
        {
            var query = $"SELECT * FROM c";
            return await ExecuteQuery(query, partitionKeys);
        }

        public async Task<List<Oeuvre>> FindAllOeuvreWithQuery(string query)
        {
            var datas = await _dbContext.ExecuteQuery<Oeuvre>(_container, query, OeuvrePartitionKeys.Item);
            return datas.Results;
        }

        /// <summary>
        /// Recherche toutes les oeuvres par status
        /// </summary>
        /// <returns></returns>
        public async Task<OeuvreQueryResult> FindAllOeuvreByStatus(OeuvreStatus status)
        {
            var query = $"SELECT * FROM c where c.status = {(int)status} and c.date > DateTimeAdd('DD', -14, GetCurrentDateTime())";
            return await ExecuteQuery(query, OeuvrePartitionKeys.Item);
        }

        /// <summary>
        /// Exécute une requête cosmos db et retourne le résultat en Json
        /// </summary>
        /// <param name="query">La requête</param>
        /// <returns>Le json obtenu</returns>
        private async Task<OeuvreQueryResult> ExecuteQuery(string query, string partitionKey = OeuvrePartitionKeys.Item)
        {
            var response = await _dbContext.ExecuteQuery<Oeuvre>(_container, query, partitionKey);
            var oeuvres = new List<OeuvreGeoJson>();

            if (response.Count > 0)
            {
                oeuvres = response.Results.Select(o => new OeuvreGeoJson(o)).ToList();
            }

            var result = new OeuvreQueryResult(oeuvres)
            {
                Count = oeuvres.Count,
                RequestUnit = response.RequestUnit
            };

            return result;
        }
    }
}
