using Newtonsoft.Json;

namespace LibraryBoxLibrary
{
    public class Book
    {
        public string id { get; set; }
        public string author { get; set; }
        public string title { get; set; }
        public string isbn { get; set; }
        public string date { get; set; }
        public string street { get; set; }
    }
}
