using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace TinderClone.Models
{
    public class Profile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Email { get; set; }

        public string About { get; set; }

        public string Hometown { get; set; }

        public string Location { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public string Phone { get; set; }

        //navigation prop
        public long UserID { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public User User { get; set; }

        //navigation
        public ICollection<ProfileImages> ProfileImages { get; set; }

        public Profile() { }

        public Profile(long id, string name, DateTime dateOfBirth, int gender, string email, string about, string location, string phone, long userID)
        {
            Id = id;
            Name = name;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            About = about;
            Location = location;
            Phone = phone;
            UserID = userID;
        }

        public Profile(string name, DateTime dateOfBirth, int gender, string email, long userID)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
            UserID = userID;

        }

        public Profile(string about, string location, DateTime dateOfBirth)
        {
            About = about;
            Location = location;
            DateOfBirth = dateOfBirth;
        }

        public Profile(SignupDTO signupDTO, long userID)
        {
            Name = signupDTO.Name;
            DateOfBirth = signupDTO.DateOfBirth;
            Gender = signupDTO.Gender;
            Email = signupDTO.Email;
            UserID = userID;
        }

        public static List<string> GetProfileImages(TinderContext context, long profileID)
        {
            List<string> results = new();
            var test = context.ProfileImages
                       .Where(x => x.ProfileID == profileID)
                       .Select(x => new { x.ImageURL, x.Id })
                       .OrderByDescending(x => x.Id).Reverse().ToArray();

            foreach (var item in test)
            {
                results.Add(item.ImageURL);
            }
            return results;
        }
    }
}
