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
    public class MessagesController : ControllerBase
    {
        private readonly TinderContext _context;

        public MessagesController(TinderContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("getmessages")]
        public async Task<ActionResult> GetMessages()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            var messages = _context.Messages
                            .Where(x => x.fromID == myId || x.toID == myId)
                            .OrderByDescending(x => x.timeStamp)
                            .Select(mess => new {
                                mess.Id,
                                mess.fromID,
                                mess.toID,
                                mess.content,
                                mess.isRead,
                                mess.isSent,
                                timestamp = mess.timeStamp,
                            }).Reverse();

            if (messages.Any())
            {
                return Ok(messages);
            }

            return Ok();
        }


        public class MessagePagingRequest
        {
            public int PageIndex { get; set; }

            public int PageSize { get; set; }

            public long RecieverID { get; set; }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> GetMessagesByID([FromBody] MessagePagingRequest pagingRequest)
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);

            var messages = _context.Messages
                            .Where(x => (x.fromID == myId && x.toID == pagingRequest.RecieverID)
                            || (x.fromID == pagingRequest.RecieverID && x.toID == myId))
                            .OrderByDescending(x => x.timeStamp)
                            .Select(mess => new {
                                mess.Id,
                                mess.fromID,
                                mess.toID,
                                mess.content,
                                mess.isRead,
                                mess.isSent,
                                timeStamp = mess.timeStamp,
                            });

            int totalRecords = await messages.CountAsync();

            if (totalRecords > 0)
            {
                var result = messages.Skip((pagingRequest.PageIndex - 1) * pagingRequest.PageSize)
                    .Take(pagingRequest.PageSize);
                return Ok(result);
            }

            return Ok();
        }


        [Authorize]
        [HttpGet("getlatestmessages")]
        public async Task<ActionResult> GetLatestMessages()
        {
            long myId = Convert.ToInt64(HttpContext.User.FindFirst("Id")?.Value);
            var matchedIDs = await _context.Matches.Where(match => match.MyId == myId && match.IsMatched).Select(m => m.ObjectId).ToListAsync();
            List<Messages> results = new List<Messages>();
            foreach (var id in matchedIDs)
            {
                var temp = _context.Messages
                    .Where(m => (m.fromID == id && m.toID == myId) || (m.fromID == myId && m.toID == id) && !m.isRead)
                    .OrderByDescending(m => m.timeStamp).FirstOrDefault();
                if(temp != default)
                {
                    results.Add(temp);
                }
            }

            return Ok(results.OrderByDescending(m => m.timeStamp));
        }
    }


}
