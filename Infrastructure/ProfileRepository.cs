using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public class ProfileRepository : IProfileRepository
    {
        public readonly TinderContext _dbContext;

        public ProfileRepository(TinderContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddAsync(Profile entity)
        {
            await _dbContext.Profiles.AddAsync(entity);
            return await _dbContext.SaveChangesAsync();
        }

        public Task<Profile> GetProfile(long userID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsExist(long userID)
        {
            return await _dbContext.Profiles.AnyAsync(x => x.UserID == userID);
        }
    }
}
