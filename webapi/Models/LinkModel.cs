using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi.Models
{
    [Table("links")]
    public class LinkModel
    {
        [Key]
        [Required]
        public int link_id { get; set; }

        [Required]
        public string? u_token { get; set;}

        public DateTime? expiry_date { get; set; }

        public bool? is_used { get; set; }

        public DateTime? created_at {  get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        public virtual UserModel? User { get; set; }
    }
}
