
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#region API classes for deserialization
class VirusTotal
{

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
}
class AbuseIPDB
{
    public class Response
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public int total { get; set; }
        public int page { get; set; }
        public int count { get; set; }
        public int perPage { get; set; }
        public int lastPage { get; set; }
        public object nextPageUrl { get; set; }
        public object previousPageUrl { get; set; }
        public List<Result> results { get; set; }
    }

    public class Result
    {
        public string reportedAt { get; set; }
        public string comment { get; set; }
        public List<int> categories { get; set; }
        public int reporterId { get; set; }
        public string reporterCountryCode { get; set; }
        public string reporterCountryName { get; set; }
    }
}
#endregion


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

      
        [HttpGet("{ip}")]
        public async Task<IActionResult> Get(string ip)
        {

            //Query APIs
            VirusTotal.Response total = await QueryFunc<VirusTotal.Response>(url: $"https://www.virustotal.com/api/v3/ip_addresses/{ip}",
                                      apikey: "c25594a277e9143407bc57d1bb89d1c5dc94cc5d930d5eecbbeaf69a83c18c39",
                                      header: "x-apikey");

            AbuseIPDB.Response abuse =  await QueryFunc<AbuseIPDB.Response>(url: $"https://api.abuseipdb.com/api/v2/reports?ipAddress={ip}",
                                       apikey: "86a9ee610b400c5be869343dfd079d0ec54a6d46ffde6ddcdc5d8f470ef290eb534b9289eb109b05",
                                       header: "Key");
            
            


            //combine results and return w/ OkObjectResult aka OK answer code 200 
            return Ok(new { VirusTotal = total, AbuseIPDB = abuse });
        }

       

        //Deserialization class, url, api, headers
        public async Task<T> QueryFunc<T>(string url, string apikey, string header)
        {
            T result = default;
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add(header, apikey);
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<T>(jsonString);
            }
            return result;
        }

       





    }
}

