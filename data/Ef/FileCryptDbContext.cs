using domain.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace data_access.Ef
{
    public class FileCryptDbContext : DbContext, ISeed
    {
        private const string HIGHEST_EMAIL = "Highest244@gmail.com";
        private const string HIGHEST_USERNAME = "Highest";
        private const string HIGHEST_PASSWORD = "$2a$11$NbXvhbJEkKlsF2axlu3/B.WrlF.fm2vvD.1Bq7Qx3yGotVM28UGKe"; //123
        private const string HIGHEST_ROLE = "HighestAdmin";

        private const string SIMPLE_EMAIL = "Simple244@gmail.com";
        private const string SIMPLE_USERNAME = "Simple";
        private const string SIMPLE_PASSWORD = "$2a$11$PPjoeOlyveAarHmMifiMiOeJbYf3IMSRJAHZ/IrwvQjoKt.k2jpuq"; //123456
        private const string SIMPLE_ROLE = "Admin";

        private const string USER_EMAIL = "User244@gmail.com";
        private const string USER_PASSWORD = "$2a$11$67rCu/FXexJpoBKzJlssv.pV0iAw7KTWnCD0G5Foc3QfnR.bmFgC6"; //123456789
        private const string USER_USERNAME = "User";
        private const string USER_ROLE = "User";

        private const string CODE = "$2a$11$eRfKDDVUFY9hm/kIiDuBAOKBgq/qfUyCtra4djWiwAfh3XCvxm05K"; //23663


        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<ActivityModel> Activity { get; set; }
        public virtual DbSet<FileModel> Files { get; set; }
        public virtual DbSet<NotificationModel> Notifications { get; set; }
        public virtual DbSet<OfferModel> Offers { get; set; }
        public virtual DbSet<MimeModel> Mimes { get; set; }
        public virtual DbSet<TokenModel> Tokens { get; set; }
        public virtual DbSet<LinkModel> Links { get; set; }
        public virtual DbSet<KeyStorageModel> KeyStorages { get; set; }
        public virtual DbSet<KeyStorageItemModel> KeyStorageItems { get; set; }

        private readonly DbContextOptions _options;

        public FileCryptDbContext(DbContextOptions<FileCryptDbContext> options) : base(options)
        {
            _options = options;
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Has one with many

            modelBuilder.Entity<ActivityModel>()
                .HasOne(a => a.User)
                .WithMany(u => u.Activity)
                .HasForeignKey(a => a.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TokenModel>()
                .HasOne(f => f.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(f => f.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FileModel>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfferModel>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.sender_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OfferModel>()
                .HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.receiver_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LinkModel>()
                .HasOne(l => l.User)
                .WithMany(l => l.Links)
                .HasForeignKey(l => l.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KeyStorageModel>()
                .HasOne(k => k.User)
                .WithMany(k => k.KeyStorages)
                .HasForeignKey(s => s.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KeyStorageItemModel>()
                .HasOne(k => k.KeyStorage)
                .WithMany(k => k.StorageItems)
                .HasForeignKey(s => s.storage_id)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Property indexes

            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<TokenModel>()
                .HasIndex(t => t.refresh_token)
                .IsUnique();

            modelBuilder.Entity<LinkModel>()
                .HasIndex(l => l.u_token)
                .IsUnique();

            modelBuilder.Entity<MimeModel>()
                .HasIndex(m => m.mime_name)
                .IsUnique();

            #endregion
        }

        public object AdminSeed()
        {
            using var context = new FileCryptDbContext((DbContextOptions<FileCryptDbContext>)_options);
            var user1 = context.Users.Find(123);
            var user2 = context.Users.Find(123456);
            var user3 = context.Users.Find(123456789);
            if (user1 is null)
            {
                user1 = new UserModel
                {
                    id = 123,
                    avatarId = null,
                    avatar_content_type = null,
                    avatar_name = null,
                    email = HIGHEST_EMAIL.ToLowerInvariant(),
                    username = HIGHEST_USERNAME,
                    password = HIGHEST_PASSWORD,
                    last_time_password_modified = DateTime.UtcNow,
                    role = HIGHEST_ROLE,
                    is_2fa_enabled = false,
                    is_blocked = false,
                };
                context.Users.Add(user1);

                context.KeyStorages.Add(new KeyStorageModel
                {
                    storage_id = 123,
                    storage_name = "123",
                    user_id = 123,
                    access_code = CODE,
                    last_time_modified = DateTime.UtcNow
                });

                context.KeyStorageItems.AddRange([
                    new KeyStorageItemModel {
                        storage_id = 123,
                        created_at = DateTime.UtcNow,
                        key_id = 123,
                        key_name = "1",
                        key_value = "QMdssjO/4WmBEcdxFR8ZxDYtGd4wmL+bf1UKMuh3x2A="
                    },
                    new KeyStorageItemModel {
                        storage_id = 123,
                        created_at = DateTime.UtcNow,
                        key_id = 124,
                        key_name = "2",
                        key_value = "GZkkxJ9F3iNfm7yRhGPAXI5xcFznyfANQ1+cup3KZzg="
                    },
                    new KeyStorageItemModel {
                        storage_id = 123,
                        created_at = DateTime.UtcNow,
                        key_id = 125,
                        key_name = "3",
                        key_value = "Hp04C2IZnJa4lztVGx9qMabluHWKXYww3JTicqCEeP4="
                    },
                ]);
            }

            if (user2 is null)
            {
                user2 = new UserModel
                {
                    id = 123456,
                    avatarId = null,
                    avatar_content_type = null,
                    avatar_name = null,
                    email = SIMPLE_EMAIL.ToLowerInvariant(),
                    username = SIMPLE_USERNAME,
                    password = SIMPLE_PASSWORD,
                    last_time_password_modified = DateTime.UtcNow,
                    role = SIMPLE_ROLE,
                    is_2fa_enabled = false,
                    is_blocked = false,
                };
                context.Users.Add(user2);

                context.KeyStorages.Add(new KeyStorageModel
                {
                    storage_id = 234,
                    storage_name = "123456",
                    user_id = 123456,
                    access_code = CODE,
                    last_time_modified = DateTime.UtcNow
                });

                context.KeyStorageItems.AddRange([
                    new KeyStorageItemModel {
                        storage_id = 234,
                        created_at = DateTime.UtcNow,
                        key_id = 126,
                        key_name = "4",
                        key_value = "mhGrL7dA+9p94i6ovZMILGHjxkfZkx9OdHt8ZINxO80="
                    },
                    new KeyStorageItemModel {
                        storage_id = 234,
                        created_at = DateTime.UtcNow,
                        key_id = 127,
                        key_name = "5",
                        key_value = "AFmYgYVWL6s/0lfR8sWuJT9oIWFGkHMI9Ly8hxi0M4o="
                    },
                    new KeyStorageItemModel {
                        storage_id = 234,
                        created_at = DateTime.UtcNow,
                        key_id = 128,
                        key_name = "6",
                        key_value = "hAXO8qGrPZ0MKXuZnIbFtpsqzlkCbv9fwTupPBnUv3U="
                    },
                ]);
            }

            if (user3 is null)
            {
                user3 = new UserModel
                {
                    id = 123456789,
                    avatarId = null,
                    avatar_content_type = null,
                    avatar_name = null,
                    email = USER_EMAIL.ToLowerInvariant(),
                    username = USER_USERNAME,
                    password = USER_PASSWORD,
                    last_time_password_modified = DateTime.UtcNow,
                    role = USER_ROLE,
                    is_2fa_enabled = false,
                    is_blocked = false,
                };
                context.Users.Add(user3);

                context.KeyStorages.Add(new KeyStorageModel
                {
                    storage_id = 345,
                    storage_name = "123456789",
                    user_id = 123456789,
                    access_code = CODE,
                    last_time_modified = DateTime.UtcNow
                });

                context.KeyStorageItems.AddRange([
                    new KeyStorageItemModel {
                        storage_id = 345,
                        created_at = DateTime.UtcNow,
                        key_id = 129,
                        key_name = "7",
                        key_value = "uOfhQ7ONycUUZZWN/5xbXG+UH5f6A4t/KhLsGyzsRcc="
                    },
                    new KeyStorageItemModel {
                        storage_id = 345,
                        created_at = DateTime.UtcNow,
                        key_id = 130,
                        key_name = "8",
                        key_value = "BxvBLTNXv1T2HPs6ECy4ZEQ7NZdKqdcsNOu977XZpxY="
                    },
                    new KeyStorageItemModel {
                        storage_id = 345,
                        created_at = DateTime.UtcNow,
                        key_id = 131,
                        key_name = "9",
                        key_value = "JsQOXx/wGGJy94Ynwz5zUWunJTLkgZhs8mzIZXTm8go="
                    },
                ]);
            }

            context.SaveChanges();
            var users = context.Users.ToList();
            var storages = context.KeyStorages.ToList();
            var keys = context.KeyStorageItems.ToList();
            return new { users, storages, keys };
        }
    }

    public interface ISeed
    {
        object AdminSeed();
    }
}
