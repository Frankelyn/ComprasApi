using static Dapper.SqlMapper;

namespace ComprasApi.Models
{
    public class Orden
    {
        public int IDOrden { get; set; }
        public DateTime Fecha { get; set; }
        public float Total { get; set; }
        public virtual List<DetalleOrdenes> Detalle { get; set; }

    }
}
