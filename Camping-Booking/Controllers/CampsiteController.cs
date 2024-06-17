using Camping_Booking.Data;
using Camping_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Camping_Booking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampsiteController : ControllerBase
    {
        private readonly IUser _userData;
        private readonly IToken _tokenData;
        private readonly ICampsite _campsiteData;
        private readonly IComment _commentData;

        public CampsiteController(IUser userData, IToken tokenData, ICampsite campsiteData, IComment commentData)
        {
            _userData = userData;
            _tokenData = tokenData;
            _campsiteData = campsiteData;
            _commentData = commentData;
        }

        [HttpPost("getCampsites")]
        public IActionResult GetCampsites([FromBody] TokenRequest tokenRequest, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var token = tokenRequest.Token;
                if (!_tokenData.ValidateToken(token))
                {
                    return Unauthorized(new { message = "Invalid or inactive token." });
                }

                int ownerId = _tokenData.GetUserIdFromToken(token);
                var user = _userData.GetUserInfo(ownerId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                bool isOwner = _campsiteData.IsUserOwner(ownerId);
                var campsites = _campsiteData.GetCampsitesByOwner(ownerId, pageNumber, pageSize);

                return Ok(new { message = "Campsites retrieved successfully.", isOwner, campsites });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campsites.", error = ex.Message });
            }
        }



        [HttpPost("addCampsite")]
        public IActionResult AddCampsite([FromForm] AddCampsiteRequest request)
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

                string imageUrl = null;
                if (request.Image != null)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

                    var filePath = Path.Combine(uploads, request.Image.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        request.Image.CopyTo(stream);
                    }

                    imageUrl = $"/uploads/{request.Image.FileName}";
                }

                _campsiteData.AddCampsite(userId, request.Name, request.Description, request.Country, request.Town, request.Address, imageUrl);

                int campsiteId = _campsiteData.GetLastInsertedCampsiteId(userId);

                if (request.FeatureIds != null && request.FeatureIds.Count > 0)
                {
                    foreach (var featureId in request.FeatureIds)
                    {
                        _campsiteData.AddFeatureToCampsite(campsiteId, featureId);
                    }
                }

                if (request.TerrainIds != null && request.TerrainIds.Count > 0)
                {
                    foreach (var terrainId in request.TerrainIds)
                    {
                        _campsiteData.AddTerrainToCampsite(campsiteId, terrainId);
                    }
                }

                return Ok(new { message = "Campsite added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the campsite.", error = ex.Message });
            }
        }

        [HttpPost("deactivateCampsite")]
        public IActionResult DeactivateCampsite([FromBody] ChangeCampsiteStatusRequest request)
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

                bool success = _campsiteData.ChangeCampsiteStatus(request.CampsiteId, false);
                if (!success)
                {
                    return NotFound(new { message = "Campsite not found or you don't have permission to deactivate it." });
                }

                return Ok(new { message = "Campsite deactivated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deactivating the campsite.", error = ex.Message });
            }
        }

        [HttpPost("activateCampsite")]
        public IActionResult ActivateCampsite([FromBody] ChangeCampsiteStatusRequest request)
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

                bool success = _campsiteData.ChangeCampsiteStatus(request.CampsiteId, true);
                if (!success)
                {
                    return NotFound(new { message = "Campsite not found or you don't have permission to activate it." });
                }

                return Ok(new { message = "Campsite activated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while activating the campsite.", error = ex.Message });
            }
        }

        [HttpPost("terminateCampsite")]
        public IActionResult TerminateCampsite([FromBody] ChangeCampsiteStatusRequest request)
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

                bool success = _campsiteData.TerminateCampsite(request.CampsiteId);
                if (!success)
                {
                    return NotFound(new { message = "Campsite not found or you don't have permission to terminate it." });
                }

                return Ok(new { message = "Campsite terminated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while terminating the campsite.", error = ex.Message });
            }
        }

        [HttpPost("searchCampsites")]
        public IActionResult SearchCampsites([FromBody] SearchCampsitesRequest request, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            try
            {
                var (campsites, hasMore) = _campsiteData.SearchCampsites(request.FeatureIds, request.TerrainIds, request.SearchTerm, pageNumber, pageSize);

                if (campsites == null || !campsites.Any())
                {
                    return Ok(new { message = "No campsites found.", campsites = new List<Campsite>(), hasMore = false });
                }

                return Ok(new { message = "Campsites retrieved successfully.", campsites, hasMore });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching campsites.", error = ex.Message });
            }
        }


        [HttpGet("{campsiteId}")]
        public IActionResult GetCampsite(int campsiteId)
        {
            try
            {
                var campsite = _campsiteData.GetCampsiteById(campsiteId);

                if (campsite == null)
                {
                    return NotFound(new { message = "Campsite not found." });
                }

                return Ok(campsite);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campsite details.", error = ex.Message });
            }
        }

        [HttpPost("bookCampsite")]
        public IActionResult BookCampsite([FromBody] BookingRequest bookingRequest)
        {
            try
            {
                var token = bookingRequest.Token;
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

                _campsiteData.BookCampsite(userId, bookingRequest.CampsiteId, bookingRequest.DateStart, bookingRequest.DateEnd);

                return Ok(new { message = "Campsite booked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while booking the campsite.", error = ex.Message });
            }
        }

        [HttpPost("addComment")]
        public IActionResult AddComment([FromBody] AddCommentRequest request)
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

                _commentData.AddComment(userId, request.CampsiteId, request.CommentText, request.Rating);
                return Ok(new { message = "Comment added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the comment.", error = ex.Message });
            }
        }

        [HttpGet("{campsiteId}/comments")]
        public IActionResult GetComments(int campsiteId)
        {
            try
            {
                var comments = _commentData.GetCommentsByCampsite(campsiteId);

                if (comments == null || !comments.Any())
                {
                    return Ok(new { message = "No comments found.", comments = new List<Comment>() });
                }

                return Ok(new { message = "Comments retrieved successfully.", comments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving comments.", error = ex.Message });
            }
        }

    }
}

