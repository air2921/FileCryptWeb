using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("keys")]
    public class KeyModel
    {
        [Key]
        public int key_id { get; set; }

        public string private_key { get; set; }

        public string? internal_key { get; set; }

        public string? received_key { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? User { get; set; }
    }
}
