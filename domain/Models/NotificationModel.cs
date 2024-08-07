﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("notifications")]
    public class NotificationModel
    {
        [Key]
        public int notification_id { get; set; }

        public string message_header { get; set; }

        public string message { get; set; }

        public int priority { get; set; }

        public DateTime send_time { get; set; }

        public bool is_checked { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? Receiver { get; set; }
    }

    public enum Priority
    {
        Trade = 101,
        Info = 102,
        Warning = 301,
        Security = 302
    }
}
