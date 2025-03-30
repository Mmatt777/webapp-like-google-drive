﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Order_Disc.Entities;

#nullable disable

namespace Order_Disc.Migrations
{
    [DbContext(typeof(AffDbContext))]
    [Migration("20241217170258_FixFileEntitySharedByUserRelation")]
    partial class FixFileEntitySharedByUserRelation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Order_Disc.Entities.FileEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("FolderId")
                        .HasColumnType("int");

                    b.Property<int?>("FolderShareId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsImportant")
                        .HasColumnType("bit");

                    b.Property<string>("OrginalFilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("SizeInBytes")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("UploadDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.HasKey("Id");

                    b.HasIndex("FileName")
                        .HasDatabaseName("IX_FileEntity_FileName");

                    b.HasIndex("FolderId");

                    b.HasIndex("FolderShareId");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Order_Disc.Entities.FileShareEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccessLevel")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FileId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsImportant")
                        .HasColumnType("bit");

                    b.Property<string>("OrginalFilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShareFilePath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SharedByUserId")
                        .HasColumnType("int");

                    b.Property<int>("SharedWithUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("SharedByUserId");

                    b.HasIndex("SharedWithUserId");

                    b.ToTable("FileShares");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("DefaultAccessLevel")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsImportant")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrginalFolderPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserAccountId")
                        .HasDatabaseName("IX_FolderEntity_UserAccountId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderShare", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AccessLevel")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FolderEntityId")
                        .HasColumnType("int");

                    b.Property<int>("FolderId")
                        .HasColumnType("int");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsImportant")
                        .HasColumnType("bit");

                    b.Property<string>("OrginalFolderPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ShareFolderPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SharedByUserId")
                        .HasColumnType("int");

                    b.Property<int>("SharedWithUserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FolderEntityId");

                    b.HasIndex("FolderId");

                    b.HasIndex("SharedByUserId");

                    b.HasIndex("SharedWithUserId");

                    b.ToTable("FolderShares");
                });

            modelBuilder.Entity("Order_Disc.Entities.UserAccounts", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("Order_Disc.Entities.FileEntity", b =>
                {
                    b.HasOne("Order_Disc.Entities.FolderEntity", "Folder")
                        .WithMany("Files")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Order_Disc.Entities.FolderShare", null)
                        .WithMany("Files")
                        .HasForeignKey("FolderShareId");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("Order_Disc.Entities.FileShareEntity", b =>
                {
                    b.HasOne("Order_Disc.Entities.FileEntity", "File")
                        .WithMany("SharedFiles")
                        .HasForeignKey("FileId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Order_Disc.Entities.UserAccounts", "SharedByUser")
                        .WithMany()
                        .HasForeignKey("SharedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Order_Disc.Entities.UserAccounts", "SharedWithUser")
                        .WithMany()
                        .HasForeignKey("SharedWithUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("File");

                    b.Navigation("SharedByUser");

                    b.Navigation("SharedWithUser");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderEntity", b =>
                {
                    b.HasOne("Order_Disc.Entities.UserAccounts", "User")
                        .WithMany("Folders")
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderShare", b =>
                {
                    b.HasOne("Order_Disc.Entities.FolderEntity", null)
                        .WithMany("SharedWithUsers")
                        .HasForeignKey("FolderEntityId");

                    b.HasOne("Order_Disc.Entities.FolderEntity", "Folder")
                        .WithMany("SharedFolders")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Order_Disc.Entities.UserAccounts", "SharedByUser")
                        .WithMany()
                        .HasForeignKey("SharedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Order_Disc.Entities.UserAccounts", "SharedWithUser")
                        .WithMany()
                        .HasForeignKey("SharedWithUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Folder");

                    b.Navigation("SharedByUser");

                    b.Navigation("SharedWithUser");
                });

            modelBuilder.Entity("Order_Disc.Entities.FileEntity", b =>
                {
                    b.Navigation("SharedFiles");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderEntity", b =>
                {
                    b.Navigation("Files");

                    b.Navigation("SharedFolders");

                    b.Navigation("SharedWithUsers");
                });

            modelBuilder.Entity("Order_Disc.Entities.FolderShare", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("Order_Disc.Entities.UserAccounts", b =>
                {
                    b.Navigation("Folders");
                });
#pragma warning restore 612, 618
        }
    }
}
