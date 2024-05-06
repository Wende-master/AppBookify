namespace AppBookify.Models
{
    public class PaginacionLibrosModel
    {
        public int IdLibroView { get; set; }
        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string ISBN { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }
        public Decimal Precio { get; set; }
        public string NombreGenero { get; set; }
        public string NombreEstado { get; set; }

    }
}
