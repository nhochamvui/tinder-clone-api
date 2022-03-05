﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TinderClone.Models;

namespace TinderClone.Migrations
{
    [DbContext(typeof(TinderContext))]
    [Migration("20220302031939_Add_User_Profile_Rela")]
    partial class Add_User_Profile_Rela
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.7")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("TinderClone.Models.DiscoverySettings", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("AgePreferenceCheck")
                        .HasColumnType("boolean");

                    b.Property<int>("DistancePreference")
                        .HasColumnType("integer");

                    b.Property<bool>("DistancePreferenceCheck")
                        .HasColumnType("boolean");

                    b.Property<int>("LikeCount")
                        .HasColumnType("integer");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<int>("LookingForGender")
                        .HasColumnType("integer");

                    b.Property<int>("MaxAge")
                        .HasColumnType("integer");

                    b.Property<int>("MinAge")
                        .HasColumnType("integer");

                    b.Property<int>("SuperlikeCount")
                        .HasColumnType("integer");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserID");

                    b.ToTable("DiscoverySettings");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            AgePreferenceCheck = false,
                            DistancePreference = 1,
                            DistancePreferenceCheck = false,
                            LikeCount = 0,
                            Location = "Hồ Chí Minh",
                            LookingForGender = 1,
                            MaxAge = 25,
                            MinAge = 18,
                            SuperlikeCount = 0,
                            UserID = 1L
                        });
                });

            modelBuilder.Entity("TinderClone.Models.Matches", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("DateOfMatch")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsDislike")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsMatched")
                        .HasColumnType("boolean");

                    b.Property<long>("MyId")
                        .HasColumnType("bigint");

                    b.Property<long>("ObjectId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("TinderClone.Models.Messages", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("content")
                        .HasColumnType("text");

                    b.Property<long>("fromID")
                        .HasColumnType("bigint");

                    b.Property<bool>("isRead")
                        .HasColumnType("boolean");

                    b.Property<bool>("isSent")
                        .HasColumnType("boolean");

                    b.Property<long>("timeStamp")
                        .HasColumnType("bigint");

                    b.Property<long>("toID")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("fromID");

                    b.HasIndex("toID");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("TinderClone.Models.Profile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("About")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserID")
                        .IsUnique();

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("TinderClone.Models.ProfileImages", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ImageURL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("UserID")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserID");

                    b.ToTable("ProfileImages");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            ImageURL = "img/unclebob.jpg",
                            UserID = 1L
                        },
                        new
                        {
                            Id = 2L,
                            ImageURL = "img/unclebob1.jpg",
                            UserID = 1L
                        },
                        new
                        {
                            Id = 3L,
                            ImageURL = "img/auntbob.jpg",
                            UserID = 2L
                        });
                });

            modelBuilder.Entity("TinderClone.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("About")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            DateOfBirth = new DateTime(1998, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "uncle.bob@gmail.com",
                            Gender = 0,
                            Location = "Hồ Chí Minh",
                            Name = "Uncle Bob",
                            Password = "1234",
                            UserName = "unclebob"
                        },
                        new
                        {
                            Id = 2L,
                            DateOfBirth = new DateTime(1999, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "aunt.bob@gmail.com",
                            Gender = 1,
                            Location = "Cần Thơ",
                            Name = "Aunt Bob",
                            Password = "1234",
                            UserName = "auntbob"
                        });
                });

            modelBuilder.Entity("TinderClone.Models.DiscoverySettings", b =>
                {
                    b.HasOne("TinderClone.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TinderClone.Models.Messages", b =>
                {
                    b.HasOne("TinderClone.Models.User", null)
                        .WithMany()
                        .HasForeignKey("fromID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TinderClone.Models.User", null)
                        .WithMany()
                        .HasForeignKey("toID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TinderClone.Models.Profile", b =>
                {
                    b.HasOne("TinderClone.Models.User", "User")
                        .WithOne("Profile")
                        .HasForeignKey("TinderClone.Models.Profile", "UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TinderClone.Models.ProfileImages", b =>
                {
                    b.HasOne("TinderClone.Models.User", "User")
                        .WithMany("ProfileImages")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TinderClone.Models.User", b =>
                {
                    b.Navigation("Profile");

                    b.Navigation("ProfileImages");
                });
#pragma warning restore 612, 618
        }
    }
}
