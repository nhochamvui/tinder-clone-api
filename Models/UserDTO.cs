using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models
{
    public class UserDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Age { get; set; }

        public string Gender { get; set; }

        public string Location { get; set; }

        public string About { get; set; }

        public List<string> ProfileImages { get; set; }

        public UserDTO(TinderContext context, User user)
        {
            this.Id = user.Id;
            this.Name = user.Name;
            this.DateOfBirth = user.DateOfBirth;
            this.Age = ((DateTime.UtcNow - user.DateOfBirth).Days / 365).ToString();
            this.Gender = Models.User.GetGender(user.Gender);
            this.About = user.About;
            this.Location = user.Location;
            this.ProfileImages = GetProfileImages(context, user.Id);
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

        //public static string GetStringProfileImages(TinderContext context, long userID)
        //{
        //    string results = null;
        //    var test = context.ProfileImages.Where(x => x.UserID == userID).Select(x => x.ImageURL).ToArray();

        //    foreach (var item in test)
        //    {
        //        results = ";" + item;
        //    }
        //    return results;
        //}

        //public static List<string> GetProfileImages(IQueryable<ProfileImages> profileImages, long userID)
        //{
        //    List<string> results = new List<string>();
        //    var test = profileImages.Where(x => x.UserID == userID).Select(x => x.ImageURL).ToArray();

        //    foreach (var item in test)
        //    {
        //        results.Add(item);
        //    }
        //    return results;
        //}
    }
}
