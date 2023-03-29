using Start.Core.Process;
using Start.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Start.GoobBarber.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--------->Traitement de géolocalisation en cours ...");
            Console.WriteLine();
            if (args.Length == 0)
                return;

            try
            {
                var filePath = args[0];
                var process = new GoogBarberProcess();
                await process.Execute(filePath);

            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("--------->Géolocalisation terminée");
            Console.ReadKey(true);
        }
    }
}
