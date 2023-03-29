using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Start.Core.Configurations;
using Start.Core.DataSourceContexts;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(ImportAppFunctions.Startup))]
namespace ImportAppFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<CosmosDbConfig>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("CosmosDbConfig").Bind(settings);
                });

            builder.Services.AddOptions<AzureStorageConfig>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureStorageConfig").Bind(settings);
                });

            builder.Services.AddOptions<AzureMapConfig>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("AzureMapConfig").Bind(settings);
                });

            builder.Services.AddTransient<CosmosDbContext>();
            builder.Services.AddTransient<OeuvreRepository>();
            builder.Services.AddTransient<UtilisateurRepository>();
            builder.Services.AddTransient<AzureStorageService>();
            builder.Services.AddTransient<AzureMapService>();

            //builder.Services.AddLogging();

            //builder.Services.AddTransient(s =>
            //{
            //    return new CosmosDbContext(s.GetService<CosmosDbConfig>(), s.GetService<ILogger>());
            //});

            //builder.Services.AddTransient(s =>
            //{
            //    return new OeuvreRepository(s.GetService<CosmosDbContext>(), s.GetService<ILogger>());
            //});
        }
    }
}
