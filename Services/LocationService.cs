using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Services
{
    public class Coordinate
    {
        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }

    public interface ILocationService
    {
        public double GetDistance(Coordinate point1, Coordinate point2);

        public Task<GeoPluginResponse> GetLocation(string ip);
    }
    public class LocationService : ILocationService
    {
        private TinderContext _dbContext;
        private IConfiguration _config;
        private HttpClient _httpClient;
        public LocationService(TinderContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
            _httpClient = new HttpClient();
        }

        public double GetDistance(Coordinate point1, Coordinate point2)
        {
            const double EARTH_RADIUS = 6376500.0;

            var d1 = double.Parse(point1.Latitude) * (Math.PI / 180.0);
            var num1 = double.Parse(point1.Longitude) * (Math.PI / 180.0);
            var d2 = double.Parse(point2.Latitude) * (Math.PI / 180.0);
            var num2 = double.Parse(point2.Longitude) * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            var distance = (EARTH_RADIUS * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)))) / 1000;
            Console.WriteLine($"[{point1.Latitude}, {point1.Longitude}], [{point2.Latitude}, {point2.Longitude}]: {distance}");
            return distance;
        }

        public async Task<GeoPluginResponse> GetLocation(string ip)
        {
            var result = await _httpClient.GetStringAsync("http://www.geoplugin.net/json.gp?ip=" + ip);
            var location = JsonConvert.DeserializeObject<GeoPluginResponse>(result.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            return location;
        }
    }
}
