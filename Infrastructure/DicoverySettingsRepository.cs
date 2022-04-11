using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public class DiscoverySettingsRepository : IDiscoverySettingsRepository
    {
        private readonly TinderContext _dbContext;

        public DiscoverySettingsRepository(TinderContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsExist(long userID)
        {
            return await _dbContext.DiscoverySettings.AnyAsync(x => x.UserID == userID);
        }

        public async Task<int> AddAsync(DiscoverySettings entity)
        {
            await _dbContext.DiscoverySettings.AddAsync(entity);
            return await _dbContext.SaveChangesAsync();
        }
    }
}
