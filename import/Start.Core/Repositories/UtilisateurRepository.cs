using Microsoft.Extensions.Logging;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;

namespace Start.Core.Repositories
{
    public class UtilisateurRepository : Repository<Utilisateur>
    {
        public UtilisateurRepository(
            CosmosDbContext dbContext,
            ILogger<UtilisateurRepository> logger) : base("utilisateur", dbContext, logger)
        {

        }
    }
}
