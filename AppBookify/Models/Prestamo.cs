namespace AppBookify.Models
{
    public class Prestamo
    {
        public int IdPrestamo { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdLibro { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public int? IdEstadoPrestamo { get; set; }
    }
}
