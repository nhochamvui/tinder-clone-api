using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using TinderClone.Authorization.Requirements;
using TinderClone.Services;

namespace TinderClone.Authorization
{
    public class AppAuthorizationHandler : IAuthorizationHandler
    {
        public readonly IUserService _userService;

        public AppAuthorizationHandler(IUserService userService)
        {
            _userService = userService;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.ToList();
            foreach (var require in requirements)
            {
                if(require is GenZRequirement)
                {
                    long userID = Convert.ToInt64(context.User.FindFirst("Id")?.Value);
                    var getProfileTask = _userService.GetProfile(userID);
                    Task.WaitAll(getProfileTask);
                    var profile = getProfileTask.Result;

                    bool isGenz = (require as GenZRequirement).IsGenz(profile.DateOfBirth.Year);
                    if (isGenz)
                    {
                        context.Succeed(require);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
