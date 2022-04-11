using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public interface IUsersRepository : IRepository<User>
    {
        public Task<Profile> GetProfile(long userID);
    }
}
