using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;
using Koncilia_Contratos.Services;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class CumpleanosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<CumpleanosController> _logger;

        public CumpleanosController(ApplicationDbContext context, IEmailService emailService, ILogger<CumpleanosController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: Cumpleanos
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var empleados = from e in _context.Empleados
                           select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                empleados = empleados.Where(e => e.Nombre.Contains(searchString) 
                    || e.Apellido.Contains(searchString)
                    || e.CorreoElectronico.Contains(searchString));
            }

            // Ordenar por próximo cumpleaños
            var empleadosList = await empleados.ToListAsync();
            empleadosList = empleadosList.OrderBy(e => 
            {
                var proximo = e.ProximoCumpleanos;
                return proximo;
            }).ToList();

            return View(empleadosList);
        }

        // GET: Cumpleanos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Cumpleanos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cumpleanos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,FechaCumpleanos,CorreoElectronico")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Empleado creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Cumpleanos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            return View(empleado);
        }

        // POST: Cumpleanos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,FechaCumpleanos,CorreoElectronico")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Empleado actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        // GET: Cumpleanos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Cumpleanos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Empleado eliminado exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cumpleanos/EnviarCorreo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarCorreo(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }

            try
            {
                await _emailService.SendBirthdayEmailAsync(empleado.CorreoElectronico, empleado.Nombre, empleado.Apellido);
                TempData["Success"] = $"Correo de cumpleaños enviado exitosamente a {empleado.NombreCompleto}.";
                _logger.LogInformation($"Correo de cumpleaños enviado manualmente a {empleado.NombreCompleto} ({empleado.CorreoElectronico})");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al enviar el correo: {ex.Message}";
                _logger.LogError(ex, $"Error al enviar correo de cumpleaños a {empleado.NombreCompleto}");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}

