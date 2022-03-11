﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
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
        Task<Result> CreateFromFB(FacebookUserData facebookUserData, GeoPluginResponse location);
        Profile CreateFromFB(SignupDTO signupDTO, long userID);
        Task<string> GetToken(long userID);
        Task<GeoPluginResponse> GetLocation(string ip);

        public Task<ImgBBResponse> UploadIMGBB(IFormFile photo);
    }
    public class UserService : IUserService
    {
        private TinderContext _dbContext;
        private IConfiguration _config;
        private HttpClient _httpClient;

        public UserService(TinderContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<GeoPluginResponse> GetLocation(string ip)
        {
            var result = await _httpClient.GetStringAsync("http://www.geoplugin.net/json.gp?ip="+ip);
            var location = JsonConvert.DeserializeObject<GeoPluginResponse>(result.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            return location;
        }

        public async Task<ImgBBResponse> UploadIMGBB(IFormFile photo)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(photo.OpenReadStream()), "image", photo.FileName+DateTime.Now.Ticks.ToString());
            var key = "e304a1574ce97d35f1ca6b92b240291d";
            var response = await _httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={key}", content);

            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ImgBBResponse>(result.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            }

            return null;
        }

        public async Task<ImgBBResponse> DeleteIMGBB(IFormFile photo)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(photo.OpenReadStream()), "image", photo.FileName + DateTime.Now.Ticks.ToString());
            var key = "e304a1574ce97d35f1ca6b92b240291d";
            var response = await _httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={key}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ImgBBResponse>(result.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });
            }

            return null;
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

        public async Task<Result> CreateFromFB(FacebookUserData facebookUserData, GeoPluginResponse location)
        {
            //create user
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

            // create profile
            var profile = new Profile(new SignupDTO(facebookUserData), user.Id);
            profile.Location = location.City + ", " + location.Country;
            profile.Longitude = location.Longtitude;
            profile.Latitude = location.Latitude;
            if (await _dbContext.Profiles.AnyAsync(x => x.UserID == user.Id))
            {
                return new Result { IsSuccess = false, Error = "User is exist" };
            }

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            await _dbContext.Profiles.AddAsync(profile);
            await _dbContext.SaveChangesAsync();

            // create discoverysetting
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

            // create profileimages
            if(facebookUserData.photo != null)
            {
                ImgBBResponse imgBBResponse = await this.UploadIMGBB(facebookUserData.photo);
                if (imgBBResponse == null || string.IsNullOrEmpty(imgBBResponse.Data.DisplayUrl))
                {
                    imgBBResponse.Data.DisplayUrl = "https://i.ibb.co/4drKLcS/make-friends.png";
                }

                await _dbContext.ProfileImages.AddAsync(new ProfileImages
                {
                    ImageURL = imgBBResponse.Data.DisplayUrl,
                    DeleteURL = imgBBResponse.Data.DeleteUrl,
                    ProfileID = profile.Id
                });
            }
            else
            {
                await _dbContext.ProfileImages.AddAsync(new ProfileImages
                {
                    ImageURL = "https://i.ibb.co/4drKLcS/make-friends.png",
                    DeleteURL = string.Empty,
                    ProfileID = profile.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            int profileImagesCount = _dbContext.ProfileImages.Where(s => s.ProfileID == profile.Id).Count();
            if (profileImagesCount < 6)
            {
                for (int i = profileImagesCount; i < 6; i++)
                {
                    await _dbContext.ProfileImages.AddAsync(new ProfileImages
                    {
                        ProfileID = profile.Id,
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
                var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return token;
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
