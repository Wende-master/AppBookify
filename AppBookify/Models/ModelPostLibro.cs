namespace AppBookify.Models
{
    public class ModelPostLibro
    {
        public Libro Libro { get; set; }
        public Autor Autor { get; set; }
        public List<int> GenerosDeLibro { get; set; }
    }
}
