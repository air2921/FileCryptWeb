using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace webapi.Models
{
    [Table("api")]
    public class ApiModel
    {
        [Key]
        public int api_id { get; set; }

        public string? api_key { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IPAddress? remote_ip { get; set; }

        public bool? is_tracking_ip { get; set; }

        public bool? is_allowed_requesting { get; set; }

        public bool? is_allowed_unknown_ip { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
