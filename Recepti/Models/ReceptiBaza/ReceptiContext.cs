using Microsoft.EntityFrameworkCore;

namespace Recepti.Models.ReceptiBaza
{
    public class ReceptiContext : DbContext
    {
        public ReceptiContext(DbContextOptions<ReceptiContext> options) : base(options) 
        {
        }

        public DbSet<KategorijaRecepta> KategorijaRecepta { get; set; }
        public DbSet<MjernaJedinica> MjernaJedinica { get; set; }
        public DbSet<Recept> Recept { get; set; }
        public DbSet<ReceptKategorija> ReceptKategorija { get; set; }
        public DbSet<ReceptSastojci> ReceptSastojci { get; set; }
        public DbSet<Sastojak> Sastojak { get; set;}
        public DbSet<SlikaOriginalnogRecepta> SlikaOriginalnogRecepta { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KategorijaRecepta>().ToTable("KategorijaRecepta");
            modelBuilder.Entity<MjernaJedinica>().ToTable("MjernaJedinica");
            modelBuilder.Entity<Recept>().ToTable("Recept");
            modelBuilder.Entity<ReceptKategorija>().ToTable("ReceptKategorija");
            modelBuilder.Entity<ReceptSastojci>().ToTable("ReceptSastojci");
            modelBuilder.Entity<Sastojak>().ToTable("Sastojak");
            modelBuilder.Entity<SlikaOriginalnogRecepta>().ToTable("SlikaOriginalnogRecepta");
        }
    }
}
