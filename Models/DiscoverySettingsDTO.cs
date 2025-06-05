using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TinderClone.Models
{
    [BindProperties]
    public class DiscoverySettingsDTO
    {
        public string Location { get; set; }

        [Required]
        [Range(2, 100)]
        public int DistancePreference { get; set; }

        [Required]
        public bool DistancePreferenceCheck { get; set; }

        [Required]
        public int LookingForGender { get; set; }

        [Required]
        [Range(18, 41)]
        public int MinAge { get; set; }

        [Required]
        [Range(42, 100)]
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
