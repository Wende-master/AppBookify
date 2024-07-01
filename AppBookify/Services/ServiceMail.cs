using AppBookify.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AppCoreBookify.Services
{
    public class ServiceMail
    {
        private MediaTypeWithQualityHeaderValue header;
        private string urlLogicApp;
        public ServiceMail(IConfiguration configuration)

        {
            var keyVaultUri = configuration.GetValue<string>("KeyVault:VaultUri");
            var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());

            this.urlLogicApp = GetSecretValue(secretClient, "BookifyLogic");

            this.header =
                new MediaTypeWithQualityHeaderValue("application/json");
        }

        private string GetSecretValue(SecretClient secretClient, string secretName)
        {
            try
            {
                KeyVaultSecret secret = secretClient.GetSecret(secretName);
                return secret.Value;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendEmailAsync

            (string email, string asunto, string mensaje)

        {
            MailModel model = new MailModel
            {
                Email = email,
                Asunto = asunto,
                Mensaje = mensaje
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);

                // Convertir modelo a JSON
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(urlLogicApp, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Correo enviado exitosamente.");
                }
                else
                {
                    Console.WriteLine("Error al enviar el correo " + response.StatusCode);
                }
            }

        }

    }
}
