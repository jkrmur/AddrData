
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;



namespace AddrDataWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InfoController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public InfoController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }


        public class Response
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public string regional_internet_registry { get; set; }
            public string jarm { get; set; }
            public string network { get; set; }
            public long last_https_certificate_date { get; set; }
            public List<string> tags { get; set; }
            public List<CrowdsourcedContext> crowdsourced_context { get; set; }
            public string country { get; set; }
            public long last_analysis_date { get; set; }
            public string as_owner { get; set; }
            public LastAnalysisStats last_analysis_stats { get; set; }
            public int asn { get; set; }
            public long whois_date { get; set; }
           // public Dictionary<string, AnalysisResult> last_analysis_results { get; set; }
        }

        public class CrowdsourcedContext
        {
            public string source { get; set; }
            public long timestamp { get; set; }
            public string detail { get; set; }
            public string severity { get; set; }
            public string title { get; set; }
        }

        public class LastAnalysisStats
        {
            public int harmless { get; set; }
            public int malicious { get; set; }
            public int suspicious { get; set; }
            public int undetected { get; set; }
            public int timeout { get; set; }
        }

        public class AnalysisResult
        {
            public string category { get; set; }
            public string result { get; set; }
            public string method { get; set; }
            public string engine_name { get; set; }
        }

        [HttpGet("{ip}")]
        public async Task<IActionResult> Get(string ip)
        {
            // Make the HTTP request to the VirusTotal API
            var apiKey = "c25594a277e9143407bc57d1bb89d1c5dc94cc5d930d5eecbbeaf69a83c18c39";
            var url = $"https://www.virustotal.com/api/v3/ip_addresses/{ip}";
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-apikey", apiKey); // API key header
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<Response>(jsonString);
                return Ok(json);
            }

            return BadRequest();

        }
    }
}

