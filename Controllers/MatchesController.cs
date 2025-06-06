﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinderClone.Hubs;
using TinderClone.Models;
using TinderClone.Models.RequestParam;
using TinderClone.Services;

namespace TinderClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly TinderContext _context;
        private readonly IHubContext<Chat, IChatHub> _hubContext;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;
        public MatchesController(TinderContext context, IHubContext<Chat, IChatHub> hubContext, IConfiguration config, IUserService userService, ILocationService locationService)
        {
            _config = config;
            _context = context;
            _hubContext = hubContext;
            _userService = userService;
            _locationService = locationService;
        }

        [HttpPost("likes")]
        [Authorize]
        public async Task<ActionResult> PostMatches([FromBody] Models.User obj)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var profileID = await _context.Profiles.Where(x => x.UserID == myId).Select(x => x.Id).FirstOrDefaultAsync();

            int likeCount = _context.DiscoverySettings.Where(x => x.UserID == myId).Select(x => x.LikeCount).FirstOrDefault();

            if (likeCount == 0)
            {
                return Ok(113);
            }

            Matches myMatches = _context.Matches.SingleOrDefault(x => x.MyId == myId && x.ObjectId == obj.Id);
            Matches objMatch = _context.Matches.SingleOrDefault(x => x.MyId == obj.Id && x.ObjectId == myId && !x.IsMatched && !x.IsDislike);

            if (myMatches == null)
            {
                myMatches = new Matches
                {
                    MyId = myId,
                    ObjectId = obj.Id,
                    IsMatched = false,
                    IsDislike = false,
                    DateOfMatch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                await _context.Matches.AddAsync(myMatches);
                await _context.SaveChangesAsync();
            }

            if (objMatch != null)
            {
                myMatches.IsMatched = true;
                objMatch.IsMatched = true;
                objMatch.DateOfMatch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _context.Entry(objMatch).State = EntityState.Modified;
                _context.Entry(myMatches).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!MatchExists(myId) || !MatchExists(objMatch.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine("Exception when matching: ", ex);
                        Console.WriteLine("my ID: ", myId, " | his/her's ID: ", obj.Id);
                        return StatusCode(500);
                    }
                }
            }

            var match = _context.Matches.Where(x => x.IsMatched && x.MyId == myId && x.ObjectId == obj.Id);

            if (match.Any())
            {
                var result = await match.Join(_context.Users, m => m.ObjectId, u => u.Id, (m, u) => new
                {
                    DateOfMatch = m.DateOfMatch,
                    UserID = u.Id
                })
                    .Join(_context.Profiles, x => x.UserID, p => p.UserID, (x, p) => new
                    {
                        Id = x.UserID,
                        x.DateOfMatch,
                        p.Name,
                        DateOfBirth = p.DateOfBirth.ToShortDateString(),
                        Age = ((DateTime.UtcNow - p.DateOfBirth).Days / 365).ToString(),
                        p.Gender,
                        p.About,
                        p.Location,
                        ProfileImages = _context.ProfileImages.Where(x => x.ProfileID == p.Id).Select(x => x.ImageURL).ToList()
                    }).FirstOrDefaultAsync();

                if (result != default)
                {
                    SendNewMatch(obj.Id, myId);
                    return Ok(new { isMatched = true, match = result });
                }
            }
            else
            {
                return Ok(new { isMatched = false });
            }

            return StatusCode(500);
        }

        private void SendNewMatch(long targetID, long matchUserID)
        {
            var result = _context.Profiles.Where(p => p.UserID == matchUserID).Select(p => new
            {
                Id = matchUserID,
                DateOfMatch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                p.Name,
                DateOfBirth = p.DateOfBirth.ToShortDateString(),
                Age = ((DateTime.UtcNow - p.DateOfBirth).Days / 365).ToString(),
                p.Gender,
                p.About,
                p.Location,
                ProfileImages = _context.ProfileImages.Where(x => x.ProfileID == p.Id).Select(x => x.ImageURL).ToList()
            }).FirstOrDefault();

            if (result != default)
            {
                _hubContext.Clients.User(targetID.ToString()).NewMatch(new { isMatched = true, match = result });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetMatches()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            var matches = _context.Matches.Where(x => x.IsMatched && x.MyId == myId);
            var profileImages = _context.ProfileImages.Select(x => x);

            if (matches.Any())
            {
                var result = await matches.Join(_context.Users, m => m.ObjectId, u => u.Id, (m, u) => new
                {
                    DateOfMatch = m.DateOfMatch,
                    UserID = u.Id
                })
                    .Join(_context.Profiles, x => x.UserID, p => p.UserID, (x, p) => new
                    {
                        Id = x.UserID,
                        x.DateOfMatch,
                        p.Name,
                        DateOfBirth = p.DateOfBirth.ToShortDateString(),
                        Age = ((DateTime.UtcNow - p.DateOfBirth).Days / 365).ToString(),
                        p.Gender,
                        p.About,
                        p.Location,
                        ProfileImages = profileImages.Where(x => x.ProfileID == p.Id).OrderByDescending(x => x.Id).Reverse().Select(x => x.ImageURL).ToArray()
                    })
                    .OrderByDescending(x => x.DateOfMatch)
                .ToListAsync();

                return Ok(result);
            }

            return Ok(matches);
        }

        [HttpPost("discover")]
        [Authorize]
        public async Task<ActionResult> DiscoverPeople(DiscoverFilter userFilter)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            if (userFilter == null)
            {
                return BadRequest("Filter is required");
            }

            if (!await _context.Users.AnyAsync(x => x.Id == myId))
            {
                return Unauthorized("User does not exist");
            }

            Profile profile = await _context.Profiles.SingleOrDefaultAsync(x => x.UserID == myId);
            if (profile == default)
            {
                return Unauthorized("User does not exist");
            }

            var header = HttpContext.Request.Headers["x-forwarded-for"];
            GeoPluginResponse myLocation = new GeoPluginResponse();
            if (!string.IsNullOrWhiteSpace(header))
            {
                myLocation = await _userService.GetLocation(header);
                profile.Location = myLocation.City + ", " + myLocation.Country;
                profile.Longitude = myLocation.Longtitude;
                profile.Latitude = myLocation.Latitude;
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
            }
            else
            {
                myLocation.Longtitude = profile.Longitude;
                myLocation.Latitude = profile.Latitude;
            }

            var myMatches = _context.Matches.Where(x => x.MyId == myId).ToList();

            var predicate = _context.Profiles.Where(x => x.UserID != myId).ToList()
                .GroupJoin(myMatches, p => p.UserID, m => m.ObjectId, (p, m) => new { p, m })
                .Where(pm => !pm.m.Any()).Select(pm => pm.p);

            //distance
            if (userFilter.Distance != 0)
            {
                predicate = predicate.Where(x => _locationService.GetDistance(new Coordinate { Latitude = x.Latitude, Longitude = x.Longitude },
                    new Coordinate { Latitude = myLocation.Latitude, Longitude = myLocation.Longtitude }) <= userFilter.Distance);
            }

            if (!string.IsNullOrWhiteSpace(userFilter.Gender.ToString())
                && userFilter.Gender < Profile.GetNumberOfGender())
            {
                predicate = predicate.Where(x => x.Gender == userFilter.Gender);
            }

            if (userFilter.minAge != 0 && userFilter.maxAge != 0)
            {
                predicate = predicate.Where(x => Profile.ParseAge(x.DateOfBirth) >= userFilter.minAge &&
                                            Profile.ParseAge(x.DateOfBirth) <= userFilter.maxAge);
            }

            var profileDTOs = predicate.Select(x => new
            {
                Name = x.Name,
                Age = ((DateTime.UtcNow - x.DateOfBirth).Days / 365),
                DateOfBirth = x.DateOfBirth.ToShortDateString(),
                Gender = Profile.ParseGender(x.Gender),
                Hometown = x.Hometown,
                About = x.About,
                UserID = x.UserID,
                ProfileImages = Profile.GetProfileImages(_context, x.Id),
            });

            return Ok(profileDTOs);
        }

        [HttpPost("dislike")]
        [Authorize]
        public async Task<ActionResult> Dislike(DiscoverFilter userFilter)
        {
            if (userFilter == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(userFilter.Id.ToString()))
            {
                return BadRequest();
            }

            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            var myMatch = _context.Matches.Where(x => x.MyId == myId && x.ObjectId == userFilter.Id).ToList();

            if (myMatch.Count == 1)
            {
                myMatch.First().IsDislike = true;
                myMatch.First().DateOfMatch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _context.Matches.Update(myMatch.First());

            }
            else
            {
                _context.Matches.Add(new Matches
                {
                    MyId = myId,
                    DateOfMatch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ObjectId = userFilter.Id,
                    IsDislike = true,
                    IsMatched = false,
                });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool MatchExists(long id)
        {
            return _context.Matches.Any(e => e.Id == id);
        }
    }


}
