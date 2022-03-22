using Microsoft.AspNetCore.SignalR;

namespace TinderClone.Singleton
{
    public class UserIDProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.GetHttpContext().User.FindFirst("Id")?.Value;
        }
    }
}
