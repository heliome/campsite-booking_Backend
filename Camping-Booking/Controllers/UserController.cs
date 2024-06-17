using Camping_Booking.Data;
using Camping_Booking.Model;
using Microsoft.AspNetCore.Mvc;

namespace Camping_Booking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUser _userData;
        private readonly IToken _tokenData;

        public UserController(IUser userData, IToken tokenData)
        {
            _userData = userData;
            _tokenData = tokenData;
        }

        [HttpPost("registration")]
        public IActionResult Register([FromBody] CreateUserRequest request)
        {
            try
            {
                if (_userData.IsEmailUsed(request.Email))
                {
                    return BadRequest(new { message = "Email is already in use." });
                }

                var newUser = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password,
                    Owner = false,
                    Active = true
                };

                _userData.CreateNewUser(newUser);
                return Ok(new { message = $"Sign up successful for: {newUser.FirstName}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return BadRequest(new { message = "Email and Password are required!" });
                }
                int? userId = _userData.VerifyEmailAndPassword(loginRequest);

                if (userId.HasValue)
                {
                    var token = _tokenData.GenerateJwtToken(userId.Value);
                    return Ok(new { token = token.TokenValue });
                }
                else
                {
                    return NotFound(new { message = "Invalid Email or Password" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging in.", error = ex.Message });
            }
        }

        [HttpGet("{userId}")]
        public IActionResult GetUserInfo(int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest(new { message = "Insert valid User Id" });
                }
                var user = _userData.GetUserInfo(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user information.", error = ex.Message });
            }
        }

        [HttpPost("showProfile")]
        public IActionResult GetProfile([FromBody] TokenRequest tokenRequest)
        {
            try
            {
                var token = tokenRequest.Token;
                if (!_tokenData.ValidateToken(token))
                {
                    return Unauthorized(new { message = "Invalid or inactive token." });
                }

                int userId = _tokenData.GetUserIdFromToken(token);
                var user = _userData.GetUserInfo(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProfile error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while retrieving profile.", error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var token = request.Token;
                if (!_tokenData.ValidateToken(token))
                {
                    return Unauthorized(new { message = "Invalid or inactive token." });
                }

                int userId = _tokenData.GetUserIdFromToken(token);
                var user = _userData.GetUserInfo(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                bool isUpdated = _userData.UpdateUserInfo(user);

                if (!isUpdated)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "User information updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating profile.", error = ex.Message });
            }
        }

        [HttpPut("profile/email")]
        public IActionResult UpdateEmail([FromBody] UpdateEmailRequest updateEmailRequest)
        {
            try
            {
                var token = updateEmailRequest.Token;
                if (!_tokenData.ValidateToken(token))
                {
                    return Unauthorized(new { message = "Invalid or inactive token." });
                }

                int userId = _tokenData.GetUserIdFromToken(token);
                var user = _userData.GetUserInfo(userId);

                if (user == null || user.Password != updateEmailRequest.OldPassword)
                {
                    return Unauthorized(new { message = "Invalid old password." });
                } 
                else if (user == null || _userData.IsEmailUsed(updateEmailRequest.Email))
                {
                    return BadRequest(new { message = "Email is already in use." });
                }

                user.Email = updateEmailRequest.Email;
                bool isUpdated = _userData.UpdateUserEmail(user);

                if (!isUpdated)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "Email updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating email.", error = ex.Message });
            }
        }

        [HttpPut("profile/password")]
        public IActionResult UpdatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            try
            {
                var token = updatePasswordRequest.Token;
                if (!_tokenData.ValidateToken(token))
                {
                    return Unauthorized(new { message = "Invalid or inactive token." });
                }

                int userId = _tokenData.GetUserIdFromToken(token);
                var user = _userData.GetUserInfo(userId);

                if (user == null || user.Password != updatePasswordRequest.OldPassword)
                {
                    return Unauthorized(new { message = "Invalid old password." });
                }

                user.Password = updatePasswordRequest.NewPassword;
                bool isUpdated = _userData.UpdateUserPassword(user);

                if (!isUpdated)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating password.", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] TokenRequest TokenRequest)
        {
            try
            {
                _tokenData.InvalidateToken(TokenRequest.Token);
                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging out.", error = ex.Message });
            }
        }
    }
}
