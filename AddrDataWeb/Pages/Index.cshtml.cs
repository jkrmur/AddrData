using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System.Net;
using System.Net.Sockets;
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

        public IActionResult OnPostDelete(string ip)
        {
            var filter = Builders<PacketData>.Filter.Eq(x => x.IP, ip);
            _collection.DeleteOne(filter);

            return RedirectToPage();
        }
        public IActionResult OnPostBlock(string sender, string ip)
        {
            Console.WriteLine("Blocking : " + ip);
            Console.WriteLine(sender);
            var localip = "";
            TempData["Status"] = true;
            IPAddress[] ipAddresses = Dns.GetHostAddresses(sender);
            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    localip = ipAddress.ToString();
                }
            }
            Console.WriteLine(localip);

            //send tcp message to block ip
            var client = new TcpClient();
            client.Connect(localip, 8787);
            var stream = client.GetStream();
            var data = System.Text.Encoding.ASCII.GetBytes(ip);
            stream.Write(data, 0, data.Length);
            stream.Close();
            client.Close();


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