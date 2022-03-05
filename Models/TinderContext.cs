using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models
{
    public class TinderContext : DbContext
    {
        public TinderContext(DbContextOptions<TinderContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<ProfileImages> ProfileImages { get; set; }

        public DbSet<Matches> Matches { get; set; }

        public DbSet<Messages> Messages { get; set; }

        public DbSet<DiscoverySettings> DiscoverySettings { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DiscoverySettings>()
            .Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Messages>().HasOne<User>().WithMany().HasForeignKey(m => m.toID);
            modelBuilder.Entity<Messages>().HasOne<User>().WithMany().HasForeignKey(m => m.fromID);
            modelBuilder.Entity<User>().HasOne<Profile>(u => u.Profile).WithOne(p => p.User).HasForeignKey<Profile>(p => p.UserID);

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                UserName = "unclebob",
                Password = "1234",
                Name = "Uncle Bob",
                Email = "uncle.bob@gmail.com",
                DateOfBirth = new DateTime(1998, 04, 25),
                Location = "Hồ Chí Minh",
                Gender = (int)Sex.Male
            }, new User
            {
                Id = 2,
                UserName = "auntbob",
                Password = "1234",
                Name = "Aunt Bob",
                Email = "aunt.bob@gmail.com",
                DateOfBirth = new DateTime(1999, 04, 25),
                Gender = (int)Sex.Female,
                Location = "Cần Thơ"
            });
            modelBuilder.Entity<ProfileImages>().HasData(new ProfileImages
            {
                Id = 1,
                ImageURL = "img/unclebob.jpg",
                UserID = 1,
            }, new ProfileImages
            {
                Id = 2,
                ImageURL = "img/unclebob1.jpg",
                UserID = 1,
            }, new ProfileImages
            {
                Id = 3,
                ImageURL = "img/auntbob.jpg",
                UserID = 2,
            });
            modelBuilder.Entity<DiscoverySettings>().HasData(new DiscoverySettings
            {
                Id = 1,
                UserID = 1,
                Location = "Hồ Chí Minh",
                DistancePreference = 1,
                LookingForGender = Models.User.GetGender("Female"),
                MinAge = 18,
                MaxAge = 100,
                AgePreferenceCheck = true,
                DistancePreferenceCheck = true,
                LikeCount = 100,
                SuperlikeCount = 4,
            });
            modelBuilder.Entity<DiscoverySettings>().HasData(new DiscoverySettings
            {
                Id = 2,
                UserID = 2,
                Location = "Hồ Chí Minh",
                DistancePreference = 1,
                LookingForGender = Models.User.GetGender("Male"),
                MinAge = 18,
                MaxAge = 100,
                AgePreferenceCheck = true,
                DistancePreferenceCheck = true,
                LikeCount = 100,
                SuperlikeCount = 4,
            });
            modelBuilder.Entity<Profile>().HasData(new Profile
            {
                Id = 1,
                About = "",
                DateOfBirth = new DateTime(1998, 1, 29),
                Email = "a@gmail.com",
                Gender = User.GetGender("Male"),
                Location = "Hồ Chí Minh",
                Name = "Tho",
                Phone = "0907904598",
                UserID = 1,
            }) ;
            modelBuilder.Entity<Profile>().HasData(new Profile
            {
                Id = 2,
                About = "",
                DateOfBirth = new DateTime(1998, 1, 29),
                Email = "a1@gmail.com",
                Gender = User.GetGender("Female"),
                Location = "Hồ Chí Minh",
                Name = "Jan",
                Phone = "0907904598",
                UserID = 2,
            });
        }
    }
}
