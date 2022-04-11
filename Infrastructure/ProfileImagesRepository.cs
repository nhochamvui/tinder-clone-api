using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public class ProfileImagesRepository : IProfileImagesRepository
    {
        private readonly TinderContext _dbContext;

        public ProfileImagesRepository(TinderContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsExist(long profileID)
        {
            return await _dbContext.ProfileImages.AnyAsync(x => x.ProfileID == profileID);
        }

        public async Task<int> AddAsync(ProfileImages profileImage)
        {
            await _dbContext.ProfileImages.AddAsync(profileImage);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountMyImagesAsync(long myProfileID)
        {
            return await _dbContext.ProfileImages.Where(s => s.ProfileID == myProfileID).CountAsync();
        }
    }
}
