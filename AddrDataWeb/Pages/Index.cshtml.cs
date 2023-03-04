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

        public IActionResult OnPost(string ip)
        {
            var filter = Builders<PacketData>.Filter.Eq(x => x.IP, ip);
            _collection.DeleteOne(filter);

            return RedirectToPage();
        }

       
        public List<PacketData> Packets { get; set; }
        public void OnGet()
        {
           // Packets = _collection.Find(FilterDefinition<PacketData>.Empty).ToList();        
                try
                {
                    Packets = _collection.Find(FilterDefinition<PacketData>.Empty).ToList();

                    if (Packets == null)
                    {
                        throw new Exception("Packets is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving packet data: " + ex.Message);
                    Packets = new List<PacketData>();
                }
         }




    }
}