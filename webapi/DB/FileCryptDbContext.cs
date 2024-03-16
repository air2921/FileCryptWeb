using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.DB
{
    public class FileCryptDbContext : DbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }
        public virtual DbSet<KeyModel> Keys { get; set; }
        public virtual DbSet<FileModel> Files { get; set; }
        public virtual DbSet<NotificationModel> Notifications { get; set; }
        public virtual DbSet<OfferModel> Offers { get; set; }
        public virtual DbSet<FileMimeModel> Mimes { get; set; }
        public virtual DbSet<TokenModel> Tokens { get; set; }
        public virtual DbSet<ApiModel> API { get; set; }
        public virtual DbSet<LinkModel> Links { get; set; }
        public virtual DbSet<KeyStorageModel> KeyStorages { get; set; }
        public virtual DbSet<KeyStorageItemModel> KeyStorageItems { get; set; }

        public FileCryptDbContext(DbContextOptions<FileCryptDbContext> options) : base(options)
        {
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

            modelBuilder.Entity<ApiModel>()
                .HasOne(a => a.User)
                .WithMany(a => a.API)
                .HasForeignKey(a => a.user_id)
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

            modelBuilder.Entity<ApiModel>()
                .HasIndex(a => a.api_key)
                .IsUnique();

            modelBuilder.Entity<LinkModel>()
                .HasIndex(l => l.u_token)
                .IsUnique();

            #endregion
        }
    }
}
