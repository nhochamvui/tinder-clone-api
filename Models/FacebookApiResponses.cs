
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TinderClone.Models
{
    public class FacebookUserData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Locale { get; set; }

        public string Birthday { get; set; }

        public IFormFile photo { get; set; }

        public FacebookPictureData Picture { get; set; }

        public string AccessToken { get; set; }
    }

    public class FacebookRequiredData
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }

        public FacebookRequiredData(FacebookUserData facebookUserData)
        {
            Email = facebookUserData.Email;
            Name = facebookUserData.Name;
            Gender = facebookUserData.Gender;
            Birthday = facebookUserData.Birthday;
        }
    }

    public class FacebookPictureData
    {
        public FacebookPicture Data { get; set; }
    }

    public class FacebookPicture
    {
        public int Height { get; set; }
        public int Width { get; set; }
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }
        public string Url { get; set; }
    }

    internal class FacebookUserAccessTokenData
    {
        [JsonProperty("app_id")]
        public long AppId { get; set; }
        public string Type { get; set; }
        public string Application { get; set; }
        [JsonProperty("expires_at")]
        public long ExpiresAt { get; set; }
        [JsonProperty("is_valid")]
        public bool IsValid { get; set; }
        [JsonProperty("user_id")]
        public long UserId { get; set; }
    }

    internal class FacebookUserAccessTokenValidation
    {
        public FacebookUserAccessTokenData Data { get; set; }
    }

    internal class FacebookAppAccessToken
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class FacebookAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
