namespace AppBookify.Models
{
    public class PedidoConDetallesViewModel
    {
        public Pedido Pedido { get; set; }
        //public DetallesPedido Detalle { get; set; }
        public List<DetallesPedido> Detalles { get; set; }
    }
}
