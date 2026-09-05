using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TinderClone.Infrastructure;
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
        Task<Result> CreateUserFromFB(FacebookUserData facebookUserData, GeoPluginResponse location);
        Task<Result> CreateLocalUser(SignupRequest request, GeoPluginResponse location);
        Task<string> GetToken(long userID);
        Task<DiscoverySettings> GetDiscoverySettingsByUserID(long userID);
        Task<GeoPluginResponse> GetLocation(string ip);
        Task<Profile> GetProfile(long userID);
        Task<bool> IsUserExist(long userID);
        Task<UploadedImage> UploadImage(IFormFile photo);
    }
    public class UserService : IUserService
    {
        private readonly TinderContext _dbContext;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly IUsersRepository _usersRepository;
        private readonly Cloudinary _cloudinary;

        public UserService(TinderContext dbContext, IConfiguration config, IUsersRepository usersRepository, Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "tinder-clone-api/1.0");
            _usersRepository = usersRepository;
            _cloudinary = cloudinary;
        }

        public async Task<GeoPluginResponse> GetLocation(string ip)
        {
            try
            {
                string ipAddress = ip?.Split(',')[0].Trim();
                var result = await _httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=status,message,country,regionName,city,lat,lon,query");
                var location = JsonConvert.DeserializeObject<GeoPluginResponse>(result,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

                if (location == null)
                {
                    return new GeoPluginResponse();
                }

                if (location.Status != "success")
                {
                    Console.WriteLine($"GetLocation: ip-api returned '{location.Status}': {location.Message}");
                    return new GeoPluginResponse();
                }

                return location;
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetLocation: failed to resolve location: " + ex.Message);
                return new GeoPluginResponse();
            }
        }

        public async Task<UploadedImage> UploadImage(IFormFile photo)
        {
            if (photo == null)
            {
                return null;
            }

            string[] permittedExtensions = { ".jpg", ".jpeg", ".png" };
            string extension = System.IO.Path.GetExtension(photo.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            {
                Console.WriteLine("UploadImage: unsupported file type: " + extension);
                return null;
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (photo.Length > maxFileSize)
            {
                Console.WriteLine("UploadImage: file too large: " + photo.Length + " bytes");
                return null;
            }

            using (var stream = photo.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(photo.FileName, stream),
                };
                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    Console.WriteLine("UploadImage failed: " + result.Error.Message);
                    return null;
                }

                return new UploadedImage
                {
                    Url = result.SecureUrl?.ToString() ?? result.Url?.ToString(),
                    PublicId = result.PublicId,
                };
            }
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }
            var user = _dbContext.Users.SingleOrDefault(x => x.UserName.ToLower().Equals(username.ToLower()));

            if (user == null)
            {
                user = _dbContext.Users.FirstOrDefault(x => x.Profile != null && x.Profile.Email != null && x.Profile.Email.ToLower().Equals(username.ToLower()));
            }

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
            if (string.IsNullOrEmpty(userPassword))
            {
                return false;
            }

            if (userPassword.Contains(":"))
            {
                return PasswordHasher.Verify(password, userPassword);
            }

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

        public async Task<Result> CreateUserFromFB(FacebookUserData facebookUserData, GeoPluginResponse location)
        {
            //create user
            var user = new User
            {
                Id = facebookUserData.Id,
            };

            if (await _dbContext.Users.AnyAsync(x => x.Id == facebookUserData.Id))
            {
                return new Result { IsSuccess = false, Error = "User is exist" };
            }

            // create profile
            var profile = new Profile(facebookUserData)
            {
                Location = (string.IsNullOrEmpty(location.City) ? location.City + ", " : string.Empty) + location.Country,
                Hometown = location.Country,
                Longitude = location.Longtitude,
                Latitude = location.Latitude
            };

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
                    Location = profile.Location,
                    LookingForGender = 3,
                    MaxAge = 100,
                    MinAge = 18,
                    SuperlikeCount = 3,
                    UserID = user.Id
                });
            }
            await _dbContext.SaveChangesAsync();

            // create profileimages
            string firstPhotoUrl = "https://i.ibb.co/yQYP8Qx/Portrait-Placeholder.png";
            if (facebookUserData.photo != null)
            {
                UploadedImage uploadedImage = await UploadImage(facebookUserData.photo);
                if (uploadedImage != null && !string.IsNullOrEmpty(uploadedImage.Url))
                {
                    firstPhotoUrl = uploadedImage.Url;
                }
            }

            await _dbContext.ProfileImages.AddAsync(new ProfileImages
            {
                ImageURL = firstPhotoUrl,
                DeleteURL = string.Empty,
                ProfileID = profile.Id
            });

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

        public async Task<Result> CreateLocalUser(SignupRequest request, GeoPluginResponse location)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new Result { IsSuccess = false, Error = "Name is required" };
            }
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return new Result { IsSuccess = false, Error = "Username is required" };
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new Result { IsSuccess = false, Error = "Password is required" };
            }
            if (string.IsNullOrWhiteSpace(request.Gender))
            {
                return new Result { IsSuccess = false, Error = "Gender is required" };
            }
            if (string.IsNullOrWhiteSpace(request.Birthday))
            {
                return new Result { IsSuccess = false, Error = "Birthday is required" };
            }

            string userName = request.UserName.Trim();

            if (await _dbContext.Users.AnyAsync(x => x.UserName.ToLower().Equals(userName.ToLower())))
            {
                return new Result { IsSuccess = false, Error = "Username is already taken" };
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && await _dbContext.Profiles.AnyAsync(x => x.Email.Equals(request.Email.Trim())))
            {
                return new Result { IsSuccess = false, Error = "Email is exist" };
            }

            DateTime dateOfBirth;
            try
            {
                dateOfBirth = DateTime.ParseExact(request.Birthday, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (Exception)
            {
                return new Result { IsSuccess = false, Error = "Birthday is invalid" };
            }

            if (location == null)
            {
                location = new GeoPluginResponse();
            }

            // create user
            var user = new User
            {
                UserName = userName,
                Password = PasswordHasher.Hash(request.Password),
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // create profile
            var profile = new Profile
            {
                Name = request.Name,
                DateOfBirth = dateOfBirth,
                Gender = Profile.ParseGender(request.Gender),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                UserID = user.Id,
                Location = (string.IsNullOrEmpty(location.City) ? string.Empty : location.City + ", ") + location.Country,
                Hometown = location.Country,
                Longitude = location.Longtitude,
                Latitude = location.Latitude,
            };
            await _dbContext.Profiles.AddAsync(profile);
            await _dbContext.SaveChangesAsync();

            // create discovery settings
            await _dbContext.DiscoverySettings.AddAsync(new DiscoverySettings
            {
                AgePreferenceCheck = false,
                DistancePreference = 2,
                DistancePreferenceCheck = false,
                LikeCount = 30,
                Location = profile.Location,
                LookingForGender = 3,
                MaxAge = 100,
                MinAge = 18,
                SuperlikeCount = 3,
                UserID = user.Id,
            });
            await _dbContext.SaveChangesAsync();

            // create profile images (1 photo + 5 empty slots)
            string firstImageUrl = "https://i.ibb.co/yQYP8Qx/Portrait-Placeholder.png";
            if (request.Photo != null)
            {
                UploadedImage uploadedImage = await UploadImage(request.Photo);
                if (uploadedImage != null && !string.IsNullOrEmpty(uploadedImage.Url))
                {
                    firstImageUrl = uploadedImage.Url;
                }
            }

            await _dbContext.ProfileImages.AddAsync(new ProfileImages
            {
                ImageURL = firstImageUrl,
                DeleteURL = string.Empty,
                ProfileID = profile.Id,
            });

            for (int i = 1; i < 6; i++)
            {
                await _dbContext.ProfileImages.AddAsync(new ProfileImages
                {
                    ProfileID = profile.Id,
                    ImageURL = string.Empty,
                });
            }
            await _dbContext.SaveChangesAsync();

            return new Result { IsSuccess = true, Error = null, UserId = user.Id };
        }

        async public Task<string> GetToken(long userID)
        {
            if (await _usersRepository.IsUserExist(userID))
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
                                                    expires: DateTime.UtcNow.AddMinutes(15),
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

        public async Task<bool> IsUserExist(long userID)
        {
            return await _usersRepository.IsUserExist(userID);
        }

        public async Task<DiscoverySettings> GetDiscoverySettingsByUserID(long userID)
        {
            var res = await _dbContext.DiscoverySettings.Where(setting => setting.UserID == userID).FirstOrDefaultAsync();
            return res;
        }

        public async Task<Profile> GetProfile(long userID)
        {
            return await _usersRepository.GetProfile(userID);
        }
    }
}
