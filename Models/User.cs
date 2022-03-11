using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TinderClone.Models
{
    enum Sex
    {
        Male,
        Female,
        Other
    }
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string Email { get; set; }

        public string About { get; set; }

        public string Location { get; set; }

        //navigation
        public Profile Profile { get; set; }

        public static string GetGender(int sex)
        {
            switch (sex)
            {
                case ((int)Sex.Female):
                    return Sex.Female.ToString();
                case ((int)Sex.Male):
                    return Sex.Male.ToString();
                case ((int)Sex.Other):
                    return Sex.Other.ToString();
                default:
                    return string.Empty;
            }
        }

        public static int GetGender(string sex)
        {
            Sex.Male.ToString();
            if (sex.Equals(Sex.Male.ToString()))
            {
                return (int)Sex.Male;
            }
            else if (sex.Equals(Sex.Female.ToString()))
            {
                return (int)Sex.Female;
            }
            else
            {
                return (int)Sex.Other;
            }
        }

        public static int GetGender2(string sex)
        {
            if (sex.Equals(Sex.Male.ToString().ToLower()))
            {
                return (int)Sex.Male;
            }
            else if (sex.Equals(Sex.Female.ToString().ToLower()))
            {
                return (int)Sex.Female;
            }
            else
            {
                return (int)Sex.Other;
            }
        }

        public static int GetAge(DateTime dateOfBirth)
        {
            return (DateTime.UtcNow - dateOfBirth).Days / 365;
        }
    }
}
