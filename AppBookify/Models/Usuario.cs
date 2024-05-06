namespace AppBookify.Models
{
    public class Usuario
    {
        public int IdUser { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public string Email { get; set; }

        public string? FotoPerfil { get; set; }

        public byte[] Password { get; set; }
        public string? Salt { get; set; }

        public bool? Activo { get; set; }
        public string TokenMail { get; set; }

        public int? RolId { get; set; }

    }
}
