﻿// <auto-generated />
using System;
using ChatServer.DataBase.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ChatServer.DataBase.Migrations
{
    [DbContext(typeof(ChatServerDbContext))]
    [Migration("20250131110141_init_1")]
    partial class init_1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.ChatPrivate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserFromId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("UserTargetId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("UserFromId");

                    b.HasIndex("UserTargetId");

                    b.ToTable("ChatPrivates");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.FriendRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Group")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<DateTime>("GroupTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("User1Id")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("User2Id")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("User1Id");

                    b.HasIndex("User2Id");

                    b.ToTable("FriendRelations");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.FriendRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Group")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("IsAccept")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSolved")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("RequestTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("SolveTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserFromId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("UserTargetId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("UserFromId");

                    b.HasIndex("UserTargetId");

                    b.ToTable("FriendRequests");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.SacurityQuestion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("SacurityQuestions");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<DateOnly>("Birth")
                        .HasColumnType("date");

                    b.Property<int>("HeadCount")
                        .HasColumnType("int");

                    b.Property<int>("HeadIndex")
                        .HasColumnType("int");

                    b.Property<string>("Introduction")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<bool>("IsMale")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("RegisteTime")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.UserOnline", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("LoginTime")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("LogoutTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserOnlines");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.ChatPrivate", b =>
                {
                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "UserFrom")
                        .WithMany()
                        .HasForeignKey("UserFromId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "UserTarget")
                        .WithMany()
                        .HasForeignKey("UserTargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserFrom");

                    b.Navigation("UserTarget");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.FriendRelation", b =>
                {
                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "User1")
                        .WithMany()
                        .HasForeignKey("User1Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "User2")
                        .WithMany()
                        .HasForeignKey("User2Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User1");

                    b.Navigation("User2");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.FriendRequest", b =>
                {
                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "UserFrom")
                        .WithMany()
                        .HasForeignKey("UserFromId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "UserTarget")
                        .WithMany()
                        .HasForeignKey("UserTargetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserFrom");

                    b.Navigation("UserTarget");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.SacurityQuestion", b =>
                {
                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ChatServer.DataBase.DataBase.DataEntity.UserOnline", b =>
                {
                    b.HasOne("ChatServer.DataBase.DataBase.DataEntity.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
