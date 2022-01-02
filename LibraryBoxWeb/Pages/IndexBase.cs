using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Radzen.Blazor;
using MudBlazor;
using LibraryBoxWeb.Models;

namespace LibraryBoxWeb.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private IDialogService DialogService { get; set; }
        public List<Addresses> ListOfAddresses { get; set; }
        public List<Book> ListOfBooks { get; set; }
        public string GoogleMapsApiKey { get; set; }
        public bool Loading { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var configuration = Configuration;            
            string azureFunctionApiKey = configuration["AzureFunctionApiKey"];
            string azureFunctionBaseEndpoint = configuration["AzureFunctionBaseEndpoint"];
            GoogleMapsApiKey = configuration["GoogleMapsApiKey"];
            ListOfAddresses = new List<Addresses>();
            ListOfBooks = new List<Book>();

            Loading = true;
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllAddresses?code={azureFunctionApiKey}");
            JToken AllAddresses = JArray.Parse(response);
            foreach(var address in AllAddresses)
            {
                ListOfAddresses.Add(address.ToObject<Addresses>());
            }

            response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllBooks?code={azureFunctionApiKey}");
            JToken AllBooks = JArray.Parse(response);
            foreach(var book in AllBooks)
            {
                ListOfBooks.Add(book.ToObject<Book>());
            }
            Loading = false;
            await Task.Delay(1000); //Map markers wouldn't show up on initial load.  Needed to wait for something but not sure.... javascript?
            StateHasChanged();
        }

        public async Task OnMarkerClick(RadzenGoogleMapMarker marker)
        {
            string markerClickTitleAddress = marker.Title;
            List<Book> markerClickListOfBooks = ListOfBooks.Where(book => book.Address == markerClickTitleAddress).OrderBy(book => book.Title).ToList();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("markerClickTitleAddress", markerClickTitleAddress);
            dialogParameters.Add("markerClickListOfBooks", markerClickListOfBooks);

            var result = await DialogService.Show<Dialog>(markerClickTitleAddress, dialogParameters).Result;
        }     
    }
}
