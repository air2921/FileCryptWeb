using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("activity")]
    public class ActivityModel
    {
        [Key]
        public int action_id { get; set; }

        public int type_id { get; set; }
        
        public DateTime action_date { get; set; }

        [ForeignKey("user_id")]
        public int user_id { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is null || obj is not ActivityModel other)
                return false;

            return 
                action_id == other.action_id &&
                type_id == other.type_id &&
                action_date == other.action_date && 
                user_id == other.user_id;
        }

        public override int GetHashCode() => HashCode.Combine(action_id, type_id, action_date, user_id);
    }

    public enum Activity
    {
        Encrypt = 101,
        Decrypt = 102,
        OpenOffer = 201,
        CloseOffer = 202,
        AddKey = 301,
        AddStorage = 302,
        ShareStorage = 303
    }
}
