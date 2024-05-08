using AppBookify.Filters;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBookify.Controllers
{
    public class LibrosController : Controller
    {
        private ServiceBookify service;
        private ServiceCacheRedis cache;
        private string UrlBlobLibros;
        public LibrosController(ServiceBookify service,
            ServiceCacheRedis cache,
             IConfiguration configuration
            )
        {
            this.service = service;
            this.cache = cache;

            UrlBlobLibros = configuration.GetValue<string>("BlobsUrls:UrlBlobLibros");
        }
        public async Task<IActionResult> Libros()
        {
            List<ModelLibro> modelList = await this.service
                .GetAllLibrosAsync();
            foreach (var img in modelList)
            {
                img.Libros.Imagen = UrlBlobLibros + "/" + img.Libros.Imagen;
            }
            return View(modelList);
        }

        public async Task<IActionResult> DetallesLibro(int idLibro)
        {
            LibrosViewModel viewModel =
            await this.service.FindLibroViewAsync(idLibro);

            List<ValoracionesLibro> valoraciones =
                await this.service.GetValoracionesLibroAsync(idLibro);

            if (viewModel != null)
            {
                viewModel.Imagen = UrlBlobLibros + "/" + viewModel.Imagen;
                
                if (valoraciones != null)
                {
                    ViewData["VALORACIONES"] = valoraciones;
                }
                return View(viewModel);
            }
            ViewData["LIBRO_NULL"] = "No existe el libro";
            return RedirectToAction("Libros", "Libros");

        }

        //CACHE
        [AuthorizeUser]
        public async Task<IActionResult> Carrito()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);
            if (user != null)
            {

                List<Libro> libros =
               await this.cache.GetProducotsFavoritosAsync();

                if (libros != null)
                {
                    foreach (var img in libros)
                    {
                        if (img != null)
                        {
                            img.Imagen = UrlBlobLibros + "/" + img.Imagen;
                        }

                        return View(libros);
                    }
                    return View(libros);
                }

                ViewData["CARRITO"] = "No tienes nigún libro en carrito";
                return View();
            }
            return RedirectToAction("Logout", "Managed");
        }

        public async Task<IActionResult> AñadirFavorito(int idlibro)
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);
            if (user != null)
            {
                Libro libro = await this.service.FindLibroAsync(idlibro);
                await this.cache.AddProductoFavoritoAsync(libro);
                ViewData["MENSAJE"] = "Libro almacenado";
                return RedirectToAction("Carrito", "Libros");
            }

            return RedirectToAction("Logout", "Managed");
        }

        public async Task<IActionResult> EliminarProducto(int idlibro)
        {
            await this.cache.DeleteProductosAsync(idlibro);
            return RedirectToAction("Carrito");
        }
        //FIN CACHE

        [AuthorizeUser]
        public async Task<IActionResult> RegistrarLibro()
        {
            List<Genero> generos = await this.service.GetGenerosAsync();
            ViewData["GENEROS"] = generos;
            return View();
        }

        [HttpPost]
        [AuthorizeUser]
        public async Task<IActionResult> RegistrarLibro(Libro libro, string nombreautor,
            string nacionalidadautor, List<int>? idsgeneros, IFormFile imagenlibro)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int idRol = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Role).Value);

                if (idRol == 1)
                {
                    ModelPostLibro model = new ModelPostLibro();
                    model.Libro = libro;

                    if (imagenlibro == null)
                    {
                        List<Genero> generos = await this.service.GetGenerosAsync();
                        ViewData["GENEROS"] = generos;
                        ViewData["IMAGEN"] = "Se requiere una imagen";
                        return View();
                    }

                    if (!string.IsNullOrEmpty(nombreautor) && !string.IsNullOrEmpty(nacionalidadautor))
                    {
                        Autor autor = new Autor
                        {
                            Nombre = nombreautor,
                            Nacionalidad = nacionalidadautor
                        };

                        model.Autor = autor;
                    }

                    if (idsgeneros != null && idsgeneros.Any())
                    {
                        model.GenerosDeLibro = idsgeneros;
                    }

                    //EN CASO DE QUE NO HAYA NI GÉNERO NI AUTOR
                    if (model.Autor == null || model.GenerosDeLibro == null)
                    {
                        await this.service.RegistrarLibroRawAsync(model.Libro, imagenlibro);
                    }
                    else
                    {
                        await this.service.RegistrarLibroAsync(libro, model.Autor, model.GenerosDeLibro, imagenlibro);
                    }

                    return RedirectToAction("Libros");
                }
                return RedirectToAction("Libros");
            }
            return RedirectToAction("Login", "Managed");
        }


        [AuthorizeUser]
        public async Task<IActionResult> EliminarLibro(int idlibro)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int idRol = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Role).Value);

                if (idRol == 1)
                {
                    await this.service.EliminarLibroAsync(idlibro);
                    return RedirectToAction("Libros", "Libros");
                }

            }
            return View();
        }

    }
}
