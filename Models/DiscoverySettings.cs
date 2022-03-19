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

        [Required]
        public long UserID { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public User User { set; get; }

        public string Location { get; set; }

        [Required]
        public int DistancePreference { get; set; }

        [Required]
        public bool DistancePreferenceCheck { get; set; }

        [Required]
        public int LookingForGender { get; set; }

        [Required]
        public int MinAge { get; set; }

        [Required]
        public int MaxAge { get; set; }

        [Required]
        public bool AgePreferenceCheck { get; set; }

        public int LikeCount { get; set; }

        public int SuperlikeCount { get; set; }

        public DiscoverySettings()
        {
            
        }

        public void Update(DiscoverySettingsDTO discoverySettingsDTO)
        {
            Location = discoverySettingsDTO.Location;
            DistancePreference = discoverySettingsDTO.DistancePreference;
            DistancePreferenceCheck = discoverySettingsDTO.DistancePreferenceCheck;
            LookingForGender = discoverySettingsDTO.LookingForGender;
            MinAge = discoverySettingsDTO.MinAge;
            MaxAge = discoverySettingsDTO.MaxAge;
            AgePreferenceCheck = discoverySettingsDTO.AgePreferenceCheck;
        }
    }

}
