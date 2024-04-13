using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("files")]
    public class FileModel
    {
        [Key]
        public int file_id { get; set; }

        public string file_name { get; set; }

        public string file_mime { get; set; }

        public string file_mime_category { get; set; }

        public string type { get; set; }

        public DateTime operation_date { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? User { get; set; }
    }

    public enum FileType
    {
        Private,
        Internal,
        Received
    }
}
