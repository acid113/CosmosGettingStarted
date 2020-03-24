//Reference: https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-get-started

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;


namespace CosmosGettingStartedTutorial
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting operation..");
                var p = new CosmosTasks();
                await p.GetStartedDemoAsync();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine($"{de.StatusCode} error occurred: {de.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Main Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }

        

    }
}
