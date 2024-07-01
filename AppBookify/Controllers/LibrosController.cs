using AppBookify.Extensions;
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
        //private ServiceCacheRedis cache;
        private string UrlBlobLibros;
        private string UrlBlobPerfiles;
        public LibrosController(ServiceBookify service,
             //ServiceCacheRedis cache,
             IConfiguration configuration
            )
        {
            this.service = service;
            //this.cache = cache;
            UrlBlobPerfiles = configuration.GetValue<string>("BlobsUrls:UrlBlobPerfiles");
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

            List<Usuario> usuarios =
                await this.service.FindUsersValoracionAsync(idLibro);

            if (viewModel != null)
            {

                foreach (var user in usuarios)
                {
                    user.FotoPerfil = UrlBlobPerfiles + "/" + user.FotoPerfil;
                }

                viewModel.Imagen = UrlBlobLibros + "/" + viewModel.Imagen;

                if (valoraciones != null)
                {
                    ViewData["USUARIOS_VALORACION"] = usuarios;
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

                List<int> carrito = HttpContext.Session.GetObject<List<int>>("FAVORITOS", user.IdUser); /* HttpContext.Session.GetObject<List<int>>("FAVORITOS");*/;
                if (carrito != null)
                {
                    List<Libro> libros = await this.service.FindListaLibrosAsync(carrito);

                    if (libros != null)
                    {
                        foreach (var img in libros)
                        {

                            img.Imagen = UrlBlobLibros + "/" + img.Imagen;
                        }
                        return View(libros);
                    }

                }
                ViewData["CARRITO"] = "No tienes nigún libro en carrito";
                return View();

                // List<Libro> libros =
                //await this.cache.GetProductosFavoritosAsync();

                // if (libros != null)
                // {
                //     foreach (var img in libros)
                //     {
                //         if (img != null)
                //         {
                //             img.Imagen = UrlBlobLibros + "/" + img.Imagen;
                //         }

                //         return View(libros);
                //     }
                //     return View(libros);
                // }

                // ViewData["CARRITO"] = "No tienes nigún libro en carrito";
                // return View();
            }
            return RedirectToAction("Logout", "Managed");
        }

        [AuthorizeUser]
        [HttpPost]
        public async Task<IActionResult> Carrito(List<int>? cantidad, List<int>? idlibro)
        {

            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Logout", "Managed");
                }

                int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                Usuario user = await this.service.FindUsuarioAsync(idusuario);

                List<int> carrito = HttpContext.Session.GetObject<List<int>>("FAVORITOS", user.IdUser); /* HttpContext.Session.GetObject<List<int>>("FAVORITOS");*/
                if (carrito == null || carrito.Count == 0)
                {
                    ViewData["CARRITO"] = "No tienes ningún libro en el carrito";
                    return RedirectToAction("Carrito");
                }
                if (cantidad == null || idlibro == null)
                {
                    ViewData["CARRITO"] = "No tienes ningún libro en el carrito";
                    return View();
                }
                await service.RealizarPedidoAsync(cantidad, idlibro);

                HttpContext.Session.Remove($"FAVORITOS_{user.IdUser}");
                //HttpContext.Session.Remove("FAVORITOS");

                // Mostrar SweetAlert de éxito
                TempData["MENSAJE_EXITO"] = "¡Compra realizada con éxito!";


                return RedirectToAction("PedidosUsuario", "Users");
            }
            catch (Exception ex)
            {
                ViewData["MENSAJE"] = ex.Message;
                return RedirectToAction("Carrito");
            }
        }

        public async Task<IActionResult> AñadirFavorito(int? idlibro)
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);
            if (user != null)
            {
                if (idlibro != null)
                {
                    List<int> carrito;
                    //if (HttpContext.Session.GetString("FAVORITOS") == null)
                    if (HttpContext.Session.GetString($"FAVORITOS_{idusuario}") == null)
                    {
                        carrito = new List<int>();
                    }
                    else
                    {
                        //carrito = HttpContext.Session.GetObject<List<int>>("FAVORITOS");
                        carrito = HttpContext.Session.GetObject<List<int>>("FAVORITOS", user.IdUser);
                    }
                    carrito.Add(idlibro.Value);
                    //HttpContext.Session.SetObject("FAVORITOS", carrito);
                    HttpContext.Session.SetObject("FAVORITOS", user.IdUser, carrito);

                    Libro libro = await this.service.FindLibroAsync(idlibro.Value);
                    TempData["MENSAJE_EXITO"] = libro.Titulo + " se añadió a tu carrito.";

                    return RedirectToAction("Carrito", "Libros");
                }

                //Libro libro = await this.service.FindLibroAsync(idlibro);
                //await this.cache.AddProductoFavoritoAsync(libro);
                //ViewData["MENSAJE"] = "Libro almacenado";
                //return RedirectToAction("Carrito", "Libros");
            }

            return RedirectToAction("Logout", "Managed");
        }

        public async Task<IActionResult> EliminarProducto(int? idlibro)
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario user = await this.service.FindUsuarioAsync(idusuario);

            if (idlibro != null)
            {
                List<int> carrito = HttpContext.Session.GetObject<List<int>>("FAVORITOS", user.IdUser);
                    /*HttpContext.Session.GetObject<List<int>>("FAVORITOS")*/
                carrito.Remove(idlibro.Value);
                if (carrito.Count() == 0)
                {
                    //HttpContext.Session.Remove("FAVORITOS");
                    HttpContext.Session.Remove($"FAVORITOS_{user.IdUser}");
                }
                else
                {
                    HttpContext.Session.SetObject("FAVORITOS", user.IdUser, carrito);
                    //HttpContext.Session.SetObject("FAVORITOS", carrito);
                }

                Libro libro = await this.service.FindLibroAsync(idlibro.Value);
                TempData["MENSAJE_EXITO"] = libro.Titulo + " ya no está en carrito.";
            }
            //await this.cache.DeleteProductosAsync(idlibro);
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
                    TempData["MENSAJE_EXITO"] = "Se ha registrado el nuevo libro con éxito";
                    return RedirectToAction("Libros");
                }
                return RedirectToAction("Libros");
            }
            return RedirectToAction("Login", "Managed");
        }


        [AuthorizeUser]
        [HttpPost]
        public async Task<IActionResult> EliminarLibro(int idlibro)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                int idRol = int.Parse(HttpContext.User.FindFirst(ClaimTypes.Role).Value);

                if (idRol == 1)
                {
                    await this.service.EliminarLibroAsync(idlibro);
                    TempData["MENSAJE_EXITO"] = "El libro se ha eliminado con éxito";
                    return RedirectToAction("Libros", "Libros");
                }

            }
            return View();
        }

    }
}
