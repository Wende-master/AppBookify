using AppBookify.Helpers;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Security.Claims;
using AppBookify.Models;

namespace AppBookify.Services
{
    public class ServiceCacheRedis
    {
        private IDatabase database;
        private IHttpContextAccessor httpContextAccessor;
        public ServiceCacheRedis(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            //this.database =
            //    HelperCacheMultiplexer.Connection.GetDatabase();

            ////SEGUNDO
            //var cacheConnectionString = configuration["CacheRedis:Cache"];
            //var connectionMultiplexer = ConnectionMultiplexer.Connect(cacheConnectionString);
            //this.database = connectionMultiplexer.GetDatabase();

            var connectionMultiplexer = HelperCacheMultiplexer.GetConnection(configuration);
            this.database = connectionMultiplexer.GetDatabase();

            this.httpContextAccessor = httpContextAccessor;
        }

        //MÉTODO ALMACENAR PRODUCTOS
        public async Task AddProductoFavoritoAsync(Libro producto)
        {
            //FUNCIONA CON KEY/VALUE
            //DICHAS CLAVES SON COMUNES PARA TODOS LOS USUARIOS
            //DEBERÍAMOS TENER LA CLAVE UNNICA PARA CADA USER (su id)
            //ALMACENAR EL PRODUCTO EN FORMATO JSON, HABRÁ UNA COLECCIÓN DE LOS PRODUCTOS

            if (this.httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier));

                string jsonProductos =
               await this.database.StringGetAsync("favoritos" + "-" + idusuario);
                List<Libro> productosList;
                if (jsonProductos == null)
                {
                    //NO TENEMOS FAVORITOS TODAVÍA, CREAR LA COLECCIÓN
                    productosList = new List<Libro>();

                }
                else
                {
                    //HAY PRODUCTOS EN CACHE Y LOS RECUPERAMOS
                    productosList =
                        JsonConvert.DeserializeObject<List<Libro>>(jsonProductos);
                }

                //INCLUIMOS EL NUEVO PRODUCTO 
                productosList.Add(producto);

                //SERIALIZAMOS LA COLECCIÓN CON LOS NUEVOS DATOS DE NUEVO
                jsonProductos = JsonConvert.SerializeObject(productosList);

                //ALACENAMOS LOS NUEVOS DATOS EN AZURE CACHE REDIS con identificador único para cada user(su id)
                await this.database.StringSetAsync("favoritos" + "-" + idusuario, jsonProductos);
            }

        }

        //MÉTODO RECUPERAR TODOS LOS PRODUCTOS
        public async Task<List<Libro>> GetProducotsFavoritosAsync()
        {
            int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier));

            string jsonProductos =
               await this.database.StringGetAsync("favoritos" + "-" + idusuario);
            if (jsonProductos == null)
            {
                return null;
            }
            else
            {
                List<Libro> productos =
                    JsonConvert.DeserializeObject<List<Libro>>(jsonProductos);
                return productos;
            }
        }

        //ELIMINAR DE CACHE
        public async Task DeleteProductosAsync(int idproducto)
        {
            int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
                    .FindFirstValue(ClaimTypes.NameIdentifier));

            //ESTO NO ES UNA BASE DE DATOS, NO PODEMOS FILTRAR
            List<Libro> favoritos =
                await this.GetProducotsFavoritosAsync();
            if (favoritos != null)
            {
                Libro producto =
                    favoritos.FirstOrDefault(x => x.IdLibro == idproducto);
                //ELIMINAMOS EL PRODUCTO DE LA COLECCIÓN
                favoritos.Remove(producto);
                //HAY QUE COMPROBAR SI TODAVIA TENEMOS ALGUN PRODUCTO
                if (favoritos.Count == 0)
                {
                    //SI YA NO TENEMOS FAVORITOS ELIMINAMOS 
                    //LA KEY DE AZURE CACHE REDIS
                    await this.database.KeyDeleteAsync("favoritos" + "-" + idusuario);
                }
                else
                {
                    //ALMACENAMOS DE NUEVO LOS PRODUCTOS
                    string jsonProductos =
                        JsonConvert.SerializeObject(favoritos);
                    //PODEMOS INDICAR EL TIEMPO DE DURACIÓN DE LA KEY DE CACHE REDIS
                    //SINO SE DICE NADA, EL TIEMPO PREDETRMINADO ES DE 24h
                    await this.database.StringSetAsync("favoritos" + "-" + idusuario, jsonProductos, TimeSpan.FromMinutes(30));
                }
            }
        }
    }
}
