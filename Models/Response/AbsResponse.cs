using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models.Response
{
    public abstract class AbsResponse
    {
        public abstract string Message { get; set; }

        public abstract bool IsSuccess { get; set; }
        public AbsResponse(){}
    }
}
