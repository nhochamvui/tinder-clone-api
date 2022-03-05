using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TinderClone.Models
{
    public class Matches
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long MyId { get; set; }

        public long ObjectId { get; set; }

        public bool IsMatched { get; set; }

        public bool IsDislike { get; set; }

        public long DateOfMatch { get; set; }

        public Matches(long objId, bool isMatched)
        {
            this.ObjectId = objId;
            this.IsMatched = isMatched;
        }

        public Matches() { }
    }
}
