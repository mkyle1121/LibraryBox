using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LibraryBox
{
    class Program
    {
        static async Task Main()
        {
            var client = new HttpClient();
            var cosmos = new Cosmos();
            var lcd = new LCD();

            while (true)
            {
                //lcd.LCDStartup();
                Console.WriteLine("Please Enter An ISBN: ");
                string ISBNInput = Console.ReadLine();
                
                
                if (string.IsNullOrEmpty(ISBNInput))
                {
                    Console.WriteLine("Please Enter An ISBN And Try Again.");
                    continue;
                }
                
                var response = await client.GetAsync($"https://www.googleapis.com/books/v1/volumes?q=isbn:{ISBNInput}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var parsedContent = JObject.Parse(content);              
                var totalItems = parsedContent["totalItems"].ToString(); 
                
                JToken parsedBook;
                if (totalItems == "0")
                {
                    Console.WriteLine("Could Not Find Book.  Please Try Again.");
                    continue;
                }
                else
                {
                    parsedBook = parsedContent["items"][0]["volumeInfo"];
                }

                var book = new Book
                {
                    Id = parsedBook["industryIdentifiers"][0]["identifier"].ToString(),
                    Author = parsedBook["authors"][0].ToString(),
                    Title = parsedBook["title"].ToString(),
                    ISBN = parsedBook["industryIdentifiers"][0]["identifier"].ToString(),
                    Date = DateTime.Now
                };

                Console.WriteLine(JsonConvert.SerializeObject(book));
                Console.WriteLine($"Author: {book.Author}  \nTitle: {book.Title} \nISBN: {book.ISBN} \nDate: {book.Date}");   

                await cosmos.CreateItem(book);
                //await cosmos.DeleteItem(book);
            }
        }
    }
}
