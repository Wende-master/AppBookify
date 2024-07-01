using AppBookify.Models;
using AppCoreBookify.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppBookify.Controllers
{
    public class ContactoController : Controller
    {
        private ServiceMail serviceMail;
        public ContactoController(ServiceMail serviceMail)
        {
            this.serviceMail = serviceMail;
        }

        public IActionResult Contacto()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contacto(MailModel mailModel)
        {
            if (mailModel != null)
            {
                await this.serviceMail.SendEmailAsync(mailModel.Email, mailModel.Asunto, mailModel.Mensaje);
                ViewData["ENVIADO"] = "Correo enviado exitosamente.";
                ModelState.Clear();
                return View();
            }
            return View(mailModel);
        }

    }
}
