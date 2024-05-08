﻿using AppBookify.Filters;
using AppBookify.Models;
using AppBookify.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppBookify.Controllers
{
    public class ValoracionesController : Controller
    {
        private ServiceBookify service;
        public ValoracionesController(ServiceBookify service)
        {
            this.service = service;
        }


        [AuthorizeUser]
        public async Task<IActionResult> RegistrarValoracion(int? idlibro)
        {
            
            if (idlibro != null)
            {
                Libro libro = await this.service.FindLibroAsync(idlibro.Value);

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
    }
}