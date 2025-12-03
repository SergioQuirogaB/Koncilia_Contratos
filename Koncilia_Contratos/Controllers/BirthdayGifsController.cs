using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class BirthdayGifsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BirthdayGifsController> _logger;

        public BirthdayGifsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BirthdayGifsController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: BirthdayGifs
        public IActionResult Index()
        {
            var gifPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday");
            
            if (!Directory.Exists(gifPath))
            {
                Directory.CreateDirectory(gifPath);
            }

            // Buscar archivos de imagen: .gif, .png, .jpg, .jpeg
            var imageFiles = Directory.GetFiles(gifPath)
                .Where(f => {
                    var ext = Path.GetExtension(f).ToLower();
                    return ext == ".gif" || ext == ".png" || ext == ".jpg" || ext == ".jpeg";
                })
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    FullPath = f,
                    FileSize = new FileInfo(f).Length,
                    LastModified = new FileInfo(f).LastWriteTime
                })
                .OrderBy(f => f.FileName)
                .ToList();

            return View(imageFiles);
        }

        // POST: BirthdayGifs/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona un archivo.";
                return RedirectToAction(nameof(Index));
            }

            // Validar que sea una imagen permitida (.gif, .png, .jpg, .jpeg)
            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg" };
            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Solo se permiten archivos de imagen: GIF, PNG, JPG o JPEG.";
                return RedirectToAction(nameof(Index));
            }

            // Validar tamaño (máximo 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "El archivo es demasiado grande. Máximo 10MB.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var gifPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday");
                
                if (!Directory.Exists(gifPath))
                {
                    Directory.CreateDirectory(gifPath);
                }

                // Generar nombre único si ya existe
                var fileName = file.FileName;
                var filePath = Path.Combine(gifPath, fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var ext = Path.GetExtension(fileName);
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    fileName = $"{nameWithoutExt}_{timestamp}{ext}";
                    filePath = Path.Combine(gifPath, fileName);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"Imagen subida exitosamente: {fileName}");
                TempData["Success"] = $"Imagen '{fileName}' subida exitosamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al subir imagen: {ex.Message}");
                TempData["Error"] = $"Error al subir el archivo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: BirthdayGifs/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["Error"] = "Nombre de archivo no válido.";
                return RedirectToAction(nameof(Index));
            }

            var gifPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "birthday", fileName);
            
            if (!System.IO.File.Exists(gifPath))
            {
                TempData["Error"] = "El archivo no existe.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                System.IO.File.Delete(gifPath);
                _logger.LogInformation($"Imagen eliminada: {fileName}");
                TempData["Success"] = $"Imagen '{fileName}' eliminada exitosamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar imagen: {ex.Message}");
                TempData["Error"] = $"Error al eliminar el archivo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


