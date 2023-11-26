using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("keys")]
    public class KeyModel
    {
        [Key]
        public int key_id { get; set; }

        public string? private_key { get; set; }

        public string? person_internal_key { get; set; }

        public string? received_internal_key { get; set; }

        [JsonIgnore]
        public int user_id { get; set; }

        [JsonIgnore]
        public UserModel User { get; set; }
    }
}
