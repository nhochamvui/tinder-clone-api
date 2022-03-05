using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private readonly TinderContext _context;
        private IConfiguration _config;

        public MatchesController(TinderContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("likes")]
        [Authorize]
        public async Task<ActionResult> PostMatches([FromBody] Models.User obj)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            int likeCount = _context.DiscoverySettings.Where(x => x.UserID == myId).Select(x => x.LikeCount).FirstOrDefault();

            if(likeCount == 0)
            {
                return Ok(113);
            }

            Matches myMatches = _context.Matches.SingleOrDefault(x => x.MyId == myId && x.ObjectId == obj.Id);
            Matches objMatch = _context.Matches.SingleOrDefault(x => x.MyId == obj.Id && x.ObjectId == myId && !x.IsDislike);

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
                        throw;
                    }
                }
            }
            return Ok(new Matches(myMatches.ObjectId, myMatches.IsMatched));
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetMatches()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            var matches = _context.Matches.Where(x => x.IsMatched == true && x.MyId == myId);
            var profileImages = _context.ProfileImages.Select(x => x);

            if (matches.Count() > 0)
            {
                var result = await matches.Join(_context.Users, x => x.ObjectId, y => y.Id, (x, y) => new
                {
                    DateOfMatch = x.DateOfMatch,
                    ID = y.Id,
                    Name = y.Name,
                    DateOfBirth = y.DateOfBirth.ToShortDateString(),
                    Age = ((DateTime.UtcNow - y.DateOfBirth).Days / 365).ToString(),
                    Gender = Models.User.GetGender(y.Gender),
                    About = y.About,
                    Location = y.Location,
                    ProfileImages = new List<string>(),})
                    .OrderByDescending(x => x.DateOfMatch)
                .ToListAsync();
                foreach (var item in result)
                {
                    item.ProfileImages.AddRange(UserDTO.GetProfileImages(_context, item.ID));
                }
                return Ok(result);
            }

            return Ok(matches);
        }
        public class DiscoverFilter
        {
            public int Id { get; set; }

            public int Gender { get; set; }

            public string Location { get; set; }

            public int minAge { get; set; }

            public int maxAge { get; set; }
        }

        [HttpPost("discover")]
        [Authorize]
        public async Task<ActionResult> DiscoverPeople(DiscoverFilter userFilter)
        {
            if (userFilter == null)
            {
                Console.WriteLine("Bad request");
                return BadRequest();
            }

            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var myMatches = _context.Matches.Where(x => (x.MyId == myId && x.IsDislike == true)
                                                   || x.MyId == myId).ToList();
            var predicate = _context.Users.Where(x => x.Id != myId).ToList()
                .GroupJoin(myMatches, x => x.Id, y => y.ObjectId, (x, y) => new { x, y })
                .SelectMany(x => x.y.DefaultIfEmpty(), (x, y) => new { x.x, x.y })
                .Where(x => x.y.Count() == 0).Select(x => x.x);
            if (!string.IsNullOrWhiteSpace(userFilter.Gender.ToString()))
            {
                predicate = predicate.Where(x => x.Gender == userFilter.Gender);
            }

            if (!string.IsNullOrWhiteSpace(userFilter.Location))
            {
                predicate = predicate.Where(x => x.Location == userFilter.Location);
             }

            if (userFilter.minAge != 0 && userFilter.maxAge != 0)
            {
                predicate = predicate.Where(x => Models.User.GetAge(x.DateOfBirth) >= userFilter.minAge &&
                                            Models.User.GetAge(x.DateOfBirth) <= userFilter.maxAge);
            }
            var userDTOs = predicate.Select(x => new UserDTO(_context, x));
            return Ok(userDTOs);
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
