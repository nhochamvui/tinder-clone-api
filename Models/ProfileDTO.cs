﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TinderClone.Models
{
    public class ProfileDTO
    {
        [StringLength(25, MinimumLength = 1)]
        public string? Name { get; set; }

        [Range(18, 100)]
        public int? Age { get; set; }

        public string DateOfBirth { get; set; }

        public string Gender { get; set; }

        [StringLength(20, MinimumLength = 0)]
        public string? Hometown { get; set; }

        [StringLength(100, MinimumLength = 0)]
        public string? About { get; set; }

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
            Gender = Profile.ParseGender(profile.Gender);
            Hometown = profile.Hometown;
            About = profile.About;
            UserID = profile.UserID;
        }
    }
}
