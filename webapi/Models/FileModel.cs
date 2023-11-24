using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("files")]
    public class FileModel
    {
        [Key]
        [Required]
        [JsonIgnore]
        public int file_id { get; set; }

        [Required]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string file_name { get; set; }

        [Required]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string file_mime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime operation_date { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [NotMapped]
        public string? file_message { get; set; }

        [ForeignKey("user_id")]
        [JsonIgnore]
        public int user_id { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }
    }
}
