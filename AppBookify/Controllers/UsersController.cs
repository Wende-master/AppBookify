using AppBookify.Filters;
using AppBookify.Helpers;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBookify.Controllers
{
    public class UsersController : Controller
    {
        private HelperPathProvider helperPathProvider;
        private ServiceBookify service;
        private ServiceCacheRedis serviceCache;
        private string UrlBlobPerfiles;
        private string UrlBlobLibros;

        public UsersController(
            ServiceBookify service,
            HelperPathProvider helperPathProvider,
            ServiceCacheRedis serviceCache,
            IConfiguration configuration)
        {
            this.service = service;
            this.helperPathProvider = helperPathProvider;
            this.serviceCache = serviceCache;
            UrlBlobPerfiles = configuration.GetValue<string>("BlobsUrls:UrlBlobPerfiles");
            UrlBlobLibros = configuration.GetValue<string>("BlobsUrls:UrlBlobLibros");
        }

        [AuthorizeUser]
        public async Task<IActionResult> Perfil()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);

            if (user != null)
            {
                user.FotoPerfil = UrlBlobPerfiles + "/" + user.FotoPerfil;

                return View(user);
            }
            return RedirectToAction("Logout", "Managed");
        }

        //[AuthorizeUser]
        public async Task<IActionResult> ActivarCuenta(string? tokenmail)
        {
            //int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            //Usuario user = await this.service.FindUsuarioAsync(idusuario);
            //user != null &&
            if (tokenmail != null)
            {
                //user.TokenMail = tokenmail;
                ViewData["ACTIVAR_CUENTA"] = "Su cuenta ha sido activada";
                Usuario usuario = await this.service.ActivateUserAsync(tokenmail);
                return View(usuario);
                //return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Logout", "Managed");

        }


        public async Task<IActionResult> PedidosUsuario()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario usuario = await this.service.FindUsuarioAsync(idusuario);
            if (usuario != null)
            {
                List<VistaPedidosUsuario> vistaPedido =
                    await this.service.GetPedidosUsuariosAsync(usuario.IdUser);

                if (vistaPedido.Count > 0)
                {
                    foreach (var vista in vistaPedido)
                    {
                        vista.Imagen = UrlBlobLibros + "/" + vista.Imagen;
                    }

                    return View(vistaPedido);
                }

                ViewData["MISPEDIDOS"] = "No tienes ninguna compra realizada";
                return View();
            }
            return RedirectToAction("Logout", "Managed");

        }

        public IActionResult RegistroUsuario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroUsuario(
            string nombre, string apellido, string email, string password, IFormFile? imagen
            )
        {
            try
            {
                RegisertModel model = new RegisertModel();
                model.Email = email;
                model.Nombre = nombre;
                model.Apellido = apellido;
                model.Contrasenya = password;

                model.Imagen = imagen.FileName;

                await this.service.RegistrarUsuarioAsync(model, imagen);
                return RedirectToAction("Login", "Managed");
            }
            catch (Exception ex)
            {
                ViewData["ERROR_MESSAGE"] = "Ha ocurrido un error, compruebe que los campos estén completos" + ex.Message;
                return View();
            }

        }


        //[AuthorizeUser]
        //public async Task<IActionResult> ActualizarPerfil()
        //{
        //    var usuarioImagen = HttpContext.User.FindFirst("FotoPerfil")?.Value;
        //    var idusuario = int.Parse(HttpContext.User.FindFirst("idUsuario")?.Value);
        //    Usuario usuario = await this.repo.GetPerfil(idusuario);

        //    usuarioImagen = this.helperPathProvider.MapPathURLProvider(usuarioImagen, Folders.Perfiles);
        //    usuario.FotoPerfil = usuarioImagen;
        //    return View(usuario);
        //}

        //[HttpPost]
        //public async Task<IActionResult> ActualizarPerfil(int id, string nombre, string apellido, string email, IFormFile? imagen)
        //{
        //    try
        //    {
        //        if (imagen != null)
        //        {
        //            await this.helperUploadFiles.UploadFilesAsync(imagen, Folders.Perfiles);
        //        }

        //        if (imagen == null)
        //            await this.repo.ActualizarUsuarioAsync(id, nombre, apellido, email, null);
        //        else
        //            await this.repo.ActualizarUsuarioAsync(id, nombre, apellido, email, imagen.FileName);

        //        return RedirectToAction("Perfil", "Users");

        //    }
        //    catch (Exception ex)
        //    {
        //        ViewData["ERROR_MESSAGE"] = ex.Message;
        //        return View();
        //    }

        //}


        //public async Task<IActionResult> ActivateUser(string? tokenmail)
        //{
        //    ViewData["ACTIVADA"] = "Su cuenta ya está activa";
        //    await this.service.ActivateUserAsync(tokenmail);
        //    return View();
        //}

    }
}
