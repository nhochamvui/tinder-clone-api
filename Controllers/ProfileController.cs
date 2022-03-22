using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TinderClone.Models;
using TinderClone.Services;

namespace TinderClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly TinderContext _context;
        private IConfiguration _config;
        private readonly IUserService _userService;

        public ProfileController(TinderContext context, IConfiguration config, IUserService userService)
        {
            _config = config;
            _context = context;
            _userService = userService;
        }

        [Authorize]
        [HttpGet]
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
                    if (profile != default)
                    {
                        var profileImages = Models.Profile.GetProfileImages(_context, profile.Id);
                        var profileDTO = new ProfileDTO(profile)
                        {
                            ProfileImages = profileImages
                        };

                        return Ok(profileDTO);
                    }
                }
            }

            var res = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Profile does not exist."),
            };
            return NotFound(res);
        }

        [HttpGet("location")]
        public async Task<IActionResult> Location()
        {
            var header = HttpContext.Request.Headers["x-forwarded-for"];
            Console.WriteLine("/api/profile/location -> request header: " + header);
            if (!header.Equals(string.Empty))
            {
                return Ok(await _userService.GetLocation(header));
            }

            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            Console.WriteLine("/api/profile/location -> RemoteIpAddress: " + ip);
            var result = await _userService.GetLocation(ip);
            Console.WriteLine("/api/profile/location -> Location: " + result.ToString());
            return Ok(result);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateUser(ProfileDTO userInfo)
        {
            long myID = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            Profile profile = await _context.Profiles.Where(x => x.UserID == myID).FirstOrDefaultAsync();
            bool isUpdated = false;
            if (profile != default)
            {
                if (!string.IsNullOrEmpty(userInfo.Hometown))
                {
                    profile.Hometown = userInfo.Hometown;
                    isUpdated = true;
                }

                if (!string.IsNullOrEmpty(userInfo.About))
                {
                    profile.About = userInfo.About;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    _context.Profiles.Update(profile);

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

        [HttpPatch("profileImages")]
        [Authorize]
        public async Task<ActionResult> RemoveProfileImage([FromBody] int imageIndex)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("id")?.Value);
            var profileID = await _context.Profiles.Where(x => x.UserID == myId).Select(x => x.Id).FirstOrDefaultAsync();

            if (imageIndex == 0)
            {
                return BadRequest();
            }

            if (profileID == default)
            {
                return Unauthorized("User does not exist");
            }

            var profileImages = await _context.ProfileImages
                                    .Where(p => p.ProfileID == profileID).Select(p => p)
                                    .OrderByDescending(p => p.Id)
                                    .Reverse().ToListAsync();
            if (profileImages.Any())
            {
                if (imageIndex > profileImages.Count - 1)
                {
                    profileImages.Add(new ProfileImages
                    {
                        ImageURL = string.Empty,
                        DeleteURL = string.Empty,
                        ProfileID = profileID,
                    });
                }
                else
                {
                    profileImages[imageIndex].ImageURL = string.Empty;
                    profileImages[imageIndex].DeleteURL = string.Empty;
                    _context.ProfileImages.Update(profileImages[imageIndex]);
                }

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

        [HttpGet("profileImages")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<string>>> GetProfileImages()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("id")?.Value);
            var profileID = await _context.Profiles.Where(x => x.UserID == myId).Select(x => x.Id).FirstOrDefaultAsync();

            if (profileID == default)
            {
                return Unauthorized("User does not exist");
            }
            List<string> profileImages = Models.Profile.GetProfileImages(_context, profileID);
            return profileImages;
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
