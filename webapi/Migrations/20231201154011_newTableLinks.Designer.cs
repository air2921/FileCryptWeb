﻿// <auto-generated />
using System;
using System.Net;
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
    [Migration("20231201154011_newTableLinks")]
    partial class newTableLinks
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
                        .HasColumnType("text");

                    b.Property<bool?>("is_allowed_requesting")
                        .HasColumnType("boolean");

                    b.Property<bool?>("is_allowed_unknown_ip")
                        .HasColumnType("boolean");

                    b.Property<bool?>("is_tracking_ip")
                        .HasColumnType("boolean");

                    b.Property<IPAddress>("remote_ip")
                        .HasColumnType("inet");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("api_id");

                    b.HasIndex("api_key")
                        .IsUnique();

                    b.HasIndex("user_id")
                        .IsUnique();

                    b.ToTable("api");
                });

            modelBuilder.Entity("webapi.Models.FileMimeModel", b =>
                {
                    b.Property<int>("mime_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("mime_id"));

                    b.Property<string>("mime_name")
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
                        .HasColumnType("text");

                    b.Property<string>("file_name")
                        .HasColumnType("text");

                    b.Property<DateTime?>("operation_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("type")
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

                    b.Property<string>("person_internal_key")
                        .HasColumnType("text");

                    b.Property<string>("private_key")
                        .HasColumnType("text");

                    b.Property<string>("received_internal_key")
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("key_id");

                    b.HasIndex("user_id")
                        .IsUnique();

                    b.ToTable("keys");
                });

            modelBuilder.Entity("webapi.Models.LinkModel", b =>
                {
                    b.Property<int>("link_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("link_id"));

                    b.Property<DateTime?>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("expiry_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("is_used")
                        .HasColumnType("boolean");

                    b.Property<string>("u_token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("link_id");

                    b.HasIndex("user_id");

                    b.ToTable("links");
                });

            modelBuilder.Entity("webapi.Models.NotificationModel", b =>
                {
                    b.Property<int>("notification_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("notification_id"));

                    b.Property<bool?>("is_checked")
                        .HasColumnType("boolean");

                    b.Property<string>("message")
                        .HasColumnType("text");

                    b.Property<string>("message_header")
                        .HasColumnType("text");

                    b.Property<string>("priority")
                        .HasColumnType("text");

                    b.Property<int>("receiver_id")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("send_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("sender_id")
                        .HasColumnType("integer");

                    b.HasKey("notification_id");

                    b.HasIndex("receiver_id");

                    b.HasIndex("sender_id");

                    b.ToTable("notifications");
                });

            modelBuilder.Entity("webapi.Models.OfferModel", b =>
                {
                    b.Property<int>("offer_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("offer_id"));

                    b.Property<DateTime?>("created_at")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("is_accepted")
                        .HasColumnType("boolean");

                    b.Property<string>("offer_body")
                        .HasColumnType("text");

                    b.Property<string>("offer_header")
                        .HasColumnType("text");

                    b.Property<string>("offer_type")
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

                    b.Property<DateTime?>("expiry_date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("refresh_token")
                        .HasColumnType("text");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("token_id");

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
                        .HasColumnType("text");

                    b.Property<string>("password_hash")
                        .HasColumnType("text");

                    b.Property<string>("role")
                        .HasColumnType("text");

                    b.Property<string>("username")
                        .HasColumnType("text");

                    b.HasKey("id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("webapi.Models.ApiModel", b =>
                {
                    b.HasOne("webapi.Models.UserModel", "User")
                        .WithOne("API")
                        .HasForeignKey("webapi.Models.ApiModel", "user_id")
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

            modelBuilder.Entity("webapi.Models.UserModel", b =>
                {
                    b.Navigation("API");

                    b.Navigation("Files");

                    b.Navigation("Keys");

                    b.Navigation("Links");

                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
