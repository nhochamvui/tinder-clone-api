
using Newtonsoft.Json;

namespace TinderClone.Models
{
    public class Data
    {
        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("delete_url")]
        public string DeleteUrl { get; set; }
    }
    public class ImgBBResponse
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
