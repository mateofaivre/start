using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Repositories
{
    public class Repository<TEntity>
        where TEntity : Entity
    {
        protected readonly CosmosDbContext _dbContext;
        protected readonly ILogger _logger;
        protected readonly Container _container;

        protected string ContainerId { get; private set; }
        public Repository(
            string containerId,
            CosmosDbContext dbContext,
            ILogger logger)
        {
            ContainerId = containerId;
            _dbContext = dbContext;
            _logger = logger;
            _container = _dbContext.GetContainer(ContainerId);
        }

        public async Task<TEntity> InsertOrMergeAsync(TEntity item)
        {
            TEntity result = null;
            try
            {
                var response = await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = response.Resource;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return result;
        }

        public async Task<bool> Delete(TEntity item)
        {
            bool result = false;
            try
            {
                var response = await _container.DeleteItemAsync<TEntity>(item.GetId(), new PartitionKey(item.PartitionKey));
                result = response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return result;
        }

        /// <summary>
        /// Recherche toutes les entités
        /// </summary>
        /// <returns></returns>
        public async Task<List<TEntity>> GetAll(string partitionKey)
        {
            var query = $"SELECT * FROM c";
            var datas = await _dbContext.ExecuteQuery<TEntity>(_container, query, partitionKey);
            return datas.Results;
        }

        /// <summary>
        /// Recherche une entité en fonction de son id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TEntity> GetById(string id, string partitionKey)
        {
            var query = $"SELECT * FROM c WHERE c.id = '{id}'";
            var datas = await _dbContext.ExecuteQuery<TEntity>(_container, query, partitionKey);
            return datas.Results.FirstOrDefault();
        }
    }
}
