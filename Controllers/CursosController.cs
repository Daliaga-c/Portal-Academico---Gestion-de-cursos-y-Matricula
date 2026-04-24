using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Web.Data;
using PortalAcademico.Web.Models.Entities;
using System.Security.Claims;

namespace Portal_academico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cursos/Catalogo
        public async Task<IActionResult> Catalogo(string buscarNombre, int? minCreditos, int? maxCreditos, TimeSpan? horarioBusqueda)
        {
            // Obtener solo cursos activos
            var query = _context.Cursos.Where(c => c.Activo).AsQueryable();

            // Aplicar Filtros
            if (!string.IsNullOrEmpty(buscarNombre))
                query = query.Where(c => c.Nombre.ToLower().Contains(buscarNombre.ToLower()));

            if (minCreditos.HasValue)
                query = query.Where(c => c.Creditos >= minCreditos.Value);

            if (maxCreditos.HasValue)
                query = query.Where(c => c.Creditos <= maxCreditos.Value);

            if (horarioBusqueda.HasValue)
                query = query.Where(c => c.HorarioInicio <= horarioBusqueda.Value && c.HorarioFin >= horarioBusqueda.Value);

            // Mantener el estado de los filtros en la vista
            ViewData["BuscarNombre"] = buscarNombre;
            ViewData["MinCreditos"] = minCreditos;
            ViewData["MaxCreditos"] = maxCreditos;
            ViewData["HorarioBusqueda"] = horarioBusqueda?.ToString(@"hh\:mm");

            var cursos = await query.ToListAsync();
            return View(cursos);
        }

        // GET: /Cursos/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: /Cursos/Inscribirse/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            // Buscamos el curso y sus matrículas actuales para verificar el cupo
            var curso = await _context.Cursos.Include(c => c.Matriculas).FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (curso == null) return NotFound();

            // Obtenemos el ID del usuario actualmente autenticado (Identity)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            if (curso.Matriculas.Count >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya no tiene cupos disponibles.";
                return RedirectToAction(nameof(Detalles), new { id });
            }

            if (curso.Matriculas.Any(m => m.UsuarioId == userId))
            {
                TempData["Error"] = "Ya te encuentras inscrito en este curso.";
                return RedirectToAction(nameof(Detalles), new { id });
            }

            var matricula = new Matricula { CursoId = id, UsuarioId = userId };
            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "¡Inscripción realizada con éxito!";
            return RedirectToAction(nameof(Detalles), new { id });
        }
    }
}