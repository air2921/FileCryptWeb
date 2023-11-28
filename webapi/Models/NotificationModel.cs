using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("notifications")]
    public class NotificationModel
    {
        [Key]
        public int notification_id { get; set; }

        public string? message_header { get; set; }

        public string? message { get; set; }

        public string? priority { get; set; }

        public DateTime? send_time { get; set; }

        public bool? is_checked { get; set; }


        [ForeignKey("sender_id")]
        public int sender_id { get; set; }

        [ForeignKey("receiver_id")]
        public int receiver_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? Sender { get; set; }

        [JsonIgnore]
        public virtual UserModel? Receiver { get; set; }
    }

    public enum Priority
    {
        trade,
        info,
        warning,
        security
    }
}
