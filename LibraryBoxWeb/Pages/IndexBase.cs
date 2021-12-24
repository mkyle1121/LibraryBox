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
        public string googleMapsApiKey { get; set; }
        public string markerClickTitle { get; set; }
        public List<Book> markerClickAddressListOfBooks { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var configuration = Configuration;            
            string azureFunctionApiKey = configuration["AzureFunctionApiKey"];
            string azureFunctionBaseEndpoint = configuration["AzureFunctionBaseEndpoint"];
            googleMapsApiKey = configuration["GoogleMapsApiKey"];
            ListOfAddresses = new List<Address>();
            

            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllAddresses?code={azureFunctionApiKey}");
            JToken AllAddresses = JArray.Parse(response);
            foreach (var address in AllAddresses)
            {
                ListOfAddresses.Add(address.ToObject<Address>());
            }
        }

        public async Task OnMarkerClick(RadzenGoogleMapMarker marker)
        {
            markerClickTitle = marker.Title;
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("markerClickTitle", markerClickTitle);
            var result = await DialogService.Show<Dialog>(markerClickTitle, dialogParameters).Result;
        }     
    }
}
