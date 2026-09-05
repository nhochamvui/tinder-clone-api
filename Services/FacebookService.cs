using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Services
{
    public interface IFacebookService
    {
        public Task<bool> IsAccessTokenValid(string facebookAccessToken);
        public Task<FacebookUserData> GetMe(string facebookAccessToken);
    }
    public class FacebookService : IFacebookService
    {
        private const string GraphApiVersion = "v25.0";

        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public FacebookService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<FacebookUserData> GetMe(string facebookAccessToken)
        {
            var result = await _httpClient.GetStringAsync($"https://graph.facebook.com/{GraphApiVersion}/me?fields=" +
                       $"id,email,first_name,last_name,name,locale,picture" +
                   $"&access_token={facebookAccessToken}");
            var data = JsonConvert.DeserializeObject<FacebookUserData>(result,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            return data;
        }

        public async Task<bool> IsAccessTokenValid(string facebookAccessToken)
        {
            var appId = _config["Facebook:AppId"];
            var appSecret = _config["Facebook:AppSecret"];

            var appAccessTokenRes = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?" +
                $"client_id={appId}&client_secret={appSecret}&grant_type=client_credentials");
            FacebookAppAccessToken appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenRes);

            // 2. validate the user access token
            var accessTokenValidationRes = await _httpClient.GetStringAsync($"https://graph.facebook.com/{GraphApiVersion}/debug_token?" +
                $"input_token={facebookAccessToken}&access_token={appAccessToken.AccessToken}");
            var accessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(accessTokenValidationRes,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

            return accessTokenValidation != null && accessTokenValidation.Data != null && accessTokenValidation.Data.IsValid;
        }


    }
}
