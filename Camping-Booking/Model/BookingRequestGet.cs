namespace Camping_Booking.Model
{
    public class BookingRequestGet
    {
        public string Token { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 4;
    }
}
