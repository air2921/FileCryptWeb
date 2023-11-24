using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("users")]
    public class UserModel
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string role { get; set; }

        [EmailAddress]
        [Required]
        public string email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string password_hash { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KeyModel Keys { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<FileModel> Files { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TokenModel Tokens { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ApiModel API { get; set; }
    }

    public enum Role
    {
        User,
        Admin,
        HighestAdmin
    }
}
