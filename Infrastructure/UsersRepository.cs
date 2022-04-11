﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

        public async Task<bool> IsExist(long userID)
        {
            return await _dbContext.Users.AnyAsync(x => x.Id == userID);
        }

        public async Task<Profile> GetProfile(long userID)
        {
            return await _dbContext.Profiles.FirstOrDefaultAsync(x => x.UserID == userID);
        }

        public async Task<int> AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
