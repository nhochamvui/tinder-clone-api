using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public class UsersRepository : IUsersRepository
    {
        private readonly TinderContext _dbContext;

        public UsersRepository(TinderContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsUserExist(long userID)
        {
            return await _dbContext.Users.AnyAsync(x => x.Id == userID);
        }

        public async Task<Profile> GetProfile(long userID)
        {
            return await _dbContext.Profiles.FirstOrDefaultAsync(x => x.UserID == userID);
        }
    }
}
