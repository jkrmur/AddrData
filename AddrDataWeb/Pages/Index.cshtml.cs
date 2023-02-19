using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System.Xml.Linq;

namespace AddrDataWeb.Pages
{
    public class IndexModel : PageModel
    {

       
        private readonly IMongoCollection<PacketData> _collection;

        public IndexModel(IMongoCollection<PacketData> collection)
        {
            _collection = collection;
        }

        public List<PacketData> Packets { get; set; }
        public void OnGet()
        {
            Packets = _collection.Find(FilterDefinition<PacketData>.Empty).ToList();
            
        }
    }
}