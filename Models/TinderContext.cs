using Microsoft.EntityFrameworkCore;
using System;

namespace TinderClone.Models
{
    public class TinderContext : DbContext
    {
        public TinderContext(DbContextOptions<TinderContext> options) : base(options)
        {
        }

        public TinderContext()
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<ProfileImages> ProfileImages { get; set; }

        public DbSet<Matches> Matches { get; set; }

        public DbSet<Messages> Messages { get; set; }

        public DbSet<DiscoverySettings> DiscoverySettings { get; set; }

        public DbSet<Profile> Profiles { get; set; }

        public DbSet<UserRoles> UserRoles { get; set; }

        public DbSet<Role> Roles { get; set; }

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
            }, new User
            {
                Id = 2,
                UserName = "auntbob",
                Password = "1234",
            });

            modelBuilder.Entity<DiscoverySettings>().HasData(new DiscoverySettings
            {
                Id = 1,
                UserID = 1,
                Location = "Hồ Chí Minh",
                DistancePreference = 1,
                LookingForGender = Profile.ParseGender("Female"),
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
                LookingForGender = Profile.ParseGender("Male"),
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
                Gender = Profile.ParseGender("Male"),
                Location = "Hồ Chí Minh",
                Hometown = "Cần Thơ",
                Name = "Tho",
                Phone = "0907904598",
                UserID = 1,
                Latitude = "10.0371100",
                Longitude = "105.7882500",
            });
            modelBuilder.Entity<Profile>().HasData(new Profile
            {
                Id = 2,
                About = "",
                DateOfBirth = new DateTime(1998, 1, 29),
                Email = "a1@gmail.com",
                Gender = Profile.ParseGender("Female"),
                Location = "Hồ Chí Minh",
                Hometown = "Cần Thơ",
                Name = "Jan",
                Phone = "0907904598",
                UserID = 2,
                //Latitude = "10.8142",
                //Longitude = "106.6438",
                Latitude = "10.045783",
                Longitude = "105.761412",
            });

            modelBuilder.Entity<ProfileImages>().HasData(new ProfileImages
            {
                Id = 1,
                ImageURL = "https://i.ibb.co/VYgMyVd/217772307-360659078758844-3269291223653109900-n.jpg",
                ProfileID = 1,
            }, new ProfileImages
            {
                Id = 2,
                ImageURL = "https://i.ibb.co/6mYstg7/273538889-1378020902629820-5496867161341207743-n.jpg",
                ProfileID = 2,
            });
        }
    }
}
