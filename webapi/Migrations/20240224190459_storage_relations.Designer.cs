﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using webapi.DB;

#nullable disable

namespace webapi.Migrations
{
    [DbContext(typeof(FileCryptDbContext))]
    [Migration("20240224190459_storage_relations")]
    partial class storage_relations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("webapi.Models.ApiModel", b =>
                {
                    b.Property<int>("api_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("api_id"));

                    b.Property<string>("api_key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("expiry_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("is_blocked")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("last_time_activity")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("max_request_of_day")
                        .HasColumnType("integer");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("api_id");

                    b.HasIndex("api_key")
                        .IsUnique();

                    b.HasIndex("user_id");

                    b.ToTable("api");
                });

            modelBuilder.Entity("webapi.Models.FileMimeModel", b =>
                {
                    b.Property<int>("mime_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("mime_id"));

                    b.Property<string>("mime_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("mime_id");

                    b.ToTable("allowedmime");
                });

            modelBuilder.Entity("webapi.Models.FileModel", b =>
                {
                    b.Property<int>("file_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("file_id"));

                    b.Property<string>("file_mime")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("file_mime_category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("file_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("operation_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("file_id");

                    b.HasIndex("user_id");

                    b.ToTable("files");
                });

            modelBuilder.Entity("webapi.Models.KeyModel", b =>
                {
                    b.Property<int>("key_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("key_id"));

                    b.Property<string>("internal_key")
                        .HasColumnType("text");

                    b.Property<string>("private_key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("received_key")
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("key_id");

                    b.HasIndex("user_id")
                        .IsUnique();

                    b.ToTable("keys");
                });

            modelBuilder.Entity("webapi.Models.KeyStorageItemModel", b =>
                {
                    b.Property<decimal>("key_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("key_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("key_value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("storage_id")
                        .HasColumnType("integer");

                    b.HasKey("key_id");

                    b.HasIndex("storage_id");

                    b.ToTable("storage_items");
                });

            modelBuilder.Entity("webapi.Models.KeyStorageModel", b =>
                {
                    b.Property<int>("storage_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("storage_id"));

                    b.Property<string>("access_code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("encrypt")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("last_time_modified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("storage_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("storage_id");

                    b.HasIndex("user_id");

                    b.ToTable("storages");
                });

            modelBuilder.Entity("webapi.Models.LinkModel", b =>
                {
                    b.Property<int>("link_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("link_id"));

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("expiry_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("u_token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("link_id");

                    b.HasIndex("u_token")
                        .IsUnique();

                    b.HasIndex("user_id");

                    b.ToTable("links");
                });

            modelBuilder.Entity("webapi.Models.NotificationModel", b =>
                {
                    b.Property<int>("notification_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("notification_id"));

                    b.Property<bool>("is_checked")
                        .HasColumnType("boolean");

                    b.Property<string>("message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("message_header")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("priority")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("send_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("notification_id");

                    b.HasIndex("user_id");

                    b.ToTable("notifications");
                });

            modelBuilder.Entity("webapi.Models.OfferModel", b =>
                {
                    b.Property<int>("offer_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("offer_id"));

                    b.Property<DateTime>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("is_accepted")
                        .HasColumnType("boolean");

                    b.Property<string>("offer_body")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("offer_header")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("offer_type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("receiver_id")
                        .HasColumnType("integer");

                    b.Property<int>("sender_id")
                        .HasColumnType("integer");

                    b.HasKey("offer_id");

                    b.HasIndex("receiver_id");

                    b.HasIndex("sender_id");

                    b.ToTable("offers");
                });

            modelBuilder.Entity("webapi.Models.TokenModel", b =>
                {
                    b.Property<int>("token_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("token_id"));

                    b.Property<DateTime>("expiry_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("refresh_token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("token_id");

                    b.HasIndex("refresh_token")
                        .IsUnique();

                    b.HasIndex("user_id")
                        .IsUnique();

                    b.ToTable("tokens");
                });

            modelBuilder.Entity("webapi.Models.UserModel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("is_2fa_enabled")
                        .HasColumnType("boolean");

                    b.Property<bool>("is_blocked")
                        .HasColumnType("boolean");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.HasIndex("email")
                        .IsUnique();

                    b.ToTable("users");
                });

            modelBuilder.Entity("webapi.Models.ApiModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithMany("API")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.FileModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithMany("Files")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.KeyModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithOne("Keys")
                        .HasForeignKey("webapi.Models.KeyModel", "user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.KeyStorageItemModel", b =>
                {
                    b.HasOne("webapi.Models.KeyStorageModel", "KeyStorage")
                        .WithMany("StorageItems")
                        .HasForeignKey("storage_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("KeyStorage");
                });

            modelBuilder.Entity("webapi.Models.KeyStorageModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithMany("KeyStorages")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.LinkModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithMany("Links")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.NotificationModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "Receiver")
                        .WithMany()
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("webapi.Models.OfferModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "Receiver")
                        .WithMany()
                        .HasForeignKey("receiver_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("webapi.Models.UserModel", "Sender")
                        .WithMany()
                        .HasForeignKey("sender_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("webapi.Models.TokenModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithOne("Tokens")
                        .HasForeignKey("webapi.Models.TokenModel", "user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("webapi.Models.KeyStorageModel", b =>
                {
                    b.Navigation("StorageItems");
                });

            modelBuilder.Entity("webapi.Models.UserModel", b =>
                {
                    b.Navigation("API");

                    b.Navigation("Files");

                    b.Navigation("KeyStorages");

                    b.Navigation("Keys");

                    b.Navigation("Links");

                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
