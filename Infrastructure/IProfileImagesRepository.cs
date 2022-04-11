using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public interface IProfileImagesRepository : IRepository<ProfileImages>
    {
        public Task<int> CountMyImagesAsync(long myProfileID);
    }
}
