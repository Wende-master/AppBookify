using AppBookify.Filters;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBookify.Controllers
{
    public class ValoracionesController : Controller
    {
        private ServiceBookify service;
        private string UrlBlobLibros;
        public ValoracionesController(ServiceBookify service, IConfiguration configuration)
        {
            this.service = service;
            UrlBlobLibros = configuration.GetValue<string>("BlobsUrls:UrlBlobLibros");

        }


        [AuthorizeUser]
        public async Task<IActionResult> RegistrarValoracion(int? idlibro)
        {

            if (idlibro != null)
            {
                Libro libro = await this.service.FindLibroAsync(idlibro.Value);
                libro.Imagen = this.UrlBlobLibros + "/" + libro.Imagen;

                ViewData["LIBRO"] = libro;
            }
            else
            {
                return RedirectToAction("Login", "Managed");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarValoracion(ValoracionesLibro valoraciones)
        {
            await this.service.RegistrarValoracion(valoraciones);
            return RedirectToAction("Index", "Home");

        }

        public async Task<IActionResult> EliminarValoracion(int? idlibro, int idvaloracion)
        {
            if (idlibro != null)
            {
                Libro libro = await this.service.FindLibroAsync(idlibro.Value);
                await this.service.EliminarValoracionAsync(idvaloracion);
                return RedirectToAction("DetallesLibro", "Libros", new { idlibro = libro.IdLibro });
            }
            return RedirectToAction("Libros", "Libros");
        }
    }
}
