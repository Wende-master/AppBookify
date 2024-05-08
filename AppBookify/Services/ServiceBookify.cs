using AppBookify.Helpers;
using AppBookify.Models;
using Azure.Storage.Blobs;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Claims;

namespace AppBookify.Services
{
    public class ServiceBookify
    {
        private string ApiUrl;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor httpContextAccessor;
        private ServiceStorageBlobs blobService;
        public ServiceBookify(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ServiceStorageBlobs blobService)
        {
            this.ApiUrl =
                configuration.GetValue<string>("ApiUrls:ApiBookify");
            this.header =
                new MediaTypeWithQualityHeaderValue("application/json");
            this.httpContextAccessor = httpContextAccessor;
            this.blobService = blobService;
        }

        #region TOKEN
        public async Task<string> GetTokenAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                Login model = new Login
                {
                    Email = username,
                    Password = password
                };

                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content =
                    new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {

                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }

            }
        }
        #endregion

        #region CALLS-API
        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                    if (token != null)
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    }
                }

                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        //MÉTODO CON TOKEN en las requests
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add
                    ("Authorization", "Bearer " + token);
                HttpResponseMessage response =
                    await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }

            }
        }
        #endregion

        #region LOGIN
        public async Task<Usuario> LoginAsync(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Auth/Login";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                Login model = new Login
                {
                    Email = email,
                    Password = password
                };
                string jsonData = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent
                    (jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    string token = keys.GetValue("response").ToString();
                    HttpContext httpContext = this.httpContextAccessor.HttpContext;
                    httpContext.Session.SetString("TOKEN", token);

                    return await this.CallApiAsync<Usuario>("api/Users/Perfil", token);
                }
                else return null;
            }
        }
        #endregion

        #region USUARIOS
        public async Task<Usuario> FindUsuarioAsync(int idusuario)
        {
            string request = "api/Users/" + idusuario;
            string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;

            return await this.CallApiAsync<Usuario>(request, token);
        }

        public async Task<Usuario> RegistrarUsuarioAsync(string nombre, string apellido, string email, string password, string? imagen)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Users/RegistrarUsuario/" + nombre + "/" + apellido +
               "/" + email + "/" + password + "/" + imagen;

                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                Usuario usuario = new Usuario();
                usuario.Nombre = nombre;
                usuario.Apellido = apellido;
                usuario.Email = email;
                usuario.FotoPerfil = imagen;
                usuario.Activo = false;
                usuario.Salt = HelperTools.GenerateSalt();
                usuario.Password = HelperCryptography.EncryptPassword(password, usuario.Salt);

                string json = JsonConvert.SerializeObject(usuario);

                StringContent content = new StringContent
                    (json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                string jsonUsuario = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<Usuario>(jsonUsuario);
            }

        }

        public async Task ActualizarPerfil(RegisertModel model, IFormFile? imagen)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Users/ActualizarPerfil/" + model.Nombre +
                    "/" + model.Apellido + "/" + model.Email;

                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                int id = int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier.ToString()));
                int idRol = int.Parse(httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role.ToString()));

                Usuario usuario = new Usuario()
                {
                    //IdUser = id,
                    RolId = idRol,
                    Password = new byte[] { },
                    Email = model.Email,
                    Apellido = model.Apellido,
                    Salt = "",
                    Nombre = model.Nombre,
                    Activo = new bool(),
                    FotoPerfil = model.Imagen,
                    TokenMail = ""
                };

                usuario = await FindUsuarioAsync(id);

                string json = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent
                    (json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(request, content);

                if (imagen != null)
                {

                    usuario.FotoPerfil = "perfil_" + usuario.IdUser + ".png";
                    using (Stream stream = imagen.OpenReadStream())
                    {
                        await this.blobService.UploadBlobAsync("imagesperfiles", usuario.FotoPerfil, stream);
                    }

                }
            }

        }


        public async Task RegistrarUsuarioAsync(RegisertModel model, IFormFile imagen)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "api/Users";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Convertir el objeto Usuario a JSON
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    Usuario newUser = await response.Content.ReadAsAsync<Usuario>();

                    if (imagen != null)
                    {
                        newUser.FotoPerfil = "perfil_" + newUser.IdUser + ".png";

                        using (Stream stream = imagen.OpenReadStream())
                        {
                            await this.blobService.UploadBlobAsync("imagesperfiles", newUser.FotoPerfil, stream);
                        }
                    }
                    else
                    {
                        throw new Exception("Se requiere una imagen");
                    }
                }
                else
                {
                    throw new Exception($"Error al registrar el usuario. Código de estado: {response.StatusCode}");
                }
            }
        }

        public async Task<Usuario> ActivateUserAsync(string? tokenmail)
        {
            string request = "api/Users/ActivarCuenta?tokenmail=" + tokenmail;
            Usuario usuario = await this.CallApiAsync<Usuario>(request);
            return usuario;
        }

        #endregion

        #region LIBROS
        public async Task<List<ModelLibro>> GetAllLibrosAsync()
        {
            string request = "api/Libros";
            List<ModelLibro> modelLibros =
                await this.CallApiAsync<List<ModelLibro>>(request);

            return modelLibros;
        }

        public async Task<List<ModelLibro>> FiltraLibrosAsync
            (int? numeropagina, int? tamanyopagina,
            List<int>? genero = null, List<int>? estado = null)
        {
            //string request = "api/Libros/FiltrarlibrosModel?numeropagina=" + numeropagina + "&tamanyopagina=" + tamanyopagina +
            //    "&genero=" + genero + "&estado=" + estado;

            string request = $"api/Libros/FiltrarlibrosModel?numeropagina={numeropagina}&tamanyopagina={tamanyopagina}";

            if (genero != null && genero.Any())
            {
                string generoString = string.Join(",", genero);
                request += $"&genero={generoString}";
            }

            if (estado != null && estado.Any())
            {
                string estadoString = string.Join(",", estado);
                request += $"&estado={estadoString}";
            }


            List<ModelLibro> librosFiltrados =
                await this.CallApiAsync<List<ModelLibro>>(request);
            return librosFiltrados;
        }

        public async Task<Libro> FindLibroAsync(int idlibro)
        {
            string request = "api/Libros/" + idlibro;
            Libro libro = await
                this.CallApiAsync<Libro>(request);
            return libro;
        }

        public async Task<LibrosViewModel> FindLibroViewAsync(int idlibro)
        {
            string request = "api/Libros/FindLibroView/" + idlibro;
            LibrosViewModel viewModel =
                await this.CallApiAsync<LibrosViewModel>(request);
            return viewModel;
        }

        public async Task RegistrarLibroAsync(ModelPostLibro model, IFormFile imagen)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "api/Libros/RegistrarLibro";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    model.Libro = new Libro();
                    if (imagen != null)
                    {
                        model.Libro.Imagen = "libro_" + model.Libro.IdLibro + ".png";

                        using (Stream stream = imagen.OpenReadStream())
                        {
                            await this.blobService.UploadBlobAsync("imageslibros", model.Libro.Imagen, stream);
                        }
                    }
                    else
                    {
                        throw new Exception("Se requiere una imagen");
                    }

                }

            }
        }

        public async Task RegistrarLibroRawAsync(Libro libro, IFormFile imagen)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "api/Libros";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(libro);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    Libro newLibro = await response.Content.ReadAsAsync<Libro>();
                    if (imagen != null)
                    {
                        newLibro.Imagen = "libro_" + newLibro.IdLibro + ".png";

                        using (Stream stream = imagen.OpenReadStream())
                        {
                            await this.blobService.UploadBlobAsync("imageslibros", newLibro.Imagen, stream);
                        }
                    }
                    else
                    {
                        throw new Exception("Se requiere una imagen");
                    }
                }

            }

        }

        public async Task RegistrarLibroAsync(Libro libro, Autor autor, List<int> generosLibro, IFormFile imagen)
        {

            using (HttpClient client = new HttpClient())
            {
                string request = "api/Libros/RegistrarLibro";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                ModelPostLibro model = new ModelPostLibro
                {
                    Libro = libro,
                    Autor = autor,
                    GenerosDeLibro = generosLibro
                };

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode)
                {
                    ModelPostLibro responseModel = await response.Content.ReadAsAsync<ModelPostLibro>();
                    Libro responseLibro = responseModel.Libro;

                    if (responseLibro != null)
                    {
                        using (Stream stream = imagen.OpenReadStream())
                        {
                            responseLibro.Imagen = "libro_" + responseLibro.IdLibro + ".png";
                            await this.blobService.UploadBlobAsync("imageslibros", responseLibro.Imagen, stream);
                        }
                    }

                    responseLibro = await response.Content.ReadAsAsync<Libro>();
                }
                else
                {
                    throw new HttpRequestException("Error al registrar libro " + response.StatusCode);
                }

            }
        }

        public async Task ActualizarLibroAsync(Libro libro, IFormFile imagen)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/libros/ActualizarLibro";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                if (imagen != null)
                {
                    libro.Imagen = "libro_" + libro.IdLibro + ".png";
                    using (Stream stream = imagen.OpenReadStream())
                    {
                        await this.blobService.UploadBlobAsync("imageslibros", libro.Imagen, stream);
                    }
                }
                string json = JsonConvert.SerializeObject(libro);
                StringContent content = new StringContent
                    (json, Encoding.UTF8, "application/json");
                HttpResponseMessage response =
                    await client.PutAsync(request, content);
            }
        }
        public async Task EliminarLibroAsync(int idlibro)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Libros/EliminarLibro/" + idlibro;
                client.BaseAddress = new Uri(ApiUrl);
                client.DefaultRequestHeaders.Clear();
                string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                if (token != null)
                {
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                    Libro libro = await this.FindLibroAsync(idlibro);

                    HttpResponseMessage response = await client.DeleteAsync(request);

                    if (libro != null)
                    {
                        if (libro.Imagen != null)
                        {
                            BlobClient blob = await this.blobService.FindBlobAsync("imageslibros", libro.Imagen);

                            if (blob != null)
                            {
                                await this.blobService.DeleteBlobAsync("imageslibros", libro.Imagen);
                            }
                        }
                    }
                }

            }

        }

        #endregion

        #region GENEROS
        public async Task<List<Genero>> GetGenerosAsync()
        {
            string request = "api/Generos";
            List<Genero> generos = await
                    this.CallApiAsync<List<Genero>>(request);

            return generos;

            //using (HttpClient client = new HttpClient())
            //{
            //    string request = "api/Generos";
            //    client.BaseAddress = new Uri(this.ApiUrl);
            //    client.DefaultRequestHeaders.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(this.header);
            //    string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
            //    if (token != null)
            //    {
            //        client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

            //        List<Genero> generos = await
            //        this.CallApiAsync<List<Genero>>(request, token);

            //        return generos;
            //    }
            //    return null;
            //}


        }


        #endregion

        #region PEDIDOS
        public async Task<List<VistaPedidosUsuario>> GetPedidosUsuariosAsync(int idusuario)
        {
            string request = "api/PedidosUsuario/VistaPedidosUser/" + idusuario;
            string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;

            List<VistaPedidosUsuario> pedidosUsuario =
                await this.CallApiAsync<List<VistaPedidosUsuario>>(request, token);
            return pedidosUsuario;
        }

        public async Task<List<Libro>> FindListaLibrosAsync(List<int> idslibro)
        {
            string request = "api/Libros/FindListaLibros?";

            foreach (int id in idslibro)
                request += "idlibro=" + id + "&";
            request = request.TrimEnd('&');

            if (idslibro.Count() == 0) request = request.TrimEnd('?');
            return await this.CallApiAsync<List<Libro>>(request);

        }


        public async Task<PedidoConDetallesViewModel>
            RealizarCompraAsync(int idUsuario, int cantidad, int idLibro)
        {
            try
            {
                PedidoConDetallesViewModel model = new PedidoConDetallesViewModel
                {
                    Pedido = new Pedido
                    {
                        IdPedido = 0,
                        Albaran = 0,
                        IdUsuario = idUsuario,
                        FechaPedido = DateTime.UtcNow,
                        IdEstadoPedido = 0,
                        PrecioTotal = 0
                    },
                    Detalle = new DetallesPedido
                    {
                        IdDetallePedido = 0,
                        IdPedido = 0,
                        IdLibro = idLibro,
                        Cantidad = cantidad
                    }
                };


                using (HttpClient client = new HttpClient())
                {
                    string request = "api/Pedidos/RealizarPedidoConDetalles";
                    client.BaseAddress = new Uri(this.ApiUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(this.header);
                    string token = httpContextAccessor.HttpContext.User.FindFirst(c => c.Type == "TOKEN").Value;
                    if (token != null)
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                    string json = JsonConvert.SerializeObject(model);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(request, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsAsync<PedidoConDetallesViewModel>();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al realizar la compra", ex);
            }
        }

        #endregion

        #region VALORACIONES
        public async Task<List<ValoracionesLibro>> GetValoracionesLibroAsync(int? idlibro)
        {
            string request =
                "api/Valoraciones/GetValoracionesLibro/" + idlibro;
            List<ValoracionesLibro> valoraciones =
                await this.CallApiAsync<List<ValoracionesLibro>>(request);
            return valoraciones;
        }

        public async Task<ValoracionesLibro> RegistrarValoracion(ValoracionesLibro valoracionesLibro)
        {


            using (HttpClient client = new HttpClient())
            {
                string request = "api/Valoraciones";
                client.BaseAddress = new Uri(this.ApiUrl);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = JsonConvert.SerializeObject(valoracionesLibro);
                StringContent content = new StringContent
                    (json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                string jsonValoracion = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ValoracionesLibro>(jsonValoracion);

                //string json = JsonConvert.SerializeObject(valoracionesLibro);
                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                //HttpResponseMessage response = await client.PostAsync(request, content);

                //if (response.IsSuccessStatusCode)
                //{

                //}

            }

        }

        #endregion

    }
}
