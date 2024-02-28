﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace webapi.Models
{
    [Table("users")]
    public class UserModel
    {
        [Key]
        public int id { get; set; }

        public string username { get; set; }

        public string role { get; set; }

        [EmailAddress]
        public string email { get; set; }

        public string password { get; set; }

        public bool is_2fa_enabled { get; set; }

        public bool is_blocked { get; set; }

        [JsonIgnore]
        public KeyModel? Keys { get; set; }

        [JsonIgnore]
        public ICollection<KeyStorageModel>? KeyStorages { get; set; }

        [JsonIgnore]
        public ICollection<FileModel>? Files { get; set; }

        [JsonIgnore]
        public TokenModel? Tokens { get; set; }

        [JsonIgnore]
        public ICollection<ApiModel>? API { get; set; }

        [JsonIgnore]
        public ICollection<LinkModel>? Links { get; set; }
    }

    public enum Role
    {
        User,
        Admin,
        HighestAdmin
    }
}
