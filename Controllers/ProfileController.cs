using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
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

        [HttpGet("location")]
        public async Task<IActionResult> Location()
        {
            var header = HttpContext.Request.Headers["x-forwarded-for"];
            Console.WriteLine("/api/profile/location -> request header: " + header);
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            Console.WriteLine("/api/profile/location -> request IP: " + ip);
            var result = await _userService.GetLocation(ip);
            return Ok(result);
        }

        [HttpPost("signup")]
        [Authorize]
        public async Task<IActionResult> Signup([FromBody] Models.SignupDTO param)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Id == myId);
            bool isProfileExist = await _context.Profiles.AnyAsync(x => x.UserID == myId);
            if (user != default && !isProfileExist)
            {
                foreach (PropertyInfo prop in param.GetType().GetProperties())
                {
                    if(prop.GetValue(param, null) == null)
                    {
                        return BadRequest("Required fields is missing");
                    }
                }

                var newProfile = new Profile(
                    param.Name, param.DateOfBirth, param.Gender, param.Email, myId);
                var res = await _context.Profiles.AddAsync(newProfile);
                try
                {
                    await _context.SaveChangesAsync();
                    return Created("users/profile", newProfile);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine("--------------start-----------------");
                    Console.WriteLine("Exception when matching: ", ex);
                    Console.WriteLine("my ID: ", myId);
                    Console.WriteLine("---------------end----------------");
                    return StatusCode(500, new { error = "Failed to update the database" });
                }
            }

            return BadRequest("The user id is invalid or profile is exist");
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
                if (!string.IsNullOrEmpty(userInfo.Location))
                {
                    profile.Location = userInfo.Location;
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
        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
