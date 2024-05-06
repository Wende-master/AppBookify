namespace AppBookify.Models
{
    public class LibrosViewModel
    {
        public int LibroId { get; set; }

        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        public int? AutorId { get; set; }

        public string? NombreAutor { get; set; }

        public int? EstadoLibroId { get; set; }

        public string? EstadoLibro { get; set; }

        public int Cantidad { get; set; }

        public string Imagen { get; set; }

        public decimal? PrecioUnitario { get; set; }

        public decimal? ValoracionPromedio { get; set; }

        public int? GeneroId { get; set; }

        public string? NombreGenero { get; set; }
        public string? DescripcionGenero { get; set; }

    }
}
