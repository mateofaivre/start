using Start.Core.Services;
using System;
using System.Threading.Tasks;

namespace Start.Core.Test
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Lancement des tests");

            try
            {

                var service = new AzureMapRouteDirectionService();
                var result = await service.GetRoute(null);

                Console.Out.WriteLine(result);
            }
            catch(Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

            Console.ReadKey(true);
        }
    }
}
