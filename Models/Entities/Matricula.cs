using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Web.Models.Entities
{
    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        public IdentityUser? Usuario { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }
}
