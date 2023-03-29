using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Start.Core.Process;
using System;
using System.Threading.Tasks;

namespace Start.Migration.Batch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddSimpleConsole(i => i.ColorBehavior = LoggerColorBehavior.Disabled);
                    builder.AddConsole();
                });
                loggerFactory.AddProvider(new DebugLoggerProvider());

                var process = new MigrationProcess(loggerFactory);
                await process.Execute();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey(true);
        }
    }
}
