using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace webapi.Models
{
    [Table("notifications")]
    public class NotificationModel
    {
        [Key]
        public int notification_id { get; set; }

        [Required]
        public string message_header { get; set; }

        [Required]
        public string message { get; set; }

        [Required]
        public string priority { get; set; }

        [Required]
        public DateTime send_time { get; set; }

        [Required]
        public bool is_checked { get; set; }


        [ForeignKey("sender_id")]
        public int sender_id { get; set; }

        [ForeignKey("receiver_id")]
        public int receiver_id { get; set; }

        public virtual UserModel Sender { get; set; }
        public virtual UserModel Receiver { get; set; }
    }

    public enum Priority
    {
        trade,
        info,
        warning,
        security
    }
}
