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

namespace LibraryBoxWeb.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private IDialogService DialogService { get; set; }
        public JToken AllBooks { get; set; }
        public string googleMapsApiKey { get; set; }
        protected override async Task OnInitializedAsync()
        {
            var configuration = Configuration;
            string azureFunctionApiKey = configuration["AzureFunctionApiKey"];
            string azureFunctionBaseEndpoint = configuration["AzureFunctionBaseEndpoint"];
            googleMapsApiKey = configuration["GoogleMapsApiKey"];

            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync($"{azureFunctionBaseEndpoint}GetAllBooks?code={azureFunctionApiKey}");
            AllBooks = JArray.Parse(response);
        }

        public async Task OnMarkerClick(RadzenGoogleMapMarker marker)
        {            
            bool? result = await DialogService.ShowMessageBox("13300 Wisterwood St.", AllBooks.ToString(), yesText: "Ok!");
            StateHasChanged();
        }
    }
}
