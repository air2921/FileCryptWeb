using Microsoft.EntityFrameworkCore;
using webapi.Models;

namespace webapi.DB
{
    public class FileCryptDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<KeyModel> Keys { get; set; }
        public DbSet<FileModel> Files { get; set; }
        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<OfferModel> Offers { get; set; }
        public DbSet<FileMimeModel> Mimes { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }
        public DbSet<ApiModel> API { get; set; }

        public FileCryptDbContext(DbContextOptions<FileCryptDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileModel>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Keys)
                .WithOne(k => k.User)
                .HasForeignKey<KeyModel>(k => k.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.sender_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotificationModel>()
                .HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.receiver_id)
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

            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.Tokens)
                .WithOne(t => t.User)
                .HasForeignKey<TokenModel>(t => t.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserModel>()
                .HasOne(u => u.API)
                .WithOne(a => a.User)
                .HasForeignKey<ApiModel>(a => a.user_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
