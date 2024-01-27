using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("api")]
    public class ApiModel
    {
        [Key]
        public int api_id { get; set; }

        public string api_key { get; set; }

        public string type { get; set; }

        public DateTime? expiry_date { get; set; }

        public bool is_blocked { get; set; }

        public DateTime last_time_activity { get; set; }

        public int max_request_of_day { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }

    public enum ApiType
    {
        Classic,
        Development,
        Production
    }
}
