using LibraryBoxLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryBoxWeb.Pages
{
    public class IndexBase : ComponentBase
    {        
        
        public List<Book> libraryBoxList = new List<Book>();
        
        protected override async Task OnInitializedAsync()
        {         
        }
    }
}
