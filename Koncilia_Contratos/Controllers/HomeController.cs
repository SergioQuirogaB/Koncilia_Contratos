using System.Diagnostics;
using Koncilia_Contratos.Models;
using Koncilia_Contratos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Koncilia_Contratos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserName = $"{user?.Nombre} {user?.Apellido}";
            ViewBag.UserEmail = user?.Email;

            // Obtener estadísticas reales de la base de datos
            var contratos = await _context.Contratos.ToListAsync();
            var hoy = DateTime.Now;

            // Estadísticas
            ViewBag.ContratosActivos = contratos.Count(c => c.Estado == "Activo");
            ViewBag.ContratosCompletados = contratos.Count(c => c.Estado == "Finalizado");
            ViewBag.ContratosPendientes = contratos.Count(c => c.Estado == "Inactivo" || c.Estado == "Suspendido");
            ViewBag.ContratosVencidos = contratos.Count(c => c.EstaVencido && c.Estado == "Activo");

            // Valores financieros
            ViewBag.ValorTotalActivos = contratos.Where(c => c.Estado == "Activo").Sum(c => c.ValorPesos);
            ViewBag.ValorTotalPendiente = contratos.Where(c => c.Estado == "Activo").Sum(c => c.ValorPendiente ?? 0);
            ViewBag.PromedioEjecucion = contratos.Where(c => c.Estado == "Activo" && c.PorcentajeEjecucion.HasValue).Any()
                ? contratos.Where(c => c.Estado == "Activo" && c.PorcentajeEjecucion.HasValue).Average(c => c.PorcentajeEjecucion!.Value)
                : 0;

            // Contratos recientes (últimos 5)
            ViewBag.ContratosRecientes = await _context.Contratos
                .OrderByDescending(c => c.FechaInicio)
                .Take(5)
                .ToListAsync();

            // Notificaciones reales
            var notificaciones = new List<object>();

            // 1. Contratos vencidos
            var contratosVencidos = contratos
                .Where(c => c.EstaVencido && c.Estado == "Activo")
                .OrderBy(c => c.FechaVencimiento)
                .Take(3)
                .ToList();

            foreach (var contrato in contratosVencidos)
            {
                var diasVencido = (hoy - contrato.FechaVencimiento).Days;
                notificaciones.Add(new
                {
                    Tipo = "error",
                    Icono = "fa-exclamation-triangle",
                    Color = "red",
                    Titulo = $"Contrato #{contrato.Id} vencido",
                    Descripcion = $"{contrato.Cliente} - Vencido hace {diasVencido} días",
                    Url = $"/Contratos/Details/{contrato.Id}",
                    Prioridad = 3
                });
            }

            // 2. Contratos por vencer (próximos 30 días)
            var contratosPorVencer = contratos
                .Where(c => c.DiasRestantes > 0 && c.DiasRestantes <= 30 && c.Estado == "Activo")
                .OrderBy(c => c.DiasRestantes)
                .Take(3)
                .ToList();

            foreach (var contrato in contratosPorVencer)
            {
                notificaciones.Add(new
                {
                    Tipo = "warning",
                    Icono = "fa-clock",
                    Color = "yellow",
                    Titulo = $"Contrato por vencer",
                    Descripcion = $"{contrato.Cliente} - Vence en {contrato.DiasRestantes} días",
                    Url = $"/Contratos/Details/{contrato.Id}",
                    Prioridad = 2
                });
            }

            // 3. Contratos recién creados (últimos 7 días)
            var contratosNuevos = contratos
                .Where(c => (hoy - c.FechaInicio).Days <= 7 && (hoy - c.FechaInicio).Days >= 0)
                .OrderByDescending(c => c.FechaInicio)
                .Take(2)
                .ToList();

            foreach (var contrato in contratosNuevos)
            {
                var diasCreado = (hoy - contrato.FechaInicio).Days;
                var texto = diasCreado == 0 ? "Hoy" : diasCreado == 1 ? "Ayer" : $"Hace {diasCreado} días";
                notificaciones.Add(new
                {
                    Tipo = "success",
                    Icono = "fa-check-circle",
                    Color = "green",
                    Titulo = $"Nuevo contrato creado",
                    Descripcion = $"{contrato.Cliente} - {texto}",
                    Url = $"/Contratos/Details/{contrato.Id}",
                    Prioridad = 1
                });
            }

            // Ordenar por prioridad (errores primero)
            ViewBag.Notificaciones = notificaciones.OrderByDescending(n => ((dynamic)n).Prioridad).Take(5).ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
