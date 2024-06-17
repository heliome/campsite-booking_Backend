using Camping_Booking.Model;
using MySqlConnector;

namespace Camping_Booking.Data
{
    public class CommentData : IComment
    {
        private readonly Database _database;

        public CommentData(Database database)
        {
            _database = database;
        }

        public void AddComment(int userId, int campsiteId, string commentText, int rating)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "INSERT INTO comments (campsiteid, userid, comment, rating) VALUES (@CampsiteId, @UserId, @Comment, @Rating)";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Comment", commentText);
                        command.Parameters.AddWithValue("@Rating", rating);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddComment error: {ex.Message}");
                throw;
            }
        }

        public List<Comment> GetCommentsByCampsite(int campsiteId)
        {
            var comments = new List<Comment>();

            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT c.commentid, c.campsiteid, c.userid, u.firstname, u.lastname, c.comment, c.rating, c.timestamp " +
                                   "FROM comments c JOIN user u ON c.userid = u.userid WHERE c.campsiteid = @CampsiteId ORDER BY c.timestamp DESC";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CampsiteId", campsiteId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comments.Add(new Comment
                                {
                                    CommentId = reader.GetInt32("commentid"),
                                    CampsiteId = reader.GetInt32("campsiteid"),
                                    UserId = reader.GetInt32("userid"),
                                    UserName = reader.GetString("firstname") + " " + reader.GetString("lastname"),
                                    CommentText = reader.GetString("comment"),
                                    Rating = reader.GetInt32("rating"),
                                    Timestamp = reader.GetDateTime("timestamp")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCommentsByCampsite error: {ex.Message}");
                throw;
            }

            return comments;
        }
    }
}
