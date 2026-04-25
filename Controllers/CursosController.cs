using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Web.Data;
using PortalAcademico.Web.Models.Entities;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Portal_academico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Cursos/Catalogo
        public async Task<IActionResult> Catalogo(string buscarNombre, int? minCreditos, int? maxCreditos, TimeSpan? horarioBusqueda)
        {
            var cacheKey = "cursos_activos";
            List<Curso> cursosActivos;
            string? cursosCache = await _cache.GetStringAsync(cacheKey);

            // 1. Intentar obtener el listado de Redis
            if (!string.IsNullOrEmpty(cursosCache))
            {
                cursosActivos = JsonSerializer.Deserialize<List<Curso>>(cursosCache) ?? new List<Curso>();
            }
            else
            {
                // 2. Si no existe, consultamos a DB y lo guardamos por 60s
                cursosActivos = await _context.Cursos.Where(c => c.Activo).ToListAsync();
                
                var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };
                var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
                
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursosActivos, jsonOptions), cacheOptions);
            }

            var query = cursosActivos.AsQueryable();

            // Aplicar Filtros
            if (!string.IsNullOrEmpty(buscarNombre))
                query = query.Where(c => c.Nombre.Contains(buscarNombre, StringComparison.OrdinalIgnoreCase));

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

            var cursos = query.ToList();
            return View(cursos);
        }

        // GET: /Cursos/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (curso == null) return NotFound();

            // 3. Guardar el último curso visitado en Sesión
            HttpContext.Session.SetString("UltimoCursoVisitado_Nombre", curso.Nombre);
            HttpContext.Session.SetInt32("UltimoCursoVisitado_Id", curso.Id);

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

            // Determinar la URL de origen para mostrar las alertas en la misma vista
            string? referer = Request.Headers["Referer"].ToString();
            string returnUrl = string.IsNullOrEmpty(referer) ? (Url.Action("Index", "Home") ?? "/") : referer;

            if (curso.Matriculas.Count >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya no tiene cupos disponibles.";
                return Redirect(returnUrl);
            }

            if (curso.Matriculas.Any(m => m.UsuarioId == userId))
            {
                TempData["Error"] = "Ya te encuentras inscrito en este curso.";
                return Redirect(returnUrl);
            }

            // Validar que no haya solapamiento de horarios con otras matrículas
            var matriculasUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId)
                .ToListAsync();

            bool haySolapamiento = matriculasUsuario.Any(m => 
                m.Curso != null &&
                curso.HorarioInicio < m.Curso.HorarioFin && 
                curso.HorarioFin > m.Curso.HorarioInicio);

            if (haySolapamiento)
            {
                TempData["Error"] = "El horario de este curso se solapa con otro curso en el que ya estás matriculado.";
                return Redirect(returnUrl);
            }

            // Crear la matrícula en estado Pendiente
            var matricula = new Matricula { CursoId = id, UsuarioId = userId, Estado = EstadoMatricula.Pendiente };
            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "¡Inscripción solicitada! Tu matrícula está en estado Pendiente.";
            return Redirect(returnUrl);
        }

        // GET: /Cursos/Create
        [Authorize]
        public IActionResult Create() => View();

        // POST: /Cursos/Create (Invalida el caché)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                
                // Invalidar caché (Redis)
                await _cache.RemoveAsync("cursos_activos");
                TempData["Exito"] = "Curso creado correctamente.";
                return RedirectToAction(nameof(Catalogo));
            }
            return View(curso);
        }

        // GET: /Cursos/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        // POST: /Cursos/Edit/5 (Invalida el caché)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(curso);
                await _context.SaveChangesAsync();
                
                // Invalidar caché (Redis)
                await _cache.RemoveAsync("cursos_activos");
                TempData["Exito"] = "Curso actualizado correctamente.";
                return RedirectToAction(nameof(Catalogo));
            }
            return View(curso);
        }
    }
}