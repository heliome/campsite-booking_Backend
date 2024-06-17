namespace Camping_Booking.Model
{
    public class CancelBookingRequest
    {
        public string Token { get; set; }
        public int BookingId { get; set; }

    }
}
