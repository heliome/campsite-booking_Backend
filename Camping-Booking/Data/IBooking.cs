using Camping_Booking.Model;

namespace Camping_Booking.Data
{
    public interface IBooking
    {
        List<Booking> GetBookingsByUser(int userId);
        bool CancelBooking(int bookingId, int userId);
    }
}
