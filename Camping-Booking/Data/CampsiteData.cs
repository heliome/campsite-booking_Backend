using Camping_Booking.Model;
using MySqlConnector;
using System.Runtime.CompilerServices;
using System.Text;

namespace Camping_Booking.Data
{
    public class CampsiteData : ICampsite
    {
        private readonly Database _database;

        public CampsiteData(Database database)
        {
            _database = database;
        }

        public List<Campsite> GetCampsitesByOwner(int ownerId, int pageNumber, int pageSize)
        {
            var campsites = new List<Campsite>();

            try
            {
                using (var connection = _database.GetConnection())
                {
                    int offset = (pageNumber - 1) * pageSize;
                    string query = "SELECT campsiteid, ownerid, name, description, country, town, address, active, terminate, numberofbookings, image_url " +
                                   "FROM campsite WHERE ownerid = @OwnerId AND terminate=0 LIMIT @PageSize OFFSET @Offset";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        command.Parameters.AddWithValue("@Offset", offset);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var campsite = new Campsite
                                {
                                    CampsiteId = reader.GetInt32("campsiteid"),
                                    OwnerId = reader.GetInt32("ownerid"),
                                    Name = reader.GetString("name"),
                                    Description = reader.GetString("description"),
                                    Country = reader.GetString("country"),
                                    Town = reader.GetString("town"),
                                    Address = reader.GetString("address"),
                                    Active = reader.GetBoolean("active"),
                                    Terminated = reader.GetBoolean("terminate"),
                                    NumberOfBookings = reader.GetInt32("numberofbookings"),
                                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url")
                                };

                                campsites.Add(campsite);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCampsitesByOwner error: {ex.Message}");
                throw;
            }

            return campsites;
        }


        public void AddCampsite(int ownerId, string name, string description, string country, string town, string address, string imageUrl)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "INSERT INTO campsite (ownerid, name, description, country, town, address, image_url, active, terminate, numberofbookings) VALUES (@OwnerId, @Name, @Description, @Country, @Town, @Address, @ImageUrl, 1, 0, 0)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Country", country);
                        command.Parameters.AddWithValue("@Town", town);
                        command.Parameters.AddWithValue("@Address", address);
                        command.Parameters.AddWithValue("@ImageUrl", imageUrl);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddCampsite error: {ex.Message}");
                throw;
            }
        }

        public int GetLastInsertedCampsiteId(int ownerId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT MAX(campsiteid) FROM campsite WHERE ownerid = @OwnerId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetLastInsertedCampsiteId error: {ex.Message}");
                throw;
            }
        }

        public void AddFeatureToCampsite(int campsiteId, int featureId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "INSERT INTO featurecamp (CampsiteID, FeatureID) VALUES (@CampsiteID, @FeatureID)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteID", campsiteId);
                        command.Parameters.AddWithValue("@FeatureID", featureId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddFeatureToCampsite error: {ex.Message}");
                throw;
            }
        }

        public void AddTerrainToCampsite(int campsiteId, int terrainId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "INSERT INTO terraincamp (CampsiteID, TerrainID) VALUES (@CampsiteID, @TerrainID)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteID", campsiteId);
                        command.Parameters.AddWithValue("@TerrainID", terrainId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddTerrainToCampsite error: {ex.Message}");
                throw;
            }
        }

        public bool IsUserOwner(int ownerId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM campsite WHERE ownerid = @OwnerId AND terminate = 0";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsUserOwner error: {ex.Message}");
                throw;
            }
        }

        public bool TerminateCampsite(int campsiteId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        string terminateCampsiteQuery = "UPDATE campsite SET terminate = 1 WHERE campsiteid = @CampsiteId";
                        using (var command = new MySqlCommand(terminateCampsiteQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        string updateBookingsQuery = "UPDATE booking SET active_owner = 0 WHERE campsiteid = @CampsiteId AND timestart > CURRENT_DATE";
                        using (var command = new MySqlCommand(updateBookingsQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TerminateCampsite error: {ex.Message}");
                throw;
            }
        }

        public bool ChangeCampsiteStatus(int campsiteId, bool isActive)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "UPDATE campsite SET active = @IsActive WHERE campsiteid = @CampsiteId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsActive", isActive);
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChangeCampsiteStatus error: {ex.Message}");
                throw;
            }
        }

        public (List<Campsite> campsites, bool hasMore) SearchCampsites(List<int> featureIds, List<int> terrainIds, string searchTerm, int pageNumber, int pageSize)
        {
            var campsites = new List<Campsite>();
            bool hasMore = false;

            try
            {
                using (var connection = _database.GetConnection())
                {
                    int offset = (pageNumber - 1) * pageSize;
                    var query = new StringBuilder();
                    query.Append("SELECT DISTINCT c.campsiteid, c.ownerid, c.name, c.description, c.country, c.town, c.address, c.active, c.terminate, c.numberofbookings, c.image_url ")
                         .Append("FROM campsite c ")
                         .Append("LEFT JOIN featurecamp fc ON c.campsiteid = fc.campsiteid ")
                         .Append("LEFT JOIN terraincamp tc ON c.campsiteid = tc.campsiteid ")
                         .Append("WHERE c.terminate = 0 AND c.active = 1 ");

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        query.Append("AND c.name LIKE @SearchTerm ");
                    }

                    if (featureIds != null && featureIds.Count > 0)
                    {
                        query.Append("AND c.campsiteid IN (SELECT campsiteid FROM featurecamp WHERE featureid IN (")
                             .Append(string.Join(",", featureIds))
                             .Append(") GROUP BY campsiteid HAVING COUNT(DISTINCT featureid) = @FeatureCount) ");
                    }

                    if (terrainIds != null && terrainIds.Count > 0)
                    {
                        query.Append("AND c.campsiteid IN (SELECT campsiteid FROM terraincamp WHERE terrainid IN (")
                             .Append(string.Join(",", terrainIds))
                             .Append(") GROUP BY campsiteid HAVING COUNT(DISTINCT terrainid) = @TerrainCount) ");
                    }

                    query.Append("LIMIT @PageSize OFFSET @Offset");

                    using (var command = new MySqlCommand(query.ToString(), connection))
                    {
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                        }

                        if (featureIds != null && featureIds.Count > 0)
                        {
                            command.Parameters.AddWithValue("@FeatureCount", featureIds.Count);
                        }

                        if (terrainIds != null && terrainIds.Count > 0)
                        {
                            command.Parameters.AddWithValue("@TerrainCount", terrainIds.Count);
                        }

                        command.Parameters.AddWithValue("@PageSize", pageSize + 1);
                        command.Parameters.AddWithValue("@Offset", offset);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var campsite = new Campsite
                                {
                                    CampsiteId = reader.GetInt32("campsiteid"),
                                    OwnerId = reader.GetInt32("ownerid"),
                                    Name = reader.GetString("name"),
                                    Description = reader.GetString("description"),
                                    Country = reader.GetString("country"),
                                    Town = reader.GetString("town"),
                                    Address = reader.GetString("address"),
                                    Active = reader.GetBoolean("active"),
                                    Terminated = reader.GetBoolean("terminate"),
                                    NumberOfBookings = reader.GetInt32("numberofbookings"),
                                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url")
                                };

                                campsites.Add(campsite);
                            }

                            if (campsites.Count > pageSize)
                            {
                                hasMore = true;
                                campsites.RemoveAt(pageSize);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchCampsites error: {ex.Message}");
                throw;
            }

            return (campsites, hasMore);
        }



        public Campsite GetCampsiteById(int campsiteId)
        {
            var campsite = new Campsite();
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT campsiteid, ownerid, name, description, country, town, address, active, terminate, numberofbookings, image_url FROM campsite WHERE campsiteid = @CampsiteId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                campsite.CampsiteId = reader.GetInt32("campsiteid");
                                campsite.OwnerId = reader.GetInt32("ownerid");
                                campsite.Name = reader.GetString("name");
                                campsite.Description = reader.GetString("description");
                                campsite.Country = reader.GetString("country");
                                campsite.Town = reader.GetString("town");
                                campsite.Address = reader.GetString("address");
                                campsite.Active = reader.GetBoolean("active");
                                campsite.Terminated = reader.GetBoolean("terminate");
                                campsite.NumberOfBookings = reader.GetInt32("numberofbookings");
                                campsite.ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url");
                            }
                        }
                    }

                    campsite.Features = new List<Feature>();
                    query = "SELECT f.featureid, f.name FROM features f INNER JOIN featurecamp fc ON f.featureid = fc.featureid WHERE fc.campsiteid = @CampsiteId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                campsite.Features.Add(new Feature
                                {
                                    FeatureId = reader.GetInt32("featureid"),
                                    Name = reader.GetString("name")
                                });
                            }
                        }
                    }

                    campsite.Terrains = new List<Terrain>();
                    query = "SELECT t.terrainid, t.name FROM terrain t INNER JOIN terraincamp tc ON t.terrainid = tc.terrainid WHERE tc.campsiteid = @CampsiteId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                campsite.Terrains.Add(new Terrain
                                {
                                    TerrainId = reader.GetInt32("terrainid"),
                                    Name = reader.GetString("name")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCampsiteById error: {ex.Message}");
                throw;
            }

            return campsite;
        }


        public void BookCampsite(int userId, int campsiteId, DateTime dateStart, DateTime dateEnd)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        string query = "INSERT INTO booking (campsiteid, userid, timestart, timeend, active, active_owner) VALUES (@CampsiteId, @UserId, @DateStart, @DateEnd, 1, 1)";
                        using (var command = new MySqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.Parameters.AddWithValue("@DateStart", dateStart);
                            command.Parameters.AddWithValue("@DateEnd", dateEnd);
                            command.ExecuteNonQuery();
                        }

                        string updateQuery = "UPDATE campsite SET numberofbookings = numberofbookings + 1 WHERE campsiteid = @CampsiteId";
                        using (var command = new MySqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BookCampsite error: {ex.Message}");
                throw;
            }
        }


    }
}