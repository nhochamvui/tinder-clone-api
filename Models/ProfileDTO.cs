using System;
using System.Text.Json.Serialization;

namespace TinderClone.Models
{
    public class ProfileDTO
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string Location { get; set; }

        public string About { get; set; }

        public long UserID { get; set; }

        [JsonConstructor]
        public ProfileDTO(string about, string location, string dateOfBirth)
        {
            About = about;
            Location = location;
            DateOfBirth = dateOfBirth;
        }

        public ProfileDTO(Models.Profile profile)
        {
            Name = profile.Name;
            Age = ((DateTime.UtcNow - profile.DateOfBirth).Days / 365);
            DateOfBirth = profile.DateOfBirth.ToShortDateString();
            Gender = Models.User.GetGender(profile.Gender);
            Location = profile.Location;
            About = profile.About;
            UserID = profile.UserID;
        }
    }
}
