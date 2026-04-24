using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Web.Models.Entities;

namespace PortalAcademico.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Código de curso único
            builder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // 2. Un usuario no puede estar matriculado más de una vez en el mismo curso
            builder.Entity<Matricula>()
                .HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            // 3. Restricciones a nivel de base de datos (Check Constraints)
            builder.Entity<Curso>()
                .ToTable(t => 
                {
                    // Creditos > 0
                    t.HasCheckConstraint("CK_Curso_Creditos", "Creditos > 0");
                    // HorarioInicio < HorarioFin
                    t.HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin");
                });
        }
    }
}
