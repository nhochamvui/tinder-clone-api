using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Infrastructure
{
    public interface IDiscoverySettingsRepository : IRepository<DiscoverySettings>
    {
    }
}
