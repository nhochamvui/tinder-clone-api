using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TinderClone.Models
{
    public class ProfileDTO
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string Hometown { get; set; }

        public string About { get; set; }

        public long UserID { get; set; }

        public ICollection<string> ProfileImages { get; set; }

        [JsonConstructor]
        public ProfileDTO(string about, string hometown, string dateOfBirth)
        {
            About = about;
            Hometown = hometown;
            DateOfBirth = dateOfBirth;
        }

        public ProfileDTO(Models.Profile profile)
        {
            Name = profile.Name;
            Age = ((DateTime.UtcNow - profile.DateOfBirth).Days / 365);
            DateOfBirth = profile.DateOfBirth.ToShortDateString();
            Gender = Models.User.GetGender(profile.Gender);
            Hometown = profile.Hometown;
            About = profile.About;
            UserID = profile.UserID;
        }
    }
}
