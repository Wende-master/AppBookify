namespace AppBookify.Models
{
    public class ModelLibro
    {
        public Libro Libros { get; set; }
        public EstadoLibro EstadosLibros { get; set; }
        public Autor Autor { get; set; }
        public List<ValoracionesLibro> ValoracionesLibros { get; set; }
        public List<Genero> GenerosLibros { get; set; }
    }
}
