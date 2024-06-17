using Camping_Booking.Data;
using Camping_Booking.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Camping_Booking.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IToken _tokenData;
        private readonly IUser _userData;
        private readonly IBooking _bookingData;

        public BookingController(IToken tokenData, IUser userData, IBooking bookingData)
        {
            _tokenData = tokenData;
            _userData = userData;
            _bookingData = bookingData;
        }

        [HttpPost("getBookings")]
        public IActionResult GetBookings([FromBody] BookingRequestGet request)
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

                var allBookings = _bookingData.GetBookingsByUser(userId);
                var today = DateTime.Today;

                var currentBookings = allBookings
                    .Where(b => b.TimeStart <= today && b.TimeEnd >= today && b.Active && b.ActiveOwner)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var futureBookings = allBookings
                    .Where(b => b.TimeStart > today && b.Active && b.ActiveOwner)
                    .OrderBy(b => b.TimeStart)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var pastBookings = allBookings
                    .Where(b => (b.TimeEnd < today) || (b.Active == false) || ( b.ActiveOwner == false))
                    .OrderByDescending(b => b.TimeStart)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return Ok(new
                {
                    currentBookings,
                    futureBookings,
                    pastBookings,
                    hasMoreCurrent = allBookings.Count(b => b.TimeStart <= today && b.TimeEnd >= today && b.Active && b.ActiveOwner) > request.PageNumber * request.PageSize,
                    hasMoreFuture = allBookings.Count(b => b.TimeStart > today && b.Active && b.ActiveOwner) > request.PageNumber * request.PageSize,
                    hasMorePast = allBookings.Count(b => (b.TimeEnd < today) || (b.Active == false && b.ActiveOwner == false)) > request.PageNumber * request.PageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving bookings.", error = ex.Message });
            }
        }

        [HttpPost("cancelBooking")]
        public IActionResult CancelBooking([FromBody] CancelBookingRequest request)
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

                bool success = _bookingData.CancelBooking(request.BookingId, userId);
                if (!success)
                {
                    return NotFound(new { message = "Booking not found or you don't have permission to cancel it." });
                }

                return Ok(new { message = "Booking cancelled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while cancelling the booking.", error = ex.Message });
            }
        }
    }
}
