using ComprasApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;

namespace ComprasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleOrdenesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string? _connectionString;

        public DetalleOrdenesController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleOrdenes>>> GetAllDetalleOrdenes()
        {
            using var connection = new SqlConnection(_connectionString);
            var detalleOrdenes = await SelectAllDetalleOrdenes(connection);
            return Ok(detalleOrdenes);

        }

        [HttpGet("{ID}")]
        public async Task<ActionResult<DetalleOrdenes>> GetDetalleOrdenesById(int ID)
        {
            using var connection = new SqlConnection(_connectionString);
            var detalleOrdenes = await connection.QueryFirstAsync<DetalleOrdenes>("Select * from DetalleOrdenes where ID = @Id ", new { Id = ID });
            return Ok(detalleOrdenes);
        }

        [HttpPost("detalleOrdenes")]
        public async Task<ActionResult<List<DetalleOrdenes>>> CreateDetalleOrdenes(DetalleOrdenes detalleOrdenes)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Insert into DetalleOrdenes (IDOrden, IDProducto, Cantidad, Precio) values (@IDOrden, @IDProducto, @Cantidad, @Precio)", detalleOrdenes);
            return Ok(await SelectAllDetalleOrdenes(connection));
        }


       
        [HttpPost("detallesOrdenes")]
        public async Task<ActionResult<List<DetalleOrdenes>>> CreateDetallesOrdenes(List<DetalleOrdenes> detallesOrdenes)
        {
            using var connection = new SqlConnection(_connectionString);

            try
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var detalle in detallesOrdenes)
                        {
            
                            detalle.ID = 0;

                            var query = "Insert into DetalleOrdenes (IDOrden, IDProducto, Cantidad, Precio) values (@IDOrden, @IDProducto, @Cantidad, @Precio)";
                            await connection.ExecuteAsync(query, detalle, transaction);
                        }

                        // Commit de la transacción si todas las inserciones son exitosas
                        transaction.Commit();

                        return Ok(await SelectAllDetalleOrdenes(connection));
                    }
                    catch (Exception ex)
                    {
                        // En caso de error, hacer un rollback de la transacción
                        transaction.Rollback();
                        return StatusCode(500, $"Error interno: {ex.Message}");
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }



        [HttpPut("{ID}")]
        public async Task<ActionResult<List<DetalleOrdenes>>> UpdateDetalleOrdenes(int ID, DetalleOrdenes detalleOrdenes)
        {
            using var connection = new SqlConnection(_connectionString);
            detalleOrdenes.ID = ID;
            await connection.ExecuteAsync("update DetalleOrdenes set IDOrden = @IDOrden, IDProducto = @IDProducto, Cantidad = @Cantidad where ID = @ID", detalleOrdenes);
            return Ok(await SelectAllDetalleOrdenes(connection));
        }

        [HttpDelete("{ID}")]
        public async Task<ActionResult<List<DetalleOrdenes>>> DeleteDetalleOrdenes(int ID)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Delete from DetalleOrdenes where ID = @Id", new { Id = ID });
            return Ok(await SelectAllDetalleOrdenes(connection));
        }



        private static async Task<IEnumerable<DetalleOrdenes>> SelectAllDetalleOrdenes(SqlConnection connection)
        {
            return await connection.QueryAsync<DetalleOrdenes>("Select * From DetalleOrdenes");
        }
    }
}
