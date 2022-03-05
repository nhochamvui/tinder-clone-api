using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models
{
    public class Messages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long fromID { get; set; }

        [Required]
        public long toID { get; set; }

        public string content { get; set; }


        public bool isRead { get; set; }

        public bool isSent { get; set; }

        public long timeStamp { get; set; }

        public Messages(long fromID, long toID, string content, long timeStamp, bool isRead, bool isSent)
        {
            this.fromID = fromID;
            this.toID = toID;
            this.content = content;
            this.timeStamp = timeStamp;
            this.isRead = isRead;
            this.isSent = isSent;
        }
    }
}
