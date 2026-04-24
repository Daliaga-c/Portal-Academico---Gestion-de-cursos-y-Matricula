using Microsoft.AspNetCore.Identity;
using PortalAcademico.Web.Models.Entities;

namespace PortalAcademico.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Crear Roles
            string[] roles = { "Coordinador", "Estudiante" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Crear Usuario Coordinador
            var adminUser = await userManager.FindByEmailAsync("coordinador@universidad.edu");
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = "coordinador@universidad.edu", Email = "coordinador@universidad.edu" };
                await userManager.CreateAsync(adminUser, "Password123!");
                await userManager.AddToRoleAsync(adminUser, "Coordinador");
            }

            // 3. Crear 3 Cursos Activos
            if (!context.Cursos.Any())
            {
                var cursos = new List<Curso>
                {
                    new Curso { Codigo = "CS101", Nombre = "Introducción a la Programación", Creditos = 4, CupoMaximo = 30, HorarioInicio = new TimeSpan(8, 0, 0), HorarioFin = new TimeSpan(10, 0, 0), Activo = true },
                    new Curso { Codigo = "DB201", Nombre = "Bases de Datos Avanzadas", Creditos = 3, CupoMaximo = 25, HorarioInicio = new TimeSpan(14, 0, 0), HorarioFin = new TimeSpan(16, 0, 0), Activo = true },
                    new Curso { Codigo = "WEB301", Nombre = "Desarrollo Web Full Stack", Creditos = 5, CupoMaximo = 20, HorarioInicio = new TimeSpan(18, 0, 0), HorarioFin = new TimeSpan(21, 0, 0), Activo = true }
                };

                await context.Cursos.AddRangeAsync(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}
