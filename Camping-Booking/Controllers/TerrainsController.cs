using Camping_Booking.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Camping_Booking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TerrainsController : ControllerBase
    {
        private readonly Database _database;

        public TerrainsController(Database database)
        {
            _database = database;
        }

        [HttpGet("getAll")]
        public IActionResult GetAllTerrains()
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT TerrainID, Name FROM terrain";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var terrains = new List<object>();
                            while (reader.Read())
                            {
                                terrains.Add(new
                                {
                                    TerrainID = reader.GetInt32("TerrainID"),
                                    Name = reader.GetString("Name")
                                });
                            }
                            return Ok(terrains);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving terrains.", error = ex.Message });
            }
        }
    }
}
