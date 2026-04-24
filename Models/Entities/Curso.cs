using System.ComponentModel.DataAnnotations;

namespace PortalAcademico.Web.Models.Entities
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayores a 0.")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor a 0.")]
        public int CupoMaximo { get; set; }

        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;
        
        // Relación de navegación
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}
