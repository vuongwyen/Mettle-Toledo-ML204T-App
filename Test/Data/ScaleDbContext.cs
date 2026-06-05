using Microsoft.EntityFrameworkCore;
using Test.Services;

namespace Test.Data
{
    public class ScaleDbContext : DbContext
    {
        public DbSet<ScaleRecord> ScaleRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(DatabaseHelper.GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the ScaleRecords table mapping
            modelBuilder.Entity<ScaleRecord>(entity =>
            {
                entity.ToTable("ScaleRecords");
                entity.HasKey(e => e.Id);
                
                // Keep Weight as TEXT in SQLite to match existing architecture (decimal preservation)
                entity.Property(e => e.Weight).HasColumnType("TEXT");
            });
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
