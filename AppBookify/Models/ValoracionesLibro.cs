namespace AppBookify.Models
{
    public class ValoracionesLibro
    {
        public int IdValoracion { get; set; }
        public int? IdLibro { get; set; }
        public int? IdUsuario { get; set; }
        public Decimal? ValoracionDecimal { get; set; }
        public string? Comentario { get; set; }
    }
}
