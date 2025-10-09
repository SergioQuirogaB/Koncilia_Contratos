using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Configuracion
        public async Task<IActionResult> Index()
        {
            var configuraciones = await _context.Configuraciones
                .OrderBy(c => c.Categoria)
                .ThenBy(c => c.Clave)
                .ToListAsync();

            // Agrupar por categoría
            var configuracionesPorCategoria = configuraciones
                .GroupBy(c => c.Categoria ?? "General")
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(configuracionesPorCategoria);
        }

        // GET: Configuracion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Configuracion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Clave,Valor,Descripcion,Categoria")] Configuracion configuracion)
        {
            if (ModelState.IsValid)
            {
                // Verificar que no exista la clave
                var existe = await _context.Configuraciones.AnyAsync(c => c.Clave == configuracion.Clave);
                if (existe)
                {
                    ModelState.AddModelError("Clave", "Ya existe una configuración con esta clave.");
                    return View(configuracion);
                }

                _context.Add(configuracion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(configuracion);
        }

        // GET: Configuracion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuracion = await _context.Configuraciones.FindAsync(id);
            if (configuracion == null)
            {
                return NotFound();
            }
            return View(configuracion);
        }

        // POST: Configuracion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Clave,Valor,Descripcion,Categoria")] Configuracion configuracion)
        {
            if (id != configuracion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar que no exista otra configuración con la misma clave
                    var existe = await _context.Configuraciones
                        .AnyAsync(c => c.Clave == configuracion.Clave && c.Id != configuracion.Id);
                    if (existe)
                    {
                        ModelState.AddModelError("Clave", "Ya existe otra configuración con esta clave.");
                        return View(configuracion);
                    }

                    _context.Update(configuracion);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Configuración actualizada exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfiguracionExists(configuracion.Id))
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
            return View(configuracion);
        }

        // GET: Configuracion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var configuracion = await _context.Configuraciones
                .FirstOrDefaultAsync(m => m.Id == id);
            if (configuracion == null)
            {
                return NotFound();
            }

            return View(configuracion);
        }

        // POST: Configuracion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var configuracion = await _context.Configuraciones.FindAsync(id);
            if (configuracion != null)
            {
                _context.Configuraciones.Remove(configuracion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Configuración eliminada exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConfiguracionExists(int id)
        {
            return _context.Configuraciones.Any(e => e.Id == id);
        }
    }
}

