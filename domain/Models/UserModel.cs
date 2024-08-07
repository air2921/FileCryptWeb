﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace domain.Models
{
    [Table("users")]
    public class UserModel
    {
        private string? _avatarId;

        [Key]
        public int id { get; set; }

        public string? avatarId
        {
            get => _avatarId;
            set
            {
                if (value is null)
                    _avatarId = value;

                _avatarId = $"#{id}:" + value;
            }
        }

        public string? avatar_content_type { get; set; }

        public string? avatar_name { get; set; }

        public string username { get; set; }

        public string role { get; set; }

        [EmailAddress]
        public string email { get; set; }

        public string password { get; set; }

        public bool is_2fa_enabled { get; set; }

        public bool is_blocked { get; set; }

        [JsonIgnore]
        public virtual ICollection<ActivityModel>? Activity { get; set; }

        [JsonIgnore]
        public virtual ICollection<KeyStorageModel>? KeyStorages { get; set; }

        [JsonIgnore]
        public virtual ICollection<FileModel>? Files { get; set; }

        [JsonIgnore]
        public virtual ICollection<TokenModel>? Tokens { get; set; }

        [JsonIgnore]
        public virtual ICollection<LinkModel>? Links { get; set; }
    }

    public enum Role
    {
        User,
        Admin,
        HighestAdmin
    }
}
