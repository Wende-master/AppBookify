namespace AppBookify.Models
{
    public class VistaPedidosUsuario
    {
        public int UsuarioId { get; set; }

        public string Nombre { get; set; }

        public string Apellido { get; set; }

        public int PedidoId { get; set; }

        public DateTime FechaPedido { get; set; }

        public int? EstadoPedidoId { get; set; }

        public decimal? PrecioTotal { get; set; }

        public int DetallePedidoId { get; set; }

        public int Cantidad { get; set; }

        public int LibroId { get; set; }
        public string Titulo { get; set; }
        public string Imagen { get; set; }
    }
}
