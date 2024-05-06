namespace AppBookify.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int? Albaran { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
        public int? IdEstadoPedido { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}
