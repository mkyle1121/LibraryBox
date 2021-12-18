using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace LibraryBox
{
    class Program
    {
        private static readonly string isbnRetrievalEndpoint = "https://www.googleapis.com/books/v1/volumes?q=isbn:";
        private static readonly string azureFunctionApiKey = ConfigurationManager.AppSettings.Get("AzureFunctionApiKey");
        private static readonly string azureFunctionBaseEndpoint = ConfigurationManager.AppSettings.Get("AzureFunctionBaseEndpoint");
        private static readonly string street = ConfigurationManager.AppSettings.Get("Street");
        static async Task Main()
        {

            await DeleteBookById("9295055020", street);

            //var lcd = new LCD();
            
            
            while (true)
            {               
                Console.WriteLine("Please Enter An ISBN: ");
                string ISBNInput = Console.ReadLine();             
                if (string.IsNullOrEmpty(ISBNInput))
                {
                    Console.WriteLine("Please Enter An ISBN And Try Again.");
                    continue;
                }
                ISBNInput = ISBNInput.Replace("-", "");


                string content = string.Empty;
                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync($"{isbnRetrievalEndpoint}{ISBNInput}");
                    response.EnsureSuccessStatusCode();
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Unable To Request Book Information.");
                    Console.WriteLine(ex.Message);
                    continue;
                }                
                
                JObject parsedContent = JObject.Parse(content);
                Console.WriteLine(parsedContent);
                
                JToken parsedBook;
                string totalItems = parsedContent["totalItems"].ToString();
                if (totalItems == "0")
                {
                    Console.WriteLine("Could Not Find Book.  Please Try Again.");
                    continue;
                }
                else
                {                    
                    parsedBook = parsedContent["items"][0]["volumeInfo"];
                }                

                Book book = new Book
                {
                    id = ISBNInput,
                    author = parsedBook["authors"][0].ToString(),
                    title = parsedBook["title"].ToString(),
                    isbn = ISBNInput,
                    date = DateTime.Now.ToShortDateString(),
                    street = street
                };           

                await CreateBook(book);
            }
            
        }

        private static async Task CreateBook(Book book)
        {
            string apiEndpointCreateBook = $"{azureFunctionBaseEndpoint}CreateBook?code={azureFunctionApiKey}";

            string bookContent = JsonConvert.SerializeObject(book);
            Console.WriteLine(bookContent);

            HttpContent httpContent = new StringContent(bookContent);
            HttpClient client = new HttpClient();            
            HttpResponseMessage apiResponse = await client.PostAsync(apiEndpointCreateBook, httpContent);
            if(apiResponse.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("Book Created.");
            }
            else if(apiResponse.StatusCode == HttpStatusCode.InternalServerError)
            {
                Console.WriteLine("Internal Server Error.");
            }
            else
            {
                Console.WriteLine("Error Creating Book.");
            }
        }

        private static async Task DeleteBookById(string id, string partitionKey)
        {
            string apiEndpointDeleteBookById = $"{azureFunctionBaseEndpoint}DeleteBookById?code={azureFunctionApiKey}&id={id}&partitionKey={partitionKey}";
            HttpClient client = new HttpClient();
            
            HttpResponseMessage apiResponse = await client.DeleteAsync(apiEndpointDeleteBookById);
            if(apiResponse.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Book Deleted.");
            }
            else if(apiResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Book Not Found.");
            } 
            else
            {
                Console.WriteLine("Error Deleting Book.");
            }
        }


    }
}
