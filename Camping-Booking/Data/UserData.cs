using Camping_Booking.Model;
using MySqlConnector;

namespace Camping_Booking.Data
{
    public class UserData: IUser
    {
        private readonly Database _database;

        public UserData(Database database)
        {
            _database = database;
        }

        public bool IsEmailUsed(string email)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM USER WHERE email = @Email";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public bool CreateNewUser(User newUser)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "INSERT INTO USER (firstname, lastname, email, password) VALUES (@FirstName, @LastName, @Email, @Password)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@FirstName", newUser.FirstName);
                command.Parameters.AddWithValue("@LastName", newUser.LastName);
                command.Parameters.AddWithValue("@Email", newUser.Email);
                command.Parameters.AddWithValue("@Password", newUser.Password);

                return command.ExecuteNonQuery() > 0;
            }
        }

        public int? VerifyEmailAndPassword(LoginRequest loginRequest)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "SELECT userid FROM USER WHERE email = @Email AND password = @Password";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", loginRequest.Email);
                command.Parameters.AddWithValue("@Password", loginRequest.Password);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : (int?)null;
            }
        }

        public User GetUserInfo(int userId)
        {
            try
            {
                Console.WriteLine($"Fetching user info for userId: {userId}");

                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT userid, firstname, lastname, email, password, owner, active FROM user WHERE userid = @UserId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = reader.GetInt32("userid"),
                                    FirstName = reader.GetString("firstname"),
                                    LastName = reader.GetString("lastname"),
                                    Email = reader.GetString("email"),
                                    Password = reader.GetString("password"),
                                    Owner = reader.GetBoolean("owner"),
                                    Active = reader.GetBoolean("active")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserInfo error: {ex.Message}");
                throw;
            }

            return null;
        }

        public bool UpdateUserInfo(User user)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "UPDATE user SET firstname = @FirstName, lastname = @LastName WHERE userid = @UserId";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", user.UserId);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);

                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateUserEmail(User user)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "UPDATE user SET email = @Email WHERE userid = @UserId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    int affectedRows = command.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
        }

        public bool UpdateUserPassword(User user)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "UPDATE user SET password = @Password WHERE userid = @UserId";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@UserId", user.UserId);
                    int affectedRows = command.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
        }


    }
}
