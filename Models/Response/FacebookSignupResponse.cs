namespace TinderClone.Models.Response
{
    public class FacebookSignupResponse : AbsResponse
    {
        public string AccessToken { get; set; }

        public override string Message { get; set; }

        public override bool IsSuccess { get; set; }

        public FacebookSignupResponse(string message, bool isSuccess, string accessToken)
        {
            Message = message;
            IsSuccess = isSuccess;
            AccessToken = accessToken;
        }
    }
}
