namespace AppBookify.Models
{
    public class DetallesPedido
    {
        public int IdDetallePedido { get; set;}
        public int? IdPedido { get; set; }
        public int? IdLibro { get; set; }
        public int Cantidad { get; set; }
    }
}
