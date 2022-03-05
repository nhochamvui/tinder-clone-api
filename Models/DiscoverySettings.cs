using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
   
namespace TinderClone.Models
{
    public class DiscoverySettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public long UserID { get; set; }
        public User User { set; get; }
        
        public string Location { get; set; }

        public int DistancePreference { get; set; }

        public bool DistancePreferenceCheck { get; set; }

        public int LookingForGender { get; set; }

        public int MinAge { get; set; }

        public int MaxAge { get; set; }

        public bool AgePreferenceCheck { get; set; }

        public int LikeCount { get; set; }

        public int SuperlikeCount { get; set; }
    }
}
