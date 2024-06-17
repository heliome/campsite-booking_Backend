using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Camping_Booking.Model;
using MySqlConnector;
using System.Security.Claims;
using System.Text;


namespace Camping_Booking.Data
{
    public class TokenData : IToken
    {
        private readonly Database _database;
        private readonly string _secretKey = "YourNewLongerSecretKeyHereewfwefwefewfewfwefewfawedferolvyg3rjmpcrewjlkxghrweklxhjrepkoxgkjerwplgxjerlkxgherfjoxdbfdbdfbsfdbfdsbfdfbsdffbsdbdf";

        public TokenData(Database database)
        {
            _database = database;
        }

        public Token GenerateJwtToken(int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            StoreTokenInDatabase(new Token { UserId = userId, TokenValue = tokenString, Active = true });

            return new Token
            {
                UserId = userId,
                TokenValue = tokenString,
                Active = true
            };
        }

        public void StoreTokenInDatabase(Token token)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "INSERT INTO tokens (user_id, token, active) VALUES (@UserId, @Token, @Active)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", token.UserId);
                    command.Parameters.AddWithValue("@Token", token.TokenValue);
                    command.Parameters.AddWithValue("@Active", token.Active);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM tokens WHERE token = @Token AND active = 1";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Token", token);
                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation error: {ex.Message}");
                return false;
            }
        }

        public void InvalidateToken(string token)
        {
            using (var connection = _database.GetConnection())
            {
                string query = "UPDATE tokens SET active = @Active WHERE token = @Token";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@Token", token);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetUserIdFromToken(string token)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "SELECT user_id FROM tokens WHERE token = @Token AND active = 1";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Token", token);
                        var userId = command.ExecuteScalar();
                        if (userId != null)
                        {
                            return Convert.ToInt32(userId);
                        }
                        else
                        {
                            throw new Exception("Token does not exist or is inactive");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserIdFromToken error: {ex.Message}");
                throw;
            }
        }

    }
}
