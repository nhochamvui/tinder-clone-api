using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TinderClone.Models
{
    public class LatestMessages
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
    }
}
