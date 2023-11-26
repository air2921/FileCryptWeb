using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("users")]
    public class UserModel
    {
        [Key]
        public int id { get; set; }

        public string? username { get; set; }

        public string? role { get; set; }

        [EmailAddress]
        public string? email { get; set; }

        public string? password_hash { get; set; }

        [JsonIgnore]
        public KeyModel? Keys { get; set; }

        [JsonIgnore]
        public ICollection<FileModel>? Files { get; set; }

        [JsonIgnore]
        public TokenModel? Tokens { get; set; }

        [JsonIgnore]
        public ApiModel? API { get; set; }
    }

    public enum Role
    {
        User,
        Admin,
        HighestAdmin
    }
}
