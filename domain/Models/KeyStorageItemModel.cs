using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("storage_items")]
    public class KeyStorageItemModel
    {
        [Key]
        public int key_id { get; set; }

        public string key_name { get; set; }

        public string key_value { get; set; }

        public DateTime created_at { get; set; }

        [ForeignKey("storage_id")]
        public int storage_id { get; set; }

        [JsonIgnore]
        public virtual KeyStorageModel? KeyStorage { get; set; }
    }
}
