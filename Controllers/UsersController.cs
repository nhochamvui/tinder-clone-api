using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TinderClone.Models;
using TinderClone.Services;

namespace TinderClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TinderContext _context;
        private IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IFacebookService _facebookService;
        public UsersController(TinderContext context, IUserService userService, IFacebookService facebookService, IConfiguration config)
        {
            _config = config;
            _context = context;
            _userService = userService;
            _facebookService = facebookService;
        }

        // GET: api/Users
        [HttpGet("getusers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("profileImages")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<string>>> GetProfileImages()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("id")?.Value);
            List<string> profileImages = UserDTO.GetProfileImages(_context, myId);
            return profileImages;
        }

        [HttpPatch("profileImages")]
        [Authorize]
        public async Task<ActionResult> RemoveProfileImage([FromBody] int imageIndex)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("id")?.Value);
            List<ProfileImages> entities = await _context.ProfileImages.Where(x => x.UserID == myId).ToListAsync();
            if (entities.Count > 0)
            {
                entities[imageIndex].ImageURL = string.Empty;
                _context.ProfileImages.Update(entities[imageIndex]);
                try
                {
                    await _context.SaveChangesAsync();

                    return Ok();
                }
                catch (Exception e)
                {
                    if (!UserExists(myId))
                    {
                        return NotFound();
                    }
                    Console.WriteLine("Exception while update profile image: " + e.Message);

                    return StatusCode(500, new { message = "Failed to update the database!" });
                }
            }

            return NotFound();
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDTO>> GetUser()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var user = await _context.Users.FindAsync(myId);

            if (user != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    name = user.Name,
                    age = ((DateTime.UtcNow - user.DateOfBirth).Days / 365).ToString(),
                    dob = user.DateOfBirth.ToShortDateString(),
                    gender = Models.User.GetGender(user.Gender),
                    email = user.Email,
                    location = user.Location,
                    about = user.About ??= "",
                });
            }

            return NotFound();
        }


        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult> Profile()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var user = await _context.Users.FindAsync(myId);

            if (user != null)
            {
                var isProfileExist = await _context.Profiles.AnyAsync(x => x.UserID == myId);
                if (isProfileExist)
                {
                    var profile = await _context.Profiles.FirstOrDefaultAsync(x => x.UserID == myId);
                    return Ok(new ProfileDTO(profile));
                }
            }
            var res = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Profile does not exist."),
            };
            return NotFound(res);
        }

        [Authorize]
        [HttpPost("getuserbylocation")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUserByLocation(Param param)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var users = await _context.Users.Where(x => x.Location.Equals(param.Location) && x.Id != myId).ToListAsync();
            List<UserDTO> userDTOs = new List<UserDTO>();
            foreach (var item in users)
            {
                userDTOs.Add(new UserDTO(_context, item));
            }
            return Ok(userDTOs);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(long id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateUser(User userInfo)
        {
            long myID = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            User user = _context.Users.Where(x => x.Id == myID).FirstOrDefault();
            bool isUpdated = false;
            if (user != default)
            {
                if (!string.IsNullOrEmpty(userInfo.Location))
                {
                    user.Location = userInfo.Location;
                    isUpdated = true;
                }

                if (!string.IsNullOrEmpty(userInfo.About))
                {
                    user.About = userInfo.About;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    _context.Users.Update(user);

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        if (!UserExists(myID))
                        {
                            return NotFound();
                        }

                        Console.WriteLine("Failed while updating user info: " + e.Message);

                        return StatusCode(500, new { message = "Failed to update the database" });
                    }
                }
            }

            return Ok();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(User userParam)
        {
            if (!string.IsNullOrWhiteSpace(userParam.UserName) && !string.IsNullOrWhiteSpace(userParam.Password))
            {
                var user = _context.Users.SingleOrDefault(x => x.UserName.Equals(userParam.UserName) && x.Password.Equals(userParam.Password));

                if (user != null)
                {
                    int profileImagesCount = _context.ProfileImages.Where(s => s.UserID == user.Id).Count();

                    var userClaims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("id", user.Id.ToString()),
                        new Claim("userName", user.UserName),
                        new Claim("name", user.Name),
                        new Claim("age", ((DateTime.UtcNow - user.DateOfBirth).Days/365).ToString()),
                        new Claim("dob", user.DateOfBirth.ToShortDateString()),
                        new Claim("gender", Models.User.GetGender(user.Gender)),
                        new Claim("email", user.Email),
                        new Claim("location", user.Location),
                        new Claim("about", user.About ??= ""),
                    };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                    var signinCredential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var jwtToken = new JwtSecurityToken(_config["Jwt:Issuer"],
                                                        _config["Jwt:Audience"],
                                                        userClaims,
                                                        expires: DateTime.UtcNow.AddDays(1),
                                                        signingCredentials: signinCredential);

                    if (!_context.DiscoverySettings.Any(s => s.UserID == user.Id))
                    {
                        _context.DiscoverySettings.Add(new DiscoverySettings
                        {
                            AgePreferenceCheck = false,
                            DistancePreference = 0,
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

                    if (profileImagesCount < 6)
                    {
                        for (int i = profileImagesCount; i < 6; i++)
                        {
                            _context.ProfileImages.Add(new ProfileImages
                            {
                                UserID = user.Id,
                                ImageURL = "",
                            });
                            await _context.SaveChangesAsync();
                        }
                    }

                    return Ok(new JwtSecurityTokenHandler().WriteToken(jwtToken));
                }
                else
                {
                    return BadRequest("Username is not exists");
                }
            }

            return BadRequest("Username or Password is null");
        }

        [HttpPost("fbauth")]
        public async Task<ActionResult> Login(FacebookAccessToken facebookAccessToken)
        {
            // 3. we've got a valid token so we can request user data from fb
            if (!await _facebookService.IsAccessTokenValid(facebookAccessToken.AccessToken))
            {
                return Unauthorized("Invalid facebook token.");
            }

                              var userInfoResponse = await _facebookService.GetMe(facebookAccessToken.AccessToken);
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse.ToString(), 
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore});

            var isUserExist = await _context.Users.AnyAsync(x => x.Id == userInfo.Id);
            if (!isUserExist)
            {
                return Ok();
            }

            var token = await _userService.GetToken(userInfo.Id);

            return Ok(new { accessToken = token });
        }

        [HttpPost("fbsignup")]
        public async Task<ActionResult> FacebookSignup(FacebookUserData facebookUserData)
        {
            // 1.generate an app access token
            if (!await _facebookService.IsAccessTokenValid(facebookUserData.AccessToken))
            {
                return Unauthorized("Token is invalid");
            }

            var userInfoResponse = await _facebookService.GetMe(facebookUserData.AccessToken);
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

            bool isUserExist = await _context.Users.AnyAsync(x => x.Id == userInfo.Id);
            if (isUserExist)
            {
                return BadRequest("User is exist");
            }

            // 4. ready to create the local user account (if necessary) and jwt
            FacebookRequiredData facebookRequiredData = new FacebookRequiredData(facebookUserData);
            foreach (PropertyInfo prop in facebookRequiredData.GetType().GetProperties())
            {
                if (prop.GetValue(facebookRequiredData, null) == null)
                {
                    return BadRequest("Required fields is empty");
                }
            }

            // signup
            facebookUserData.Id = userInfo.Id;
            Result result = await _userService.CreateFromFB(facebookUserData);
            if (!result.IsSuccess) 
            {
                return StatusCode(500, new { Content = new StringContent(result.Error) });
            }

            var token = _userService.GetToken(userInfo.Id);
            if (token.Equals(string.Empty))
            {
                return NotFound(new { message = "User is not found" });
            }

            return Ok(new { accessToken = token });
        }

        [HttpGet("discoverysettings")]
        public async Task<ActionResult> DiscoverySettings()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var result = _context.DiscoverySettings.Where(setting => setting.UserID == myId).FirstOrDefault();
            if (result == default)
            {
                return Ok();
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("savesettings")]
        public async Task<ActionResult> SaveSettings(DiscoverySettings settings)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            //var result = _context.DiscoverySettings.(setting => setting.UserID == myId).FirstOrDefault();
            if (myId != settings.UserID)
            {
                return BadRequest();
            }
            _context.Entry(settings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SettingsExists(settings.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        [HttpPost("uploadphoto")]
        [Authorize]
        public async Task<IActionResult> UploadPhoto(IFormFile photo, [FromForm] int index)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            string photoPath = Path.Combine(_config.GetSection("StoredPhotoPath")["path"],
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(photo.FileName));
            Console.WriteLine(photoPath);
            if (!Directory.Exists(_config.GetSection("StoredPhotoPath")["path"]))
            {
                Directory.CreateDirectory(_config.GetSection("StoredPhotoPath")["path"]);
            }

            #region Content Validation
            string[] permitedExtension = { ".jpg", ".jpeg", ".png", ".bmp" };
            string extension = Path.GetExtension(photoPath).ToLowerInvariant();

            // Extension validation
            if (string.IsNullOrEmpty(extension) && !permitedExtension.Contains(extension))
            {
                return Ok("Not supported photo");
            }

            // Signature validation
            Dictionary<string, List<byte[]>> signatures = new Dictionary<string, List<byte[]>>
            {
                {
                    ".jpeg",
                    new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                    }
                },
                {
                    ".jpg",
                    new List<byte[]>
                    {
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                    }
                },
                {
                    ".png",
                    new List<byte[]>
                    {
                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                    }
                },
                {
                    ".bmp",
                    new List<byte[]>
                    {
                        new byte[] { 0x42, 0x4D },
                    }
                }
            };

            Stream photoStream = photo.OpenReadStream();
            using (var reader = new BinaryReader(photoStream))
            {
                var fileSignature = signatures[extension];
                var headerBytes = reader.ReadBytes(fileSignature.Max(m => m.Length));

                bool isValidSignature = fileSignature.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature));
                if (!isValidSignature)
                {
                    photo.OpenReadStream();
                    return Ok("Invalid photo signature");
                }
            }
            #endregion

            using (var stream = System.IO.File.Create(photoPath))
            {
                await photo.CopyToAsync(stream);
                var profileImages = await _context.ProfileImages
                                        .Where(p => p.UserID == myId).Select(p => p)
                                        .OrderByDescending(p => p.Id)
                                        .Reverse().ToListAsync();
                if (index < profileImages.Count)
                {
                    var temp = profileImages[index];
                    temp.ImageURL = Path.GetFileName(stream.Name);
                    _context.ProfileImages.Update(temp);
                }
                // max slot is 6
                else if (index <= 5)
                {
                    profileImages.Add(new ProfileImages
                    {
                        ImageURL = Path.GetFileName(stream.Name),
                        UserID = myId,
                    });
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal Server Error. Something went Wrong!");
                }

                return Created(Path.GetFileName(stream.Name).ToString(), new { index = index });
            }
        }

        [HttpPatch("setgender")]
        [Authorize]
        public async Task<IActionResult> SetGender([FromBody] Models.User param)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == myId);
            if (user != default)
            {
                if (!string.IsNullOrWhiteSpace(Models.User.GetGender(param.Gender)))
                {
                    user.Gender = param.Gender;
                    _context.Users.Update(user);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Ok(new { gender = Models.User.GetGender(param.Gender) });
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Exception save changes: ", ex);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            return BadRequest("The user id or gender is invalid");
        }

        [HttpPost("check/email")]
        [Authorize]
        public async Task<IActionResult> CheckEmail(Models.User param)
        {
            if (!param.Email.Equals(string.Empty))
            {
                var isExist = await _context.Users.AnyAsync(x => x.Email.Equals(param.Email.Trim()));
                return isExist ? StatusCode(422) : Ok();
            }

            return BadRequest();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private bool SettingsExists(long id)
        {
            return _context.DiscoverySettings.Any(e => e.Id == id);
        }
    }
}
