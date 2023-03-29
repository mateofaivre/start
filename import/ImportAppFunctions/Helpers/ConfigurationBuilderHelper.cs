using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImportAppFunctions.Helpers
{
    public static class ConfigurationBuilderHelper
    {
        public static IConfigurationRoot GetConfig(this ExecutionContext context)
        {
          return new ConfigurationBuilder()
                   .SetBasePath(context.FunctionAppDirectory)
                   // This gives you access to your application settings in your local development environment
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                   // This is what actually gets you the application settings in Azure
                   .AddEnvironmentVariables()
                   .Build();
        }
    }
}
