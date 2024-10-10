using CitasMedicas.AutenticacionApi.Model;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.AutenticacionApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            base.OnModelCreating(modelBuilder);
        }
    }
}
