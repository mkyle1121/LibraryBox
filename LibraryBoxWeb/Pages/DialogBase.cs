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
        [Parameter] public List<Book> markerClickListOfBooks { get; set; }
        [Parameter] public string markerClickTitleAddress { get; set; }        
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();            
        }

        public void CloseDialog()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

    }
}
