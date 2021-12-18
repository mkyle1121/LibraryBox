using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace LibraryBoxLibrary
{
    

    public class Cosmos
    {        
        private CosmosClient cosmosClient;
        private Container container;

        public Cosmos(string database, string container, string key, string endpointUri)
        {            
            this.cosmosClient = new CosmosClient(endpointUri, key);
            this.container = cosmosClient.GetContainer(database, container);
        }
        public async Task CreateItemAsync(Book book)
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
        public async Task DeleteItemAsync(Book book)
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

        public async Task<List<Book>> GetAllItemsInContainerAsync()
        {
            var query = container.GetItemLinqQueryable<Book>();
            var iterator = query.ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            return results.ToList<Book>();

            //string sqlQueryText = "SELECT * FROM c";
            //QueryDefinition definition = new QueryDefinition(sqlQueryText);
            //var iterator = container.GetItemQueryIterator<Book>(definition);



        }
    }
}

