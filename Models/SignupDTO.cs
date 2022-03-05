using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TinderClone.Models
{
    public class SignupDTO
    {
        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Email { get; set; }

        [JsonConstructor]
        public SignupDTO(string name, DateTime dateOfBirth, int gender, string email)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Email = email;
        }

        public SignupDTO(FacebookUserData facebookUser)
        {
            Name = facebookUser.Name;
            DateOfBirth = DateTime.ParseExact(facebookUser.Birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
            Gender = Models.User.GetGender(facebookUser.Gender ?? "Other");
            Email = facebookUser.Email;
        }

        public SignupDTO()
        {
        }
    }
}
