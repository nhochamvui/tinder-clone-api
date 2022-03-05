using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinderClone.Models;

namespace TinderClone.Hubs
{
    public class Chat : Hub
    {
        private readonly TinderContext _context;
        public Chat(TinderContext context)
        {
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("on connected");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string targetID, string content)
        {
            long senderID = Int32.Parse(Context.User.Claims.Where(x => x.Type.Equals("id")).Select(x => x.Value).FirstOrDefault().ToString());
            Messages messages = new Messages(senderID, Int32.Parse(targetID), content, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), false, true);
            await _context.Messages.AddAsync(messages);
            await _context.SaveChangesAsync();
            var message = _context.Messages.SingleOrDefault(m => m.Id == messages.Id);
            var users = new string[] { senderID.ToString(), targetID };
            Clients.Users(senderID.ToString()).SendAsync("Inbox", message);
            Clients.Users(targetID).SendAsync("Inbox", message);
        }
        

        public class Message
        {
            public string content { get; set; }
            public string senderID { get; set; }
            public string targetID { get; set; }
            public long timestamp { get; set; }

            public Message(string content, string senderID, string targetID, long timestamp)
            {
                this.content = content;
                this.senderID = senderID;
                this.targetID = targetID;
                this.timestamp = timestamp;
            }
        }
    }
}
