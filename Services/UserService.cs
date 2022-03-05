using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetByID(string id);
        User Create(User user, string password);
        void Update(User user, string password = null);
        void Delete(int id);
        Task<Result> CreateFromFB(FacebookUserData facebookUserData);
        Profile CreateFromFB(SignupDTO signupDTO, long userID);
        Task<string> GetToken(long userID);
    }
    public class UserService : IUserService
    {
        private TinderContext _dbContext;
        private IConfiguration _config;
        public UserService(TinderContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }
        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }
            var user = _dbContext.Users.SingleOrDefault(x => x.UserName.Equals(username));

            if (user == null)
            {
                return null;
            }

            if (!VerifyPassword(password, user.Password))
            {
                return null;
            }

            return user;
        }

        private static bool VerifyPassword(string password, string userPassword)
        {
            return password.Equals(userPassword);
        }

        public User Create(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ApplicationException("Password is required");
            }

            if (_dbContext.Users.Any(x => x.UserName.Equals(user.UserName)))
            {
                throw new ApplicationException("User is exist");
            }

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        public async Task<Result> CreateFromFB(FacebookUserData facebookUserData)
        {
            var user = new User
            {
                Id = facebookUserData.Id,
                DateOfBirth = new DateTime(),
                Email = facebookUserData.Email,
                Location = facebookUserData.Locale,
                Name = facebookUserData.Name
            };

            if (await _dbContext.Users.AnyAsync(x => x.Id == facebookUserData.Id))
            {
                return new Result { IsSuccess = false, Error = "User is exist" };
            }

            var profile = new Profile(new SignupDTO(facebookUserData), user.Id);

            if (await _dbContext.Profiles.AnyAsync(x => x.UserID == user.Id))
            {
                return new Result { IsSuccess = false, Error = "User is exist" };
            }

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Profiles.AddAsync(profile);
            await _dbContext.SaveChangesAsync();

            if (!await _dbContext.DiscoverySettings.AnyAsync(s => s.UserID == user.Id))
            {
                await _dbContext.DiscoverySettings.AddAsync(new DiscoverySettings
                {
                    AgePreferenceCheck = false,
                    DistancePreference = 2,
                    DistancePreferenceCheck = false,
                    LikeCount = 30,
                    Location = user.Location,
                    LookingForGender = TinderClone.Models.User.GetGender("Other"),
                    MaxAge = 100,
                    MinAge = 18,
                    SuperlikeCount = 3,
                    UserID = user.Id
                });
            }
            await _dbContext.SaveChangesAsync();

            int profileImagesCount = _dbContext.ProfileImages.Where(s => s.UserID == user.Id).Count();
            if (profileImagesCount < 6)
            {
                for (int i = profileImagesCount; i < 6; i++)
                {
                    await _dbContext.ProfileImages.AddAsync(new ProfileImages
                    {
                        UserID = user.Id,
                        ImageURL = "",
                    });
                }
                await _dbContext.SaveChangesAsync();
            }

            return new Result { IsSuccess = true, Error = null };
        }

        public Profile CreateFromFB(SignupDTO signupDTO, long userID)
        {
            var profile = new Profile(signupDTO, userID);

            if (_dbContext.Profiles.Any(x => x.UserID == userID))
            {
                throw new ApplicationException("User is exist");
            }

            _dbContext.Profiles.Add(profile);
            _dbContext.SaveChanges();

            return profile;
        }

        async public Task<string> GetToken(long userID)
        {
            if (await _dbContext.Users.AnyAsync(x => x.Id == userID))
            {
                var userClaims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("id", userID.ToString()),
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var signinCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var jwtToken = new JwtSecurityToken(_config["Jwt:Issuer"],
                                                    _config["Jwt:Audience"],
                                                    userClaims,
                                                    expires: DateTime.UtcNow.AddDays(1),
                                                    signingCredentials: signinCredential);

                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return string.Empty;
        }

        public void Delete(int id)
        {
            var user = _dbContext.Users.Find(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _dbContext.Users;
        }

        public void Update(User userParam, string password = null)
        {
        }

        public User GetByID(string id)
        {
            return _dbContext.Users.Find(id);
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
    }
}
