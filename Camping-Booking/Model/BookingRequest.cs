namespace Camping_Booking.Model
{
    public class BookingRequest
    {
        public string Token { get; set; }
        public int CampsiteId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
