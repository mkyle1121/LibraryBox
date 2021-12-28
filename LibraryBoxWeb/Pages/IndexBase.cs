using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
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
        public List<Address> ListOfAddresses { get; set; }
        public List<Book> ListOfBooks { get; set; }
        public string googleMapsApiKey { get; set; }
        public bool Loading { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var configuration = Configuration;            
            string azureFunctionApiKey = configuration["AzureFunctionApiKey"];
            string azureFunctionBaseEndpoint = configuration["AzureFunctionBaseEndpoint"];
            googleMapsApiKey = configuration["GoogleMapsApiKey"];
            ListOfAddresses = new List<Address>();
            ListOfBooks = new List<Book>();

            Loading = true;
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllAddresses?code={azureFunctionApiKey}");
            JToken AllAddresses = JArray.Parse(response);
            foreach(var address in AllAddresses)
            {
                ListOfAddresses.Add(address.ToObject<Address>());
            }

            response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllBooks?code={azureFunctionApiKey}");
            JToken AllBooks = JArray.Parse(response);
            foreach(var book in AllBooks)
            {
                ListOfBooks.Add(book.ToObject<Book>());
            }
            Loading = false;
        }

        public async Task OnMarkerClick(RadzenGoogleMapMarker marker)
        {

            string markerClickTitleAddress = marker.Title;
            List<Book> markerClickListOfBooks = ListOfBooks.Where(book => book.address == markerClickTitleAddress).OrderBy(book => book.title).ToList();

            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("markerClickTitleAddress", markerClickTitleAddress);
            dialogParameters.Add("markerClickListOfBooks", markerClickListOfBooks);

            var result = await DialogService.Show<Dialog>(markerClickTitleAddress, dialogParameters).Result;
        }     
    }
}
