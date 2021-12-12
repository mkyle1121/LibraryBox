using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBox
{
    public class Book
    {        
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }        
        public string Author { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public DateTime Date { get; set; }
                  
    }
}
