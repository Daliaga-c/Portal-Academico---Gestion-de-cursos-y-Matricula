using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;
using Portal_academico.Models;
using PortalAcademico.Web.Models.Entities;
using PortalAcademico.Web.Data;


namespace Portal_academico.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public HomeController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var cacheKey = "cursos_activos";
        List<Curso> cursosActivos;
        string? cursosCache = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cursosCache))
        {
            cursosActivos = JsonSerializer.Deserialize<List<Curso>>(cursosCache) ?? new List<Curso>();
        }
        else
        {
            cursosActivos = await _context.Cursos.Where(c => c.Activo).ToListAsync();
            
            var cacheOptions = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cursosActivos, jsonOptions), cacheOptions);
        }

        return View(cursosActivos);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
