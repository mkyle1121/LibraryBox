using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace LibraryBoxFunction
{
    public static class LibraryBoxFunction
    {
        static string cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        static string cosmosContainer = Environment.GetEnvironmentVariable("CosmosContainer");
        static string cosmosDatabase = Environment.GetEnvironmentVariable("CosmosDatabase");
        static string cosmosContainerAddresses = Environment.GetEnvironmentVariable("CosmosContainerAddresses");

        [FunctionName("GetBookById")]
        public static async Task<IActionResult> GetBookById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {            

            string id = req.Query["id"];
            string partitionKey = req.Query["partitionKey"];

            CosmosClient client = new CosmosClient(cosmosConnectionString);
            Container container = client.GetContainer(cosmosDatabase, cosmosContainer);

            try
            {
                ItemResponse<Book> response = await container.ReadItemAsync<Book>(id, new PartitionKey(partitionKey));
                Book book = response.Resource;
                return new OkObjectResult(book);                
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }            
        }

        [FunctionName("DeleteBookById")]
        public static async Task<IActionResult> DeleteBookById(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req, ILogger log)        
        {
            string id = req.Query["id"];
            string partitionKey = req.Query["partitionKey"];

            CosmosClient client = new CosmosClient(cosmosConnectionString);
            Container container = client.GetContainer(cosmosDatabase, cosmosContainer);

            try
            {
                ItemResponse<Book> response = await container.DeleteItemAsync<Book>(id, new PartitionKey(partitionKey));
                log.LogInformation($"Deleted Book: {id} from {partitionKey}.");
                return new OkResult();                
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                log.LogError(ex.Message);
                log.LogError($"Error Deleting Book.  Not Found: {id} from {partitionKey}.");
                return new NotFoundResult();                
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message);
                log.LogError($"Error Deleting Book: {id} from {partitionKey}.");
                return new StatusCodeResult(500);
            }
        }

        [FunctionName("CreateBook")]
        public static async Task<IActionResult> CreateBook(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Book book = JsonConvert.DeserializeObject<Book>(requestBody);                     

            CosmosClient client = new CosmosClient(cosmosConnectionString);
            Container container = client.GetContainer(cosmosDatabase, cosmosContainer);

            try
            {
                ItemResponse<Book> response = await container.CreateItemAsync<Book>(book, new PartitionKey(book.Address));                
                log.LogInformation($"Created Book: {requestBody}.");
                return new StatusCodeResult(201);             
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                log.LogError(ex.Message);
                log.LogError($"Book Already Exists: {requestBody}.");
                return new StatusCodeResult(409);                
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message);
                log.LogError($"Error Creating Book: {requestBody}.");
                return new StatusCodeResult(500);
            }
        }

        [FunctionName("GetAllBooks")]
        public static async Task<IActionResult> GetAllBooks(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            CosmosClient client = new CosmosClient(cosmosConnectionString);
            Container container = client.GetContainer(cosmosDatabase, cosmosContainer);

            try
            {
                var response = container.GetItemLinqQueryable<Book>(true);                
                return new OkObjectResult(response);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message);
                log.LogError("Error Returning All Books.");
                return new StatusCodeResult(500);                
            }
        }

        [FunctionName("GetAllAddresses")]
        public static async Task<IActionResult> GetAllAddresses(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            CosmosClient client = new CosmosClient(cosmosConnectionString);
            Container container = client.GetContainer(cosmosDatabase, cosmosContainerAddresses);

            try
            {
                var response = container.GetItemLinqQueryable<Addresses>(true);
                return new OkObjectResult(response);
            }
            catch (CosmosException ex)
            {
                log.LogError(ex.Message);
                log.LogError("Error Returning All Addresses.");
                return new StatusCodeResult(500);                
            }
        }
    }
}
