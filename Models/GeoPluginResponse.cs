
using Newtonsoft.Json;

namespace TinderClone.Models
{
    public class GeoPluginResponse
    {
        [JsonProperty("geoplugin_request")]
        public string RequestIP { get; set; }

        [JsonProperty("geoplugin_city")]
        public string City { get; set; }

        [JsonProperty("geoplugin_regionName")]
        public string RegionName { get; set; }

        [JsonProperty("geoplugin_countryName")]
        public string Country { get; set; }

        [JsonProperty("geoplugin_latitude")]
        public string Latitude { get; set; }

        [JsonProperty("geoplugin_longitude")]
        public string Longtitude { get; set; }
    }
}
