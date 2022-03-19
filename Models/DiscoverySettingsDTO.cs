using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
   
namespace TinderClone.Models
{
    [BindProperties]
    public class DiscoverySettingsDTO
    {
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

        [JsonConstructor]
        public DiscoverySettingsDTO(string location, int distancePreference, bool distancePreferenceCheck, int lookingForGender, int minAge, int maxAge, bool agePreferenceCheck)
        {
            Location = location;
            DistancePreference = distancePreference;
            DistancePreferenceCheck = distancePreferenceCheck;
            LookingForGender = lookingForGender;
            MinAge = minAge;
            MaxAge = maxAge;
            AgePreferenceCheck = agePreferenceCheck;
        }

        public DiscoverySettingsDTO(DiscoverySettings discoverySettings)
        {
            Location = discoverySettings.Location;
            DistancePreference = discoverySettings.DistancePreference;
            DistancePreferenceCheck = discoverySettings.DistancePreferenceCheck;
            LookingForGender = discoverySettings.LookingForGender;
            MinAge = discoverySettings.MinAge;
            MaxAge = discoverySettings.MaxAge;
            AgePreferenceCheck = discoverySettings.AgePreferenceCheck;
        }
    }
}
