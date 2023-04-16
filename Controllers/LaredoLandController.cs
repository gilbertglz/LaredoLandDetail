using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Web;

namespace LaredoLandAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class LaredoLandController : ControllerBase
    {
        private readonly string _csvFilePath = "C:\\Users\\gilbe\\OneDrive\\Desktop\\Scripts\\LaredoLandAPI\\LaredoLandDetailsClean.csv";

        private readonly ILogger<LaredoLandController> _logger;

        public LaredoLandController(ILogger<LaredoLandController> logger)
        {
            _logger = logger;
        }
        private string GetGoogleMapsUrl(string address)
        {
            string formattedAddress = HttpUtility.UrlEncode(address);
            return $"https://www.google.com/maps?q={formattedAddress}";
        }

        [HttpGet(Name = "GetLaredoLand")]
        public IEnumerable<Dictionary<string, string>> Get(int Lot, int Block, string Subdivision)
        {
            var csvLines = System.IO.File.ReadAllLines(_csvFilePath);
            Subdivision = HttpUtility.UrlDecode(Subdivision);
            Subdivision = Subdivision.ToUpper();
            var headers = csvLines.First().Split(','); // Assumes first row is header row
            var records = csvLines.Skip(1) // Skip header row
                                   .Select(l => l.Split(',')) // split by columns
                                   .Select(l => headers.Zip(l, (h, v) => new { h, v })
                                                       .ToDictionary(x => x.h, x => x.v))
                                   .Where(d => d["LOT"] == Lot.ToString() && d["BLOCK"] == Block.ToString() && d["SUBDIVISIO"]==Subdivision) // filter by Lot and Block
                                   .Select(d => new Dictionary<string, string>
                                   {
                                       { "FULLADDRESS", d["FULLADDRES"] }
                                   });
            //we are given a [{FULLADDRESS}] we need a to now pass an url to googlemaps
            List<Dictionary<string, string>> updatedRecords = new List<Dictionary<string, string>>();
            foreach (var record in records)
            {
                string googleMapsUrl = GetGoogleMapsUrl(record["FULLADDRESS"]);
                Dictionary<string, string> updatedRecord = new Dictionary<string, string>();
                updatedRecord.Add("FULLADDRESS", record["FULLADDRESS"]);
                updatedRecord.Add("GOOGLEMAPSURL", googleMapsUrl);
                updatedRecords.Add(updatedRecord);
            }
            return updatedRecords;
        }

    }
}
