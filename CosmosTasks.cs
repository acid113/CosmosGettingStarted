using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosGettingStartedTutorial
{
    public class CosmosTasks
    {
        const string ENDPOINT = "https://imlocalcosmostraining.documents.azure.com:443/";
        const string PRIMARYKEY = "D8ThyzfMgqhkHRiiZYDkZF7WB1Kaphy7asOwsijwkU8aLQwDL1RSniiyOXR9aPFRm030pBiCKeVkDwXWfeqmug==";

        CosmosClient _cosmosClient;
        Database _database;
        Container _container;

        const string DATABASEID = "FamilyDatabase";
        const string CONTAINERID = "FamilyContainer";

        public async Task GetStartedDemoAsync()
        {
            _cosmosClient = new CosmosClient(ENDPOINT, PRIMARYKEY);
            await CreateDatabaseAsync();
            await CreateContainerAsync();
            await AddItemsToContainerAsyc();
            await QueryItemsAsync();
            await UpdateFamilyItemAsync();
            await DeleteFamilyItemAsync();
            await DeleteDatabaseAsync();
        }

        private async Task CreateDatabaseAsync()
        {
            // Creates database if it doesn't exist, gets if it exists
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DATABASEID);
            Console.WriteLine("Created database: {0}", _database.Id);
        }

        private async Task CreateContainerAsync()
        {
            _container = await _database.CreateContainerIfNotExistsAsync(CONTAINERID, "/LastName");
            Console.WriteLine("Created container: {0}", _container.Id);
        }

        private async Task AddItemsToContainerAsyc()
        {
            #region 1st Family
            var andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new List<Parent>
                {
                    new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay" }
                },
                Children = new List<Child>
                {
                    new Child
                    {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new List<Pet>
                        {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            try
            {
                // Read the item to see if it exists. Exception error will occur if it's not found
                var response = await _container.ReadItemAsync<Family>(andersenFamily.Id, new PartitionKey(andersenFamily.LastName));
                Console.WriteLine("Item in database with id: {0} already exists", response.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var response = await _container.CreateItemAsync<Family>(andersenFamily, new PartitionKey(andersenFamily.LastName));
                Console.WriteLine("Item in database with id: {0} created", response.Resource.Id);
                Console.WriteLine("Request charge property: {0}", response.RequestCharge);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"1. AddItemsToContainerAsyc() error: {ex.Message}");
            }
            #endregion

            #region 2nd Family
            var wakefieldFamily = new Family
            {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new List<Parent>
                {
                    new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
                    new Parent { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new List<Child>
                {
                    new Child
                    {
                        FamilyName = "Merriam",
                        FirstName = "Jesse",
                        Gender = "female",
                        Grade = 8,
                        Pets = new List<Pet>
                        {
                            new Pet { GivenName = "Goofy" },
                            new Pet { GivenName = "Shadow" }
                        }
                    },
                    new Child
                    {
                        FamilyName = "Miller",
                        FirstName = "Lisa",
                        Gender = "female",
                        Grade = 1
                    }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = true
            };

            try
            {
                var wakefieldFamilyResponse = await _container.ReadItemAsync<Family>(wakefieldFamily.Id, new PartitionKey(wakefieldFamily.LastName));
                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var response = await _container.CreateItemAsync<Family>(wakefieldFamily, new PartitionKey(wakefieldFamily.LastName));
                Console.WriteLine("Item in database with id: {0} created", response.Resource.Id);
                Console.WriteLine("Request charge property: {0}", response.RequestCharge);
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"2. AddItemsToContainerAsyc() error: {ex.Message}");
            }
            #endregion

        }

        private async Task QueryItemsAsync()
        {
            var families = new List<Family>();

            #region Using GetItemQueryIterator();
            Console.WriteLine("using GetItemQueryIterator()");

            //Note: select all 
            var query = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

            //Note: or select specific fields
            //var query = "SELECT c.LastName, c.IsRegistered FROM c WHERE c.LastName = 'Andersen'";

            Console.WriteLine($"Running query: {query}");
            var queryDefinition = new QueryDefinition(query);

            //Note: unpopulated Family properties will be displayed as NULL
            var resultSetIterator = _container.GetItemQueryIterator<Family>(queryDefinition);

            //Note: we can use "object" for dynamic properties
            //var resultSetIterator = _container.GetItemQueryIterator<object>(queryDefinition);

            while (resultSetIterator.HasMoreResults)
            {
                var currentResultSet = await resultSetIterator.ReadNextAsync();

                foreach (var family in currentResultSet)
                {
                    families.Add(family);   //remove if "family" is of object type

                    // if "family" is of object type, it will be displayed with indents
                    Console.WriteLine($"\nRead {family}");
                }

            }
            #endregion

            #region Using GetItemLinqQueryable()
            Console.WriteLine("\nusing GetItemLinqQueryable()");
            Console.WriteLine("Running query: SELECT * FROM c WHERE c.LastName = 'Wakefield'");
            var itemQueryable = _container.GetItemLinqQueryable<Family>(true);
            var resultSet = itemQueryable.Where(x => x.LastName == "Wakefield");
            //var resultSet = from items in itemQueryable
            //                        select new Family()
            //                        {
            //                            LastName = items.LastName
            //                            , IsRegistered = items.IsRegistered
            //                        };
            foreach (var family in resultSet)
            {
                families.Add(family);
                Console.WriteLine($"\nRead {family}");
            }

            // ? What if the properties are dynamic? Can we use "objects"?
            #endregion

        }

        private async Task UpdateFamilyItemAsync()
        {
            var familyId = "Wakefield.7";
            var key = "Wakefield";
            var fieldResponse = await _container.ReadItemAsync<Family>(familyId, new PartitionKey(key));
            var itemBody = fieldResponse.Resource;
            var previousGrade = itemBody.Children[1].Grade;

            //do updates here
            itemBody.IsRegistered = false;
            itemBody.Children[1].Grade = 2;

            //apply updates
            fieldResponse = await _container.ReplaceItemAsync<Family>(itemBody, itemBody.Id, new PartitionKey(key));
            Console.WriteLine("\nUpdated {0} family. {1} who was in Grade {2} is now Grade {3}"
                , itemBody.LastName, itemBody.Children[1].FirstName
                , previousGrade
                , fieldResponse.Resource.Children[1].Grade);
        }

        private async Task DeleteFamilyItemAsync()
        {
            var familyId = "Wakefield.7";
            var key = "Wakefield";

            try
            {
                var fieldResponse = await _container.DeleteItemAsync<Family>(familyId, new PartitionKey(key));
                Console.WriteLine($"\nDeleted {key} family");
            }
            catch (CosmosException ce) when (ce.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Family not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteFamilyItemAsync() error: {ex.Message}");
            }
            
        }

        private async Task DeleteDatabaseAsync()
        {
            await _database.DeleteAsync();
            _cosmosClient.Dispose();

            Console.WriteLine($"\nDatabase {DATABASEID} deleted");
            
            
        }
    }
}
