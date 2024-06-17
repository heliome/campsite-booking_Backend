using Camping_Booking.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Camping_Booking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeaturesController : ControllerBase
    {
        private readonly Database _database;

        public FeaturesController(Database database)
        {
            _database = database;
        }

        [HttpGet("getAll")]
        public IActionResult GetAllFeatures()
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT FeatureID, Name FROM features";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var features = new List<object>();
                            while (reader.Read())
                            {
                                features.Add(new
                                {
                                    FeatureID = reader.GetInt32("FeatureID"),
                                    Name = reader.GetString("Name")
                                });
                            }
                            return Ok(features);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving features.", error = ex.Message });
            }
        }
    }
}
