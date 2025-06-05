using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Authorization.Requirements
{
    public class GenZRequirement : IAuthorizationRequirement
    {
        public int FromYear { get; }
        public int ToYear { get; }

        public GenZRequirement(int fromYear = 1996, int toYear = 2005)
        {
            FromYear = fromYear;
            ToYear = toYear;
        }

        public bool IsGenz(int year)
        {
            return (year >= FromYear) && (year <= ToYear);
        }
    }
}
