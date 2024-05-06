using AppBookify.Filters;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBookify.Controllers
{
    public class PedidosController : Controller
    {
        private ServiceBookify service;
        private ServiceCacheRedis serviceCache;
        private string UrlBlobLibros;

        public PedidosController(ServiceBookify service, IConfiguration configuration)
        {
            this.service = service;
            UrlBlobLibros = configuration.GetValue<string>("BlobsUrls:UrlBlobLibros");

        }

        [AuthorizeUser]
        public async Task<IActionResult> ComprarProducto(int? idlibro)
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);
            if (user != null)
            {
                if (idlibro != null)
                {
                    Libro producto = await this.service.FindLibroAsync(idlibro.Value);
                    ViewData["LIBRO_ID"] = producto;
                }
                else
                {
                    ViewData["PRODUCTOS"] = "NO HAY LIBROS";
                    return View();
                }
                return View();

            }
            return RedirectToAction("Logout", "Managed");
        }

        [HttpPost]
        [AuthorizeUser]
        public async Task<IActionResult> ComprarProducto(int cantidad, int idlibro)
        {
            try
            {
                int idUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await this.service.RealizarCompraAsync(idUsuario, cantidad, idlibro);

                return RedirectToAction("PedidosUsuario", "Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error al realizar la compra: " + ex.Message);
                return RedirectToAction("Carrito", "Libros");
            }
        }

    }
}
