using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("offers")]
    public class OfferModel
    {
        [Key]
        public int offer_id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string offer_header { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string offer_body { get; set; }

        public int offer_type { get; set; }

        public bool is_accepted { get; set; }

        public DateTime created_at { get; set; }

        [ForeignKey("sender_id")]
        public int sender_id { get; set; }

        [ForeignKey("receiver_id")]
        public int receiver_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? Sender { get; set; }

        [JsonIgnore]
        public virtual UserModel? Receiver { get; set; }
    }

    public enum TradeType
    {
        Key = 101
    }
}
