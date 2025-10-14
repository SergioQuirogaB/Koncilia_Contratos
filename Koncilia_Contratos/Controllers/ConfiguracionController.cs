using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Koncilia_Contratos.Data;
using Koncilia_Contratos.Models;

namespace Koncilia_Contratos.Controllers
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfiguracionController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Configuracion - Lista de usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Configuracion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Configuracion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Apellido,Email,UserName,PhoneNumber")] ApplicationUser usuario, string Password, string ConfirmarPassword)
        {
            // Validar contraseñas
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");
            }
            else if (Password != ConfirmarPassword)
            {
                ModelState.AddModelError("ConfirmarPassword", "Las contraseñas no coinciden.");
            }
            else if (Password.Length < 6)
            {
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres.");
            }

            // Validar que el email no exista
            if (!string.IsNullOrEmpty(usuario.Email))
            {
                var existeEmail = await _userManager.FindByEmailAsync(usuario.Email);
                if (existeEmail != null)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con este correo electrónico.");
                }
            }

            // Validar que el username no exista
            if (!string.IsNullOrEmpty(usuario.UserName))
            {
                var existeUserName = await _userManager.FindByNameAsync(usuario.UserName);
                if (existeUserName != null)
                {
                    ModelState.AddModelError("UserName", "Ya existe un usuario con este nombre de usuario.");
                }
            }

            if (ModelState.IsValid)
            {
                usuario.EmailConfirmed = true; // Confirmar email automáticamente
                var result = await _userManager.CreateAsync(usuario, Password);
                
                if (result.Succeeded)
                {
                    TempData["Success"] = "Usuario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            
            return View(usuario);
        }

        // GET: Configuracion/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Configuracion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Nombre,Apellido,Email,UserName,PhoneNumber")] ApplicationUser usuario, string? NuevaPassword, string? ConfirmarPassword)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Validar contraseñas si se proporcionaron
            if (!string.IsNullOrEmpty(NuevaPassword) || !string.IsNullOrEmpty(ConfirmarPassword))
            {
                if (string.IsNullOrEmpty(NuevaPassword))
                {
                    ModelState.AddModelError("NuevaPassword", "Debe ingresar la nueva contraseña.");
                }
                else if (NuevaPassword != ConfirmarPassword)
                {
                    ModelState.AddModelError("ConfirmarPassword", "Las contraseñas no coinciden.");
                }
                else if (NuevaPassword.Length < 6)
                {
                    ModelState.AddModelError("NuevaPassword", "La contraseña debe tener al menos 6 caracteres.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = await _userManager.FindByIdAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades
                    usuarioExistente.Nombre = usuario.Nombre;
                    usuarioExistente.Apellido = usuario.Apellido;
                    usuarioExistente.Email = usuario.Email;
                    usuarioExistente.UserName = usuario.UserName;
                    usuarioExistente.PhoneNumber = usuario.PhoneNumber;

                    var result = await _userManager.UpdateAsync(usuarioExistente);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(usuario);
                    }

                    // Cambiar contraseña si se proporcionó
                    if (!string.IsNullOrEmpty(NuevaPassword))
                    {
                        // Remover la contraseña actual
                        var removePasswordResult = await _userManager.RemovePasswordAsync(usuarioExistente);
                        if (!removePasswordResult.Succeeded)
                        {
                            foreach (var error in removePasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(usuario);
                        }

                        // Agregar la nueva contraseña
                        var addPasswordResult = await _userManager.AddPasswordAsync(usuarioExistente, NuevaPassword);
                        if (!addPasswordResult.Succeeded)
                        {
                            foreach (var error in addPasswordResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(usuario);
                        }
                    }

                    TempData["Success"] = !string.IsNullOrEmpty(NuevaPassword) 
                        ? "Usuario y contraseña actualizados exitosamente." 
                        : "Usuario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UsuarioExists(usuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(usuario);
        }

        // GET: Configuracion/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Configuracion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                var result = await _userManager.DeleteAsync(usuario);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Usuario eliminado exitosamente.";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar el usuario.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UsuarioExists(string id)
        {
            return await _userManager.FindByIdAsync(id) != null;
        }
    }
}

