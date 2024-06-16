using AppBookify.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppBookify.Models;

namespace AppBookify.Controllers
{
    public class ManagedController : Controller
    {
        private ServiceBookify service;


        public ManagedController(ServiceBookify service)
        {
            this.service = service;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            Usuario usuario = await this.service.LoginAsync(model.Email, model.Password);
            if (usuario == null)
            {
                ViewData["LOGINERROR"] = "Usuario o Contraseña incorrectos";
                return View();
            }
            else if (usuario.Activo == false)
            {
                ViewData["ACTIVE"] = "Active su cuenta mediante el correo que le enviamos, gracias";
                return View();
            }
            else
            {
                ClaimsIdentity identity =
                    new ClaimsIdentity
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , ClaimTypes.Name, ClaimTypes.Role);
                //ALMACENAMOS EL NOMBRE DE USUARIO
                identity.AddClaim
                    (new Claim(ClaimTypes.Name, usuario.Email));

                //identity.AddClaim
                //    (new Claim(ClaimTypes.GivenName, usuario.Nombre));

                //ALMACENAMOS EL ID DEL USUARIO
                identity.AddClaim
                (new Claim(ClaimTypes.NameIdentifier, usuario.IdUser.ToString()));
                //ALMACENAMOS EL ROL DEL USUARIO
                identity.AddClaim
                (new Claim(ClaimTypes.Role, usuario.RolId.ToString()));

                identity.AddClaim
                    (new Claim("Nombre", usuario.Nombre));

                identity.AddClaim
                    (new Claim("Foto", usuario?.FotoPerfil));

                identity.AddClaim(new Claim("TOKEN", HttpContext.Session.GetString("TOKEN")));

                ClaimsPrincipal userPrincipal =
                    new ClaimsPrincipal(identity);
                //DAMOS DE ALTA AL USUARIO INDICANDO QUE 
                //ESTARA VALIDADO DURANTE 30 MINUTOS
                await HttpContext.SignInAsync
                    (CookieAuthenticationDefaults.AuthenticationScheme
                    , userPrincipal, new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                    });

                HttpContext.Session.Remove("TOKEN");
                string controller = TempData["controller"].ToString();
                string action = TempData["action"].ToString();

                return RedirectToAction(action, controller);
            }

        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            if (email == null)
            {
                ViewData["ERROR"] = "Por favor introduce un email válido";
                return View();
            }
            await this.service.RecuperarTokenAsync(email);
            return RedirectToAction("RestablecerPassword", "Users");
        }

        public IActionResult ErrorAcceso()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Libros", "Libros");
        }
    }
}
