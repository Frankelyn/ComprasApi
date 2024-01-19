using ComprasApi.Models;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace ComprasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DetalleOrdenes _detalleOrdenes;
        public OrdenController(IConfiguration config, DetalleOrdenes detalleOrdenes)
        {
            _config = config;  
            _detalleOrdenes = detalleOrdenes;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Orden>>> GetAllOrdenes()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var ordenes = await SelectAllOrdenes(connection);
            return Ok(ordenes);

        }

        [HttpGet("{IDOrden}")]
        public async Task<ActionResult<Orden>> GetOrdenById(int IDOrden)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var orden = await connection.QueryFirstAsync<Orden>("Select * from Orden where IDOrden = @IdOrden ", new { IdOrden = IDOrden });
            return Ok(orden);
        }

        [HttpPost]
        public async Task<ActionResult<List<Orden>>> createFactura(Orden orden)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            // Insertar la orden y obtener el ID generado
            var insertedOrderId = await connection.ExecuteScalarAsync<int>("INSERT INTO Orden (Fecha, Total) VALUES (@Fecha, @Total); SELECT SCOPE_IDENTITY();", orden);

            // Asignar el IDOrden a cada detalle de la orden
            foreach (var item in orden.Detalle)
            {
                item.IDOrden = insertedOrderId;
            }

            // Crear los detalles de la orden
            await _detalleOrdenes.CreateDetallesOrdenes(orden.Detalle);

            // Devolver todas las órdenes (o puedes devolver la orden recién creada)
            return Ok(await SelectAllOrdenes(connection));
        }


        [HttpPut("{IDOrden}")]
        public async Task<ActionResult<List<Orden>>> UpdateOrden(int IDOrden, Orden orden)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            orden.IDOrden = IDOrden;
            await connection.ExecuteAsync("update Orden set Fecha = @Fecha, Total = @Total where IDOrden = @IDOrden", orden);
            return Ok(await SelectAllOrdenes(connection));
        }

        [HttpDelete("{IDOrden}")]
        public async Task<ActionResult<List<Orden>>> DeleteOrden(int IDOrden)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("Delete from Orden where IDOrden = @IdOrden", new { IdOrden = IDOrden });
            return Ok(await SelectAllOrdenes(connection));
        }







        private static async Task<IEnumerable<Orden>> SelectAllOrdenes(SqlConnection connection)
        {
            var orders = await connection.QueryAsync<Orden>("SELECT * FROM Orden");

            foreach (var order in orders)
            {
                order.Detalle = (List<DetalleOrdenes>)await connection.QueryAsync<DetalleOrdenes>("SELECT * FROM DetalleOrdenes WHERE IDOrden = @IDOrden", new { order.IDOrden });
            }

            return orders;
        }

    }
}
