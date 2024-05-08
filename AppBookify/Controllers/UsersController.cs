﻿using AppBookify.Filters;
using AppBookify.Helpers;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

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

                List<VistaPedidosUsuario> mislibros =
                    await this.service.GetPedidosUsuariosAsync(user.IdUser);
                foreach (var libro in mislibros)
                {
                    libro.Imagen = this.UrlBlobLibros + "/" + libro.Imagen;
                }

                ViewData["MISPEDIDOS"] = mislibros;

                return View(user);
            }
            return RedirectToAction("Logout", "Managed");
        }

        [HttpPost]
        public async Task<IActionResult> Perfil(int? idusuario, string nombre, string apellido,
             string email, IFormFile? imagen)
        {
            RegisertModel model = new RegisertModel();
            model.Email = email;
            model.Nombre = nombre;
            model.Apellido = apellido;
            if (imagen != null)
            {
                model.Imagen = imagen.FileName;
            }

            if (idusuario != null)
            {
                //model.Imagen = "perfil_" + idusuario.Value + ".png";

                await this.service.ActualizarPerfil(model, imagen);

                return RedirectToAction("Perfil");
            }
            return RedirectToAction("Logout", "Managed");
        }


        public async Task<IActionResult> ActivarCuenta(string? tokenmail)
        {
            if (tokenmail != null)
            {
                ViewData["ACTIVAR_CUENTA"] = "Su cuenta ha sido activada, gracias por su verificación";
                Usuario usuario = await this.service.ActivateUserAsync(tokenmail);
                return View(usuario);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
                ViewData["ERROR_MESSAGE"] = "Ha ocurrido un error, compruebe que los campos estén completos: " + ex.Message.ToString();
                return View();
            }

        }

    }
}
