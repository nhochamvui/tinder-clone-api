using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TinderClone.Models
{
    [Table("ProfileImages")]
    public class ProfileImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string ImageURL { get; set; }

        public string DeleteURL { get; set; }

        public long ProfileID { get; set; }
        public Profile Profile { set; get; }
    }
}
