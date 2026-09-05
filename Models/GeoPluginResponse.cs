
using Newtonsoft.Json;

namespace TinderClone.Models
{
    public class GeoPluginResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("query")]
        public string RequestIP { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("regionName")]
        public string RegionName { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("lat")]
        public string Latitude { get; set; }

        [JsonProperty("lon")]
        public string Longtitude { get; set; }
    }
}
