using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;

namespace ComprasApi.Models
{
    public class DetalleOrdenes
    {
        public int ID { get; set; }
        public int IDOrden { get; set; }
        public int IDProducto { get; set; }
        public int Cantidad { get; set; }

        public float Precio { get; set; }

        private readonly IConfiguration _config;
        private readonly string? _connectionString;

        public DetalleOrdenes()
        {

        }

        public DetalleOrdenes(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }


        public async Task<List<DetalleOrdenes>> CreateDetallesOrdenes(List<DetalleOrdenes> detallesOrdenes)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var detalle in detallesOrdenes)
                    {
                        detalle.ID = 0;

                        var query = "Insert into DetalleOrdenes (IDOrden, IDProducto, Cantidad, Precio) values (@IDOrden, @IDProducto, @Cantidad, @Precio)";
                        await connection.ExecuteAsync(query, detalle, transaction);
                    }
                    transaction.Commit();

                    return (List<DetalleOrdenes>)await SelectAllDetalleOrdenes(connection);
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }


        private static async Task<IEnumerable<DetalleOrdenes>> SelectAllDetalleOrdenes(SqlConnection connection)
        {
            return await connection.QueryAsync<DetalleOrdenes>("Select * From DetalleOrdenes");
        }

    }
}
