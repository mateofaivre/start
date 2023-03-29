using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Start.Core.Process;
using System;
using System.Threading.Tasks;


namespace Start.UnitTest
{
    class Program
    {
        static  async Task Main(string[] args)
        {
            Console.WriteLine("Unit test!");

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(i => i.ColorBehavior = LoggerColorBehavior.Disabled);
                builder.AddConsole();
            });
            loggerFactory.AddProvider(new DebugLoggerProvider());

            try
            {
                var testImage = new ExifTest();
                testImage.TestFromBase64(string.Empty);

                //ZipTest zipTest = new ZipTest();
                //zipTest.ZipManyFilesTest();
                //var azureMapServiceTest = new AzureMapServiceTest(loggerFactory);
                //await azureMapServiceTest.TestGetPositionAsync();
                //await azureMapServiceTest.TestGetAdresseAsync();

                //var importProcessTest = new ImportProcessTest(loggerFactory);
                //await importProcessTest.TestExecute();

                //var test = new FindOeuvreProcessTest(loggerFactory);

                //Console.WriteLine("Test generate sas url ...");
                //var result = await test.TestGetBlob();
                //Console.WriteLine("uri " + result.ToString() + "r/n");

                //Console.WriteLine("Test generate oeuvre and uri");
                //var data = await test.TestGetWithLocation();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadKey(true);
            //WebClient client = new WebClient();
            //await client.DownloadFileTaskAsync(new Uri("blob:https://app.start-prod.fr/0346de14-6a0f-4c8a-8916-c634c9a153ff"), "c:/test.png");
        }
    }
}
