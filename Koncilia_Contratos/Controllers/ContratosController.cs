using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;
using System.Globalization;
using ClosedXML.Excel;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContratosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contratos
        public async Task<IActionResult> Index(string searchString, string estado, int? anio, int pageNumber = 1, int pageSize = 25)
        {
            // Validar pageSize - solo permitir 25, 50, 100 o 200
            if (pageSize != 25 && pageSize != 50 && pageSize != 100 && pageSize != 200)
            {
                pageSize = 25;
            }
            
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentEstado"] = estado;
            ViewData["CurrentAnio"] = anio;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["PageSize"] = pageSize;

            var contratos = from c in _context.Contratos
                           select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                contratos = contratos.Where(c => c.Cliente.Contains(searchString) 
                    || c.Empresa.Contains(searchString)
                    || c.NumeroContrato.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                contratos = contratos.Where(c => c.Estado == estado);
            }

            if (anio.HasValue)
            {
                contratos = contratos.Where(c => c.Anio == anio.Value);
            }

            // Ordenar
            contratos = contratos.OrderByDescending(c => c.FechaInicio);

            // Calcular total de registros y páginas
            var totalContratos = await contratos.CountAsync();
            ViewData["TotalPages"] = (int)Math.Ceiling(totalContratos / (double)pageSize);
            ViewData["TotalContratos"] = totalContratos;

            // Aplicar paginación
            var contratosPaginados = await contratos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(contratosPaginados);
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // GET: Contratos/Create
        public IActionResult Create()
        {
            var contrato = new Contrato
            {
                Anio = DateTime.Now.Year,
                FechaInicio = DateTime.Today,
                FechaVencimiento = DateTime.Today.AddYears(1),
                Estado = "Activo"
            };
            return View(contrato);
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Anio,Empresa,Cliente,NumeroContrato,ValorPesos,ValorDolares,Descripcion,Categoria,ValorMensual,Observaciones,FechaInicio,FechaVencimiento,ValorFacturado,PorcentajeEjecucion,ValorPendiente,Estado,NumeroHoras,NumeroFactura,NumeroPoliza,FechaVencimientoPoliza")] Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                // Calcular valores automáticamente
                if (contrato.ValorFacturado.HasValue && contrato.ValorPesos > 0)
                {
                    contrato.PorcentajeEjecucion = (contrato.ValorFacturado.Value / contrato.ValorPesos) * 100;
                    contrato.ValorPendiente = contrato.ValorPesos - contrato.ValorFacturado.Value;
                }

                _context.Add(contrato);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contrato creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null)
            {
                return NotFound();
            }
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Anio,Empresa,Cliente,NumeroContrato,ValorPesos,ValorDolares,Descripcion,Categoria,ValorMensual,Observaciones,FechaInicio,FechaVencimiento,ValorFacturado,PorcentajeEjecucion,ValorPendiente,Estado,NumeroHoras,NumeroFactura,NumeroPoliza,FechaVencimientoPoliza")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Calcular valores automáticamente
                    if (contrato.ValorFacturado.HasValue && contrato.ValorPesos > 0)
                    {
                        contrato.PorcentajeEjecucion = (contrato.ValorFacturado.Value / contrato.ValorPesos) * 100;
                        contrato.ValorPendiente = contrato.ValorPesos - contrato.ValorFacturado.Value;
                    }

                    _context.Update(contrato);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Contrato actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoExists(contrato.Id))
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
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contrato eliminado exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Contratos/Importar
        public IActionResult Importar()
        {
            return View();
        }

        // POST: Contratos/Importar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Importar(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Por favor selecciona un archivo Excel.");
                return View();
            }

            try
            {
                using var stream = new MemoryStream();
                await archivo.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Saltar encabezado

                var contratosImportados = 0;

                foreach (var row in rows)
                {
                    var contrato = new Contrato
                    {
                        Anio = row.Cell(1).GetValue<int>(),
                        Empresa = row.Cell(2).GetString(),
                        Cliente = row.Cell(3).GetString(),
                        NumeroContrato = row.Cell(4).GetString(),
                        ValorPesos = row.Cell(5).GetValue<decimal>(),
                        ValorDolares = row.Cell(6).TryGetValue<decimal>(out var vd) ? vd : (decimal?)null,
                        Descripcion = row.Cell(7).GetString(),
                        Categoria = row.Cell(8).GetString(),
                        ValorMensual = row.Cell(9).TryGetValue<decimal>(out var vm) ? vm : (decimal?)null,
                        Observaciones = row.Cell(10).GetString(),
                        FechaInicio = row.Cell(11).GetValue<DateTime>(),
                        FechaVencimiento = row.Cell(12).GetValue<DateTime>(),
                        ValorFacturado = row.Cell(13).TryGetValue<decimal>(out var vf) ? vf : (decimal?)null,
                        Estado = row.Cell(14).GetString(),
                        NumeroHoras = row.Cell(15).TryGetValue<int>(out var nh) ? nh : (int?)null,
                        NumeroFactura = row.Cell(16).GetString(),
                        NumeroPoliza = row.Cell(17).GetString(),
                        FechaVencimientoPoliza = row.Cell(18).TryGetValue<DateTime>(out var fvp) ? fvp : (DateTime?)null
                    };

                    // Calcular valores automáticamente
                    if (contrato.ValorFacturado.HasValue && contrato.ValorPesos > 0)
                    {
                        contrato.PorcentajeEjecucion = (contrato.ValorFacturado.Value / contrato.ValorPesos) * 100;
                        contrato.ValorPendiente = contrato.ValorPesos - contrato.ValorFacturado.Value;
                    }

                    _context.Contratos.Add(contrato);
                    contratosImportados++;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{contratosImportados} contratos importados exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al importar el archivo: {ex.Message}");
                return View();
            }
        }

        // GET: Contratos/Exportar
        public async Task<IActionResult> Exportar()
        {
            var contratos = await _context.Contratos.OrderByDescending(c => c.FechaInicio).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Contratos");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Año";
            worksheet.Cell(1, 2).Value = "Empresa";
            worksheet.Cell(1, 3).Value = "Cliente";
            worksheet.Cell(1, 4).Value = "Número de Contrato";
            worksheet.Cell(1, 5).Value = "Valor en Pesos (sin IVA)";
            worksheet.Cell(1, 6).Value = "Valor en Dólares";
            worksheet.Cell(1, 7).Value = "Descripción";
            worksheet.Cell(1, 8).Value = "Categoría";
            worksheet.Cell(1, 9).Value = "Valor Mensual";
            worksheet.Cell(1, 10).Value = "Observaciones";
            worksheet.Cell(1, 11).Value = "Fecha de Inicio";
            worksheet.Cell(1, 12).Value = "Fecha de Vencimiento";
            worksheet.Cell(1, 13).Value = "Valor Facturado";
            worksheet.Cell(1, 14).Value = "% de Ejecución";
            worksheet.Cell(1, 15).Value = "Valor Pendiente";
            worksheet.Cell(1, 16).Value = "Estado";
            worksheet.Cell(1, 17).Value = "Número de Horas";
            worksheet.Cell(1, 18).Value = "Número de Factura";
            worksheet.Cell(1, 19).Value = "Número de Póliza";
            worksheet.Cell(1, 20).Value = "Fecha de Vencimiento de la Póliza";

            // Datos
            for (int i = 0; i < contratos.Count; i++)
            {
                var contrato = contratos[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = contrato.Anio;
                worksheet.Cell(row, 2).Value = contrato.Empresa;
                worksheet.Cell(row, 3).Value = contrato.Cliente;
                worksheet.Cell(row, 4).Value = contrato.NumeroContrato;
                worksheet.Cell(row, 5).Value = contrato.ValorPesos;
                worksheet.Cell(row, 6).Value = contrato.ValorDolares;
                worksheet.Cell(row, 7).Value = contrato.Descripcion;
                worksheet.Cell(row, 8).Value = contrato.Categoria;
                worksheet.Cell(row, 9).Value = contrato.ValorMensual;
                worksheet.Cell(row, 10).Value = contrato.Observaciones;
                worksheet.Cell(row, 11).Value = contrato.FechaInicio;
                worksheet.Cell(row, 12).Value = contrato.FechaVencimiento;
                worksheet.Cell(row, 13).Value = contrato.ValorFacturado;
                worksheet.Cell(row, 14).Value = contrato.PorcentajeEjecucion;
                worksheet.Cell(row, 15).Value = contrato.ValorPendiente;
                worksheet.Cell(row, 16).Value = contrato.Estado;
                worksheet.Cell(row, 17).Value = contrato.NumeroHoras;
                worksheet.Cell(row, 18).Value = contrato.NumeroFactura;
                worksheet.Cell(row, 19).Value = contrato.NumeroPoliza;
                worksheet.Cell(row, 20).Value = contrato.FechaVencimientoPoliza;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Contratos_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }
    }
}

