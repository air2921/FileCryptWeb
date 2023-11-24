using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("offers")]
    public class OfferModel
    {
        [Key]
        public int offer_id { get; set; }

        [Required]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string offer_header { get; set; }

        [Required]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string offer_body { get; set; }

        [Required]
        public string offer_type { get; set; }

        [Required]
        public bool is_accepted { get; set; }

        [ForeignKey("sender_id")]
        public int sender_id { get; set; }

        [ForeignKey("receiver_id")]
        public int receiver_id { get; set; }

        public virtual UserModel Sender { get; set; }
        public virtual UserModel Receiver { get; set; }
    }
}
