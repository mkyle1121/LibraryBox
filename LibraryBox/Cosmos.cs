using Microsoft.Azure.Cosmos;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace LibraryBox
{
    public class Cosmos
    {        
        private readonly ConnectionStringSettings cosmosKey = ConfigurationManager.ConnectionStrings["cosmosKey"];
        private readonly ConnectionStringSettings cosmosContainer = ConfigurationManager.ConnectionStrings["cosmosContainer"];
        private readonly ConnectionStringSettings cosmosDatabase = ConfigurationManager.ConnectionStrings["cosmosDatabase"];
        private readonly ConnectionStringSettings cosmosEndpointUri = ConfigurationManager.ConnectionStrings["cosmosEndpointUri"];        
        private CosmosClient cosmosClient;
        private Container container;
        
        public Cosmos()
        {          
            cosmosClient = new CosmosClient(cosmosEndpointUri.ConnectionString, cosmosKey.ConnectionString);
            container = cosmosClient.GetContainer(cosmosDatabase.ConnectionString, cosmosContainer.ConnectionString);           
        }
        public async Task CreateItem(Book book)
        {
            try
            {
                ItemResponse<Book> bookResponse = await container.ReadItemAsync<Book>(book.Id, new PartitionKey(book.Id));
                Console.WriteLine($"Item Already Exists: {bookResponse.Resource.Title}.");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                ItemResponse<Book> bookResponse = await container.CreateItemAsync<Book>(book, new PartitionKey(book.Id));
                Console.WriteLine($"Added Item: {bookResponse.Resource.Title}.");
            }
        }
        public async Task DeleteItem(Book book)
        {
            try
            {
                ItemResponse<Book> bookResponse = await container.ReadItemAsync<Book>(book.Id, new PartitionKey(book.Id));                
                bookResponse = await container.DeleteItemAsync<Book>(book.Id, new PartitionKey(book.Id));
                Console.WriteLine($"Deleted Item: {book.Title}.");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Item Not Found: {book.Title}");
            }
        }



    }
}
