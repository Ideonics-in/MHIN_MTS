using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MTSDB : DbContext
    {
        public MTSDB() : base("name=MTSDBConnectionString")
        {

        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BOM>()
                .HasKey(c => new { c.FGPartID, c.PartID});

            modelBuilder.Entity<Part>()
                .HasMany(c => c.BOM)
                .WithRequired()
                .HasForeignKey(c => c.PartID);

            modelBuilder.Entity<FGPart>()
                .HasMany(c => c.BOM)
                .WithRequired()
                .HasForeignKey(c => c.FGPartID);
        }
        public DbSet<Part> Parts { get; set; }
        public DbSet<FromStores> FromStores { get; set; }
        public DbSet<ToLine> ToLine { get; set; }
        public DbSet<ToStores> ToStores { get; set; }
        public DbSet<BOM> BOMs { get; set; }
        public DbSet<FGPart> FGParts { get; set; }
        public DbSet<FG> FGs { get; set; }
        public DbSet<LineRejection> LineRejections { get; set; }

        public DbSet<FromStoresToLines> FromStoresToLines { get; set; }
    }
}
