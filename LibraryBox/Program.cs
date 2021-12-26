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
        private static readonly string azureFunctionBaseEndpoint = ConfigurationManager.AppSettings.Get("AzureFunctionBaseEndpoint");
        private static readonly string isbnRetrievalEndpoint = "https://www.googleapis.com/books/v1/volumes?q=isbn:";
        private static readonly string azureFunctionApiKey = ConfigurationManager.AppSettings.Get("AzureFunctionApiKey");
        private static readonly string address = ConfigurationManager.AppSettings.Get("Address");       
        private static LCD lcd;
        private static Button button;
        
        static async Task Main()
        {
            lcd = new LCD();
            button = new Button();
            button.ButtonChange += OnButtonChange;
            Task buttonPress = button.ButtonPress();
            ConsoleKeyInfo conKeyInfo = new ConsoleKeyInfo();

            while(true)
            {
                lcd.WriteTopText(button.state.ToString());
                string ISBNInput = string.Empty;
                while(conKeyInfo.Key != ConsoleKey.Enter)
                {
                    conKeyInfo = Console.ReadKey();
                    switch (conKeyInfo.Key)
                    {
                        case ConsoleKey.Enter:
                            continue;
                        case ConsoleKey.Backspace:
                            if(ISBNInput.Length > 0)
                            {
                                ISBNInput = ISBNInput.Remove(ISBNInput.Length - 1);
                            }
                            break;
                        default:
                            ISBNInput += conKeyInfo.KeyChar;
                            break;
                    }
                    lcd.WriteBottomText(ISBNInput);
                }
                conKeyInfo = default(ConsoleKeyInfo);

                if(string.IsNullOrEmpty(ISBNInput))
                {
                    Console.WriteLine("Empty, Try Again.");
                    lcd.WriteBottomText("Empty, Try Again");
                    await Task.Delay(3000);
                    lcd.ClearBottomText();
                    continue;
                }
                ISBNInput = ISBNInput.Replace("-", "");

                switch(button.state.ToString())
                {
                    case "DEPOSIT_BOOK":
                        await StartToCreateBook(ISBNInput);
                        break;
                    case "WITHDRAW_BOOK":
                        await DeleteBookById(ISBNInput, address);
                        break;
                    default:
                        Console.WriteLine("Cannot Determine Button State.");
                        break;
                }               
            }           
        }              
        private static async Task StartToCreateBook(string ISBNInputCreate)
        {
            string content = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"{isbnRetrievalEndpoint}{ISBNInputCreate}");
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Error Requesting Book Information.");
                Console.WriteLine(ex.Message);
                lcd.WriteBottomText("Error Retrieving");
                await Task.Delay(3000);
                lcd.ClearBottomText();
                return;
            }
            JObject parsedContent = JObject.Parse(content);

            JToken parsedBook;
            string totalItems = parsedContent["totalItems"].ToString();
            if(totalItems == "0")
            {
                Console.WriteLine("Book Not Found.");
                lcd.WriteBottomText("Book Not Found");
                await Task.Delay(3000);
                lcd.ClearBottomText();
                return;
            }
            else
            {
                parsedBook = parsedContent["items"][0]["volumeInfo"];
            }

            Book book = new Book
            {
                id = ISBNInputCreate,
                author = parsedBook["authors"] == null ? string.Empty : parsedBook["authors"][0].ToString(),
                title = parsedBook["title"] == null ? string.Empty : parsedBook["title"].ToString(),
                isbn = ISBNInputCreate,
                category = parsedBook["categories"] == null ? string.Empty : parsedBook["categories"][0].ToString(),
                smallThumbnail = parsedBook["imageLinks"] == null ? string.Empty : parsedBook["imageLinks"]["smallThumbnail"].ToString(),
                date = DateTime.Now.ToShortDateString(),
                address = address
            };

            await CreateBook(book);            
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
                Console.WriteLine("Book Deposited!");
                lcd.WriteBottomText("Book Deposited!");
                await Task.Delay(3000);
                lcd.ClearBottomText();

            }
            else if(apiResponse.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Book Exists.");                
                lcd.WriteBottomText("Book Exists");
                await Task.Delay(3000);
                lcd.ClearBottomText();
            }
            else
            {
                Console.WriteLine("Error Creating Book.");
                lcd.WriteBottomText("Error Creating");
                await Task.Delay(3000);
                lcd.ClearBottomText();
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
                lcd.WriteBottomText("Book Withdrawn!");
                await Task.Delay(3000);
                lcd.ClearBottomText();

            }
            else if(apiResponse.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Book Not Found.");
                lcd.WriteBottomText("Book Not Found");
                await Task.Delay(3000);
                lcd.ClearBottomText();
            }
            else
            {
                Console.WriteLine("Error Deleting Book.");
                lcd.WriteBottomText("Error Deleting");
                await Task.Delay(3000);
                lcd.ClearBottomText();
            }
        }
        private static void OnButtonChange(object sender, EventArgs args)
        {
            lcd.WriteTopText(button.state.ToString());
        }
    }
}
