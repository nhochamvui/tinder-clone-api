using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models.RequestParam
{
    public class DiscoverFilter
    {
        public int Id { get; set; }

        public int Gender { get; set; }

        public string Location { get; set; }

        public int Distance { get; set; }

        public int minAge { get; set; }

        public int maxAge { get; set; }
    }
}
