using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public interface IUsersRepository
    {
        public Task<bool> IsUserExist(long userID);
        public Task<Profile> GetProfile(long userID);
    }
}
