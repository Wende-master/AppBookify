namespace AppBookify.Models
{
    public class Libro
    {
        public int IdLibro { get; set; }
        public string Titulo { get; set; }
        public string? Descripcion { get; set; }
        public int? IdAutor { get; set; }
        public Autor Autor { get; set; }
        public string? ISBN { get; set; }
        public int? IdEstadoLibro { get; set; }
        public Decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? Imagen { get; set; }
        public List<GenerosLibro> GenerosLibros { get; set; }
        public List<Genero> Generos { get; set; }
    }
}
