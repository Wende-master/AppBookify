﻿using AppBookify.Helpers;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Security.Claims;
using AppBookify.Models;

namespace AppBookify.Services
{
    public class ServiceCacheRedis
    {
        //    private IDatabase database;
        //    private IHttpContextAccessor httpContextAccessor;
        //    public ServiceCacheRedis(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        //    {
        //        var connectionMultiplexer = HelperCacheMultiplexer.GetConnection(configuration);
        //        this.database = connectionMultiplexer.GetDatabase();

        //        this.httpContextAccessor = httpContextAccessor;
        //    }

        //    //MÉTODO ALMACENAR PRODUCTOS
        //    public async Task AddProductoFavoritoAsync(Libro producto)
        //    {

        //        if (this.httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
        //        {
        //            int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
        //                .FindFirstValue(ClaimTypes.NameIdentifier));

        //            string jsonProductos =
        //           await this.database.StringGetAsync("favoritos" + "-" + idusuario);
        //            List<Libro> productosList;
        //            if (jsonProductos == null)
        //            {
        //                productosList = new List<Libro>();

        //            }
        //            else
        //            {
        //                productosList =
        //                    JsonConvert.DeserializeObject<List<Libro>>(jsonProductos);
        //            }

        //            productosList.Add(producto);

        //            jsonProductos = JsonConvert.SerializeObject(productosList);

        //            //ALACENAMOS LOS NUEVOS DATOS EN AZURE CACHE REDIS con identificador único para cada user(su id)
        //            await this.database.StringSetAsync("favoritos" + "-" + idusuario, jsonProductos);
        //        }

        //    }

        //    //MÉTODO RECUPERAR TODOS LOS PRODUCTOS
        //    public async Task<List<Libro>> GetProductosFavoritosAsync()
        //    {
        //        int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
        //                .FindFirstValue(ClaimTypes.NameIdentifier));

        //        string jsonProductos =
        //           await this.database.StringGetAsync("favoritos" + "-" + idusuario);
        //        if (jsonProductos == null)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            List<Libro> productos =
        //                JsonConvert.DeserializeObject<List<Libro>>(jsonProductos);
        //            return productos;
        //        }
        //    }

        //    //ELIMINAR DE CACHE
        //    public async Task DeleteProductosAsync(int idproducto)
        //    {
        //        int idusuario = int.Parse(this.httpContextAccessor.HttpContext.User
        //                .FindFirstValue(ClaimTypes.NameIdentifier));

        //        //ESTO NO ES UNA BASE DE DATOS, NO PODEMOS FILTRAR
        //        List<Libro> favoritos =
        //            await this.GetProductosFavoritosAsync();
        //        if (favoritos != null)
        //        {
        //            Libro producto =
        //                favoritos.FirstOrDefault(x => x.IdLibro == idproducto);
        //            //ELIMINAMOS EL PRODUCTO DE LA COLECCIÓN
        //            favoritos.Remove(producto);
        //            //HAY QUE COMPROBAR SI TODAVIA TENEMOS ALGUN PRODUCTO
        //            if (favoritos.Count == 0)
        //            {
        //                await this.database.KeyDeleteAsync("favoritos" + "-" + idusuario);
        //            }
        //            else
        //            {
        //                string jsonProductos =
        //                    JsonConvert.SerializeObject(favoritos);
        //                //PODEMOS INDICAR EL TIEMPO DE DURACIÓN DE LA KEY DE CACHE REDIS
        //                //SINO SE DICE NADA, EL TIEMPO PREDETRMINADO ES DE 24h
        //                await this.database.StringSetAsync("favoritos" + "-" + idusuario, jsonProductos, TimeSpan.FromMinutes(30));
        //            }
        //        }
        //    }



    }
}
