using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reportes
        public async Task<IActionResult> Index()
        {
            var contratos = await _context.Contratos.ToListAsync();

            // Estadísticas generales
            ViewBag.TotalContratos = contratos.Count;
            ViewBag.ContratosActivos = contratos.Count(c => c.Estado == "Activo");
            ViewBag.ContratosPorVencer = contratos.Count(c => c.DiasRestantes > 0 && c.DiasRestantes <= 30 && c.Estado == "Activo");
            ViewBag.ContratosVencidos = contratos.Count(c => c.EstaVencido && c.Estado == "Activo");
            
            // Valores totales
            ViewBag.ValorTotalPesos = contratos.Sum(c => c.ValorPesos);
            ViewBag.ValorTotalFacturado = contratos.Sum(c => c.ValorFacturado ?? 0);
            ViewBag.ValorTotalPendiente = contratos.Sum(c => c.ValorPendiente ?? 0);

            // Porcentaje de ejecución promedio
            var contratosConEjecucion = contratos.Where(c => c.PorcentajeEjecucion.HasValue).ToList();
            ViewBag.PromedioEjecucion = contratosConEjecucion.Any() 
                ? contratosConEjecucion.Average(c => c.PorcentajeEjecucion!.Value) 
                : 0;

            // Contratos por estado (para gráfica)
            ViewBag.ContratosPorEstado = contratos
                .GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToList();

            // Contratos por categoría (para gráfica)
            ViewBag.ContratosPorCategoria = contratos
                .Where(c => !string.IsNullOrEmpty(c.Categoria))
                .GroupBy(c => c.Categoria)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count(), Valor = g.Sum(c => c.ValorPesos) })
                .OrderByDescending(x => x.Valor)
                .ToList();

            // Contratos por año (para gráfica)
            ViewBag.ContratosPorAnio = contratos
                .GroupBy(c => c.Anio)
                .Select(g => new { Anio = g.Key, Cantidad = g.Count(), Valor = g.Sum(c => c.ValorPesos) })
                .OrderBy(x => x.Anio)
                .ToList();

            // Contratos por empresa (para gráfica)
            ViewBag.ContratosPorEmpresa = contratos
                .GroupBy(c => c.Empresa)
                .Select(g => new { Empresa = g.Key, Cantidad = g.Count(), Valor = g.Sum(c => c.ValorPesos) })
                .OrderByDescending(x => x.Valor)
                .Take(10)
                .ToList();

            // Top 10 clientes por valor
            ViewBag.TopClientes = contratos
                .GroupBy(c => c.Cliente)
                .Select(g => new { Cliente = g.Key, Cantidad = g.Count(), Valor = g.Sum(c => c.ValorPesos) })
                .OrderByDescending(x => x.Valor)
                .Take(10)
                .ToList();

            return View();
        }

        // GET: Reportes/Clientes
        public async Task<IActionResult> Clientes()
        {
            var contratos = await _context.Contratos.ToListAsync();

            var reporteClientes = contratos
                .GroupBy(c => c.Cliente)
                .Select(g => new
                {
                    Cliente = g.Key,
                    TotalContratos = g.Count(),
                    ContratosActivos = g.Count(c => c.Estado == "Activo"),
                    ValorTotal = g.Sum(c => c.ValorPesos),
                    ValorFacturado = g.Sum(c => c.ValorFacturado ?? 0),
                    ValorPendiente = g.Sum(c => c.ValorPendiente ?? 0),
                    PromedioEjecucion = g.Where(c => c.PorcentajeEjecucion.HasValue).Any() 
                        ? g.Where(c => c.PorcentajeEjecucion.HasValue).Average(c => c.PorcentajeEjecucion!.Value) 
                        : 0
                })
                .OrderByDescending(x => x.ValorTotal)
                .ToList();

            ViewBag.TotalClientes = reporteClientes.Count;
            ViewBag.ClientesActivos = reporteClientes.Count(c => c.ContratosActivos > 0);

            return View(reporteClientes);
        }

        // GET: Reportes/Ejecutivo
        public async Task<IActionResult> Ejecutivo()
        {
            var contratos = await _context.Contratos.ToListAsync();
            var anioActual = DateTime.Now.Year;

            // Resumen ejecutivo
            ViewBag.TotalContratos = contratos.Count;
            ViewBag.ContratosAnioActual = contratos.Count(c => c.Anio == anioActual);
            ViewBag.ContratosActivos = contratos.Count(c => c.Estado == "Activo");
            
            ViewBag.ValorTotalCartera = contratos.Sum(c => c.ValorPesos);
            ViewBag.ValorCarteraActiva = contratos.Where(c => c.Estado == "Activo").Sum(c => c.ValorPesos);
            ViewBag.ValorTotalFacturado = contratos.Sum(c => c.ValorFacturado ?? 0);
            ViewBag.ValorTotalPendiente = contratos.Sum(c => c.ValorPendiente ?? 0);

            // Porcentaje de ejecución general
            var contratosConEjecucion = contratos.Where(c => c.PorcentajeEjecucion.HasValue).ToList();
            ViewBag.PromedioEjecucionGeneral = contratosConEjecucion.Any() 
                ? contratosConEjecucion.Average(c => c.PorcentajeEjecucion!.Value) 
                : 0;

            // Distribución por estado
            ViewBag.DistribucionEstado = contratos
                .GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count(), Valor = g.Sum(c => c.ValorPesos) })
                .ToList();

            // Evolución anual
            ViewBag.EvolucionAnual = contratos
                .GroupBy(c => c.Anio)
                .Select(g => new { 
                    Anio = g.Key, 
                    Cantidad = g.Count(), 
                    Valor = g.Sum(c => c.ValorPesos),
                    Facturado = g.Sum(c => c.ValorFacturado ?? 0)
                })
                .OrderBy(x => x.Anio)
                .ToList();

            // Alertas
            ViewBag.ContratosPorVencer = contratos.Count(c => c.DiasRestantes > 0 && c.DiasRestantes <= 30 && c.Estado == "Activo");
            ViewBag.ContratosVencidos = contratos.Count(c => c.EstaVencido && c.Estado == "Activo");

            return View();
        }
    }
}

