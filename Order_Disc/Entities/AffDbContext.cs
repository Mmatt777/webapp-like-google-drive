using Microsoft.EntityFrameworkCore;

namespace Order_Disc.Entities
{
    public class AffDbContext : DbContext
    {
        public AffDbContext(DbContextOptions<AffDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccounts> UserAccounts { get; set; }
        public DbSet<FolderEntity> Folders { get; set; }
        public DbSet<FileEntity> Files { get; set; }
        public DbSet<FolderShare> FolderShares { get; set; }

        public DbSet<FileShareEntity> FileShares { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FolderEntity>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FileEntity>()
                .HasOne(f => f.Folder)
                .WithMany(f => f.Files)
                .HasForeignKey(f => f.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FolderEntity>()
                .Property(f => f.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<FileEntity>()
                .Property(f => f.UploadDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<FolderEntity>()
                .HasIndex(f => f.UserAccountId)
                .HasDatabaseName("IX_FolderEntity_UserAccountId");

            modelBuilder.Entity<FileEntity>()
                .HasIndex(f => f.FileName)
                .HasDatabaseName("IX_FileEntity_FileName");

            modelBuilder.Entity<FolderShare>()
                .HasOne(fs => fs.Folder)
                .WithMany(f => f.SharedFolders)
                .HasForeignKey(fs => fs.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FolderShare>()
                .HasOne(fs => fs.SharedByUser)
                .WithMany()
                .HasForeignKey(fs => fs.SharedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FolderShare>()
                .HasOne(fs => fs.SharedWithUser)
                .WithMany()
                .HasForeignKey(fs => fs.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FileShareEntity>()
                  .HasOne(fs => fs.File)
                  .WithMany(f => f.SharedFiles)
                  .HasForeignKey(fs => fs.FileId)
                  .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FileShareEntity>()
                  .HasOne(fs => fs.SharedByUser)
                  .WithMany()
                  .HasForeignKey(fs => fs.SharedByUserId)
                  .OnDelete(DeleteBehavior.Restrict); 

                 modelBuilder.Entity<FileShareEntity>()
                  .HasOne(fs => fs.SharedWithUser)
                  .WithMany()
                  .HasForeignKey(fs => fs.SharedWithUserId)
                  .OnDelete(DeleteBehavior.Restrict);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name }, LogLevel.Information);

            base.OnConfiguring(optionsBuilder);
        }


    }
}
