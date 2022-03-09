using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Services
{
    public interface IFacebookService
    {
        Task<bool> IsAccessTokenValid(string facebookAccessToken);
        Task<string> GetMe(string facebookAccessToken);
    }
    public class FacebookService : IFacebookService
    {
        private TinderContext _dbContext;
        private IConfiguration _config;
        private HttpClient _httpClient;
        public FacebookService(TinderContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetMe(string facebookAccessToken)
        {
            var result =  await _httpClient.GetStringAsync($"https://graph.facebook.com/v13.0/me?fields=" +
                       $"id,email,first_name,last_name,name,gender,locale,birthday,picture" +
                   $"&access_token={facebookAccessToken}");
            return result.ToString();
        }

        public async Task<bool> IsAccessTokenValid(string facebookAccessToken)
        {
            bool isValid = true;
            var appAccessTokenRes = await _httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?" +
                $"client_id=591690891823251&client_secret=4143b070cc7f6e80258e440c14fa35aa&grant_type=client_credentials");
            FacebookAppAccessToken appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenRes);

            // 2. validate the user access token
            var accessTokenValidationRes = await _httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?" +
                $"input_token={facebookAccessToken}&access_token={appAccessToken.AccessToken}");
            var accessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(accessTokenValidationRes,

                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

            // 3. we've got a valid token so we can request user data from fb
            if (!accessTokenValidation.Data.IsValid)
            {
                isValid = false;
            }

            return isValid;
        }


    }
}
