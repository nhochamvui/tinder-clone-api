using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public interface IProfileRepository : IRepository<Profile>
    {
        public Task<Profile> GetProfile(long userID);
    }
}
