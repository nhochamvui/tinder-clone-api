using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        public Task<bool> IsExist(long userID);
        public Task<int> AddAsync(T entity);
    }
}
