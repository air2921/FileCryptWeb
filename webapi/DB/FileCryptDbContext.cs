using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.DB
{
    public class FileCryptDbContext : DbContext
    {
        private const int USER_ID = 264950;

        private const string EMAIL = "FileCrypt147@gmail.com";
        private const string USERNAME = "FileCrypt";
        private const string PASSWORD = "$2a$11$vOj8wlzPuP/.7Bdj4YOU.O7Dki4.eZwSztFfndoJNETtFsI51Lfny"; //FileCrypt123
        private const string ROLE = "HighestAdmin";
        private const bool TWO_FA_ENABLED = false;
        private const bool IS_BLOCKED = false;

        private const int KEYS_ID = 265000;
        private const string PRIVATE_KEY = "Qfz+jKaBDu3ToOWlabZ5tPxZhCJDcWNhlruuVw2yUgg+bkBpaurIX1F34QYMJpFlY7tOrUSn63pmo4E56eEnSA==";
        private const string INTERNAL_KEY = "/oalkI8PEHT8/moTD40CM2mjinp7VYc4jTfLcl1PELFUBZPS08Q3byx43IQmg7q2/e5aTEExdc0cZopkaULC+Q==";

        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<KeyModel> Keys { get; set; }
        public virtual DbSet<FileModel> Files { get; set; }
        public virtual DbSet<NotificationModel> Notifications { get; set; }
        public virtual DbSet<OfferModel> Offers { get; set; }
        public virtual DbSet<FileMimeModel> Mimes { get; set; }
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
            #region Has one with one

            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Keys)
                .WithOne(k => k.User)
                .HasForeignKey<KeyModel>(k => k.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region Has one with many

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

            #endregion
        }

        public void Initial()
        {
            using var context = new FileCryptDbContext((DbContextOptions<FileCryptDbContext>)_options);

            if (!context.Users.Any())
            {
                context.Users.Add(new UserModel
                {
                    id = USER_ID,
                    email = EMAIL.ToLowerInvariant(),
                    username = USERNAME,
                    password = PASSWORD,
                    role = ROLE,
                    is_2fa_enabled = TWO_FA_ENABLED,
                    is_blocked = IS_BLOCKED
                });

                context.Keys.Add(new KeyModel
                {
                    user_id = USER_ID,
                    key_id = KEYS_ID,
                    private_key = PRIVATE_KEY,
                    internal_key = INTERNAL_KEY
                });

                context.SaveChanges();
            }
        }
    }
}
