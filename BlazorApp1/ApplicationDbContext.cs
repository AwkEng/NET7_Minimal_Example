using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BlazorApp1
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<StateWithGeometry> StatesWithGeometry { get; set; }
        public DbSet<StateWithoutGeometry> StatesWithoutGeometry { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("postgis");

            builder.Entity<StateWithGeometry>(b =>
            {
                b.Property(e => e.Border)
                    .HasColumnType("geography (geometry)");
            });

            builder.Entity<StateWithoutGeometry>(b =>
            {
                
            });

        }
    }

    public class StateWithGeometry
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Abbreviation { get; set; } = default!;
        public int FipsCode { get; set; }
        public Geometry Border { get; set; } = default!;
    }

    public class StateWithoutGeometry
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Abbreviation { get; set; } = default!;
        public int FipsCode { get; set; }
    }
}
