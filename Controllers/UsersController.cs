using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
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
using TinderClone.Infrastructure;
using TinderClone.Models;
using TinderClone.Models.Response;
using TinderClone.Services;

namespace TinderClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TinderContext _context;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IFacebookService _facebookService;
        private readonly ILocationService _locationService;
        private readonly IUsersRepository _usersRepository;
        public UsersController(TinderContext context, ILocationService locationService, IUserService userService, IFacebookService facebookService, IConfiguration config)
        {
            _config = config;
            _context = context;
            _userService = userService;
            _facebookService = facebookService;
            _locationService = locationService;
        }

        // GET: api/Users
        [HttpGet("getusers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
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
                });
            }

            return NotFound();
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
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
                    var profileID = await _context.Profiles.Where(x => x.UserID == user.Id).Select(x => x.Id).FirstOrDefaultAsync();

                    if (profileID == default)
                    {
                        return Unauthorized("User does not exist");
                    }

                    int profileImagesCount = _context.ProfileImages.Where(s => s.ProfileID == profileID).Count();

                    var userClaims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("id", user.Id.ToString()),
                        new Claim("userName", user.UserName),
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
                            Location = string.Empty,
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
                                ProfileID = profileID,
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

        [HttpDelete]
        [Authorize]
        public async Task<ActionResult> DeleteUser()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var profileID = await _context.Profiles.Where(x => x.UserID == myId).Select(x => x.Id).FirstOrDefaultAsync();

            if (profileID == default)
            {
                return Unauthorized("User does not exist");
            }

            var discoverySettings = _context.DiscoverySettings.Where(setting => setting.UserID == myId).FirstOrDefault();
            var matches = _context.Matches.Where(m => m.MyId == myId || m.ObjectId == myId).ToArray();
            var messages = _context.Messages.Where(m => m.fromID == myId || m.toID == myId).ToList();
            var profileImages = await _context.ProfileImages.Where(u => u.ProfileID == profileID).ToListAsync();
            var profile = _context.Profiles.Where(u => u.UserID == myId).FirstOrDefault();
            var user = _context.Users.Where(u => u.Id == myId).FirstOrDefault();
            if (discoverySettings == default || user == default || profile == default || profileImages.Count <= 0)
            {
                return NotFound(new { message = "User not found" });
            }

            _context.DiscoverySettings.Remove(discoverySettings);
            _context.Matches.RemoveRange(matches);
            _context.Messages.RemoveRange(messages);
            _context.ProfileImages.RemoveRange(profileImages);
            _context.Profiles.Remove(profile);
            _context.Users.Remove(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete user '{myId}' occured an exception: {ex.Message}");
                Console.WriteLine($"Delete user '{myId}' Stack Trace: {ex.StackTrace}");
                return StatusCode(500, ex.Message);
            }

            return Ok();
        }

        //test
        //login with fb accesstoken
        [HttpPost("fbauth")]
        public async Task<ActionResult> Login(FacebookAccessToken facebookAccessToken)
        {
            // 3. we've got a valid token so we can request user data from fb
            if (!await _facebookService.IsAccessTokenValid(facebookAccessToken.AccessToken))
            {
                return Unauthorized("Invalid facebook token");
            }

            var userInfoResponse = await _facebookService.GetMe(facebookAccessToken.AccessToken);

            if(userInfoResponse == null)
            {
                return StatusCode(500, new FacebookSignupResponse("Facebook Internal's error", false, null));
            }

            var isUserExist = await _userService.IsUserExist(userInfoResponse.Id);
            if (!isUserExist)
            {
                return new OkObjectResult(string.Empty);
            }

            var token = await _userService.GetToken(userInfoResponse.Id);

            return new OkObjectResult(new { accessToken = token });
        }

        //test
        //signup with fb accesstoken
        [HttpPost("fbsignup")]
        public async Task<ActionResult> FacebookSignup([FromForm] FacebookUserData facebookUserData)
        {
            // 1.generate an app access token
            if (!await _facebookService.IsAccessTokenValid(facebookUserData.AccessToken))
            {
                return Unauthorized(new FacebookSignupResponse("Facebook token is invalid", false, null));
            }

            var userInfoResponse = await _facebookService.GetMe(facebookUserData.AccessToken);
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse.ToString(),
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore });

            bool isUserExist = await _userService.IsUserExist(userInfo.Id);
            if (isUserExist)
            {
                return BadRequest(new FacebookSignupResponse("User is exist", false, null));
            }

            // 4. ready to create the local user account (if necessary) and jwt
            FacebookRequiredData facebookRequiredData = new FacebookRequiredData(facebookUserData);
            foreach (PropertyInfo prop in facebookRequiredData.GetType().GetProperties())
            {
                if (prop.GetValue(facebookRequiredData, null) == null)
                {
                    return BadRequest(new FacebookSignupResponse("Required fields is empty", false, null));
                }
            }

            // create user dependencies
            facebookUserData.Id = userInfo.Id;
            var ip = HttpContext.Request.Headers["x-forwarded-for"];
            GeoPluginResponse location = await _locationService.GetLocation(ip);

            Result result = await _userService.CreateUserFromFB(facebookUserData, location);
            if (!result.IsSuccess) 
            {
                return StatusCode(500, new FacebookSignupResponse(result.Error, false, null));
            }

            // generate token
            var token = _userService.GetToken(userInfo.Id);
            if (token.Equals(string.Empty))
            {
                return NotFound(new FacebookSignupResponse("User is not found", false, null));
            }

            return Ok(new FacebookSignupResponse(null, true, token.Result));
        }

        //test
        [HttpGet("discoverysettings")]
        [Authorize]
        public async Task<ActionResult> DiscoverySettings()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var result = await _userService.GetDiscoverySettingsByUserID(myId);
            if (result == default)
            {
                return Ok();
            }

            return Ok(new DiscoverySettingsDTO(result));
        }

        //test
        [Authorize]
        [HttpPost("savesettings")]
        public async Task<ActionResult> SaveSettings(DiscoverySettingsDTO settings)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var discoverySetting = await _userService.GetDiscoverySettingsByUserID(myId);
            if(discoverySetting == null)
            {
                return BadRequest();
            }

            discoverySetting.Update(settings);

            _context.Entry(discoverySetting).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SettingsExists(discoverySetting.Id))
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

        // deprecated
        [HttpPost("/local/uploadphoto")]
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
                                        .Where(p => p.ProfileID == myId).Select(p => p)
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
                        ProfileID = myId,
                    });
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Internal Server Error. Something went Wrong! " + ex.Message });
                }

                return Created(Path.GetFileName(stream.Name).ToString(), new { index = index });
            }
        }

        // test
        [HttpPost("uploadphoto")]
        [Authorize]
        public async Task<IActionResult> UploadPhotoIMGBB(IFormFile photo, [FromForm] int index)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var profileID = await _context.Profiles.Where(x => x.UserID == myId).Select(x => x.Id).FirstOrDefaultAsync();

            if (profileID == default)
            {
                return Unauthorized("User does not exist");
            }

            ImgBBResponse imgBBResponse = await _userService.UploadIMGBB(photo);
            if (imgBBResponse == null)
            {
                return StatusCode(500, new { message = "Upload Failed: " });
            }

            var profileImages = await _context.ProfileImages
                                    .Where(p => p.ProfileID == profileID).Select(p => p)
                                    .OrderByDescending(p => p.Id)
                                    .Reverse().ToListAsync();
            if (index < profileImages.Count)
            {
                var temp = profileImages[index];
                temp.ImageURL = imgBBResponse.Data.DisplayUrl;
                temp.DeleteURL = imgBBResponse.Data.DeleteUrl;
                _context.ProfileImages.Update(temp);
            }
            // max slot is 6
            else if (index <= 5)
            {
                _context.ProfileImages.Add(new ProfileImages
                {
                    ImageURL = imgBBResponse.Data.DisplayUrl,
                    DeleteURL = imgBBResponse.Data.DeleteUrl,
                    ProfileID = profileID,
                });
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error. Something went Wrong! " + ex);
            }

            return Created(imgBBResponse.Data.DisplayUrl, new { index = index });
        }

        //test
        [HttpPatch("setgender")]
        [Authorize]
        public async Task<IActionResult> SetGender([FromBody] Models.Profile param)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var profile = await _userService.GetProfile(myId);

            if (profile != default)
            {
                if (!string.IsNullOrWhiteSpace(Models.User.GetGender(param.Gender)))
                {
                    profile.Gender = param.Gender;
                    _context.Profiles.Update(profile);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Ok(new { gender = Models.User.GetGender(param.Gender) });
                    }
                    catch (Exception ex)
                    {
                        Console.Write("Exception save changes: "+ ex);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            }

            return BadRequest("The user id or gender is invalid");
        }

        //test
        [HttpPost("check/email")]
        public async Task<IActionResult> CheckEmail(Models.Profile param)
        {
            if (!param.Email.Equals(string.Empty))
            {
                var isExist = await _context.Profiles.AnyAsync(x => x.Email.Equals(param.Email.Trim()));
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
