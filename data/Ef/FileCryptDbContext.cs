using domain.Models;
using Microsoft.EntityFrameworkCore;

namespace data_access.Ef
{
    public class FileCryptDbContext : DbContext, ISeed
    {
        private const int USER_ID = 264950;

        private const string EMAIL = "FileCrypt147@gmail.com";
        private const string USERNAME = "FileCrypt";
        private const string PASSWORD = "$2a$11$vOj8wlzPuP/.7Bdj4YOU.O7Dki4.eZwSztFfndoJNETtFsI51Lfny"; //FileCrypt123
        private const string ROLE = "HighestAdmin";
        private const bool TWO_FA_ENABLED = false;
        private const bool IS_BLOCKED = false;

        public virtual DbSet<UserModel> Users { get; set; }
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

        public async Task AdminSeed()
        {
            using var context = new FileCryptDbContext((DbContextOptions<FileCryptDbContext>)_options);
            if (context.Users.FirstOrDefaultAsync(x => x.id.Equals(USER_ID)) is null)
            {
                await context.Users.AddAsync(new UserModel
                {
                    id = USER_ID,
                    username = USERNAME,
                    email = EMAIL,
                    password = PASSWORD,
                    role = ROLE,
                    is_2fa_enabled = TWO_FA_ENABLED,
                    is_blocked = IS_BLOCKED
                });

                await context.SaveChangesAsync();
            }
        }
    }

    public interface ISeed
    {
        Task AdminSeed();
    }
}
