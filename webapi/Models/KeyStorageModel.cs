using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("storages")]
    public class KeyStorageModel
    {
        [Key]
        public int storage_id { get; set; }

        public string storage_name { get; set; }

        public DateTime last_time_modified { get; set; }

        public string access_code { get; set; }

        public bool encrypt { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public virtual UserModel? User { get; set; }

        [JsonIgnore]
        public virtual ICollection<KeyStorageItemModel>? StorageItems { get; set; }
    }
}
