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
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "tinder-clone-api/1.0");
        }

        public double GetDistance(Coordinate point1, Coordinate point2)
        {
            if (!double.TryParse(point1?.Latitude, out var lat1) ||
                !double.TryParse(point1?.Longitude, out var lon1) ||
                !double.TryParse(point2?.Latitude, out var lat2) ||
                !double.TryParse(point2?.Longitude, out var lon2))
            {
                return double.MaxValue;
            }

            const double EARTH_RADIUS = 6376500.0;

            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = lon1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = lon2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            var distance = (EARTH_RADIUS * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)))) / 1000;
            Console.WriteLine($"[{point1.Latitude}, {point1.Longitude}], [{point2.Latitude}, {point2.Longitude}]: {distance}");
            return distance;
        }

        public async Task<GeoPluginResponse> GetLocation(string ip)
        {
            try
            {
                string ipAddress = ip?.Split(',')[0].Trim();
                var result = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=status,message,country,regionName,city,lat,lon,query");
                var location = JsonConvert.DeserializeObject<GeoPluginResponse>(result,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

                if (location == null)
                {
                    return new GeoPluginResponse();
                }

                if (location.Status != "success")
                {
                    Console.WriteLine($"GetLocation: ip-api returned '{location.Status}': {location.Message}");
                    return new GeoPluginResponse();
                }

                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLocation: failed to resolve location: " + ex.Message);
                return new GeoPluginResponse();
            }
        }
    }
}
