using LibraryBoxWeb.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LibraryBoxWeb.Pages
{
    public class DialogBase : ComponentBase
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }
        [Parameter] public string markerClickTitle { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        public List<Book> ListOfBooks { get; set; }
        public List<Book> markerClickAddressListOfBooks { get; set; }
        public bool finishedLoading { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();            

            var configuration = Configuration;
            string azureFunctionApiKey = configuration["AzureFunctionApiKey"];
            string azureFunctionBaseEndpoint = configuration["AzureFunctionBaseEndpoint"];
            ListOfBooks = new List<Book>();
            markerClickAddressListOfBooks = new List<Book>();
            finishedLoading = false;

            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllBooks?code={azureFunctionApiKey}");
            JToken AllBooks = JArray.Parse(response);
            foreach (var book in AllBooks)
            {
                ListOfBooks.Add(book.ToObject<Book>());
            }

            markerClickAddressListOfBooks = ListOfBooks.Where(book => book.address == markerClickTitle).ToList();
            finishedLoading = true;
        }

        public void CloseDialog()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

    }
}
