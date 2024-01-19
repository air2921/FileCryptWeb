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

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
