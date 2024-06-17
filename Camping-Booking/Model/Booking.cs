namespace Camping_Booking.Model
{
    public class Booking
    {
        public int BookingId { get; set; }
        public int CampsiteId { get; set; }
        public int UserId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public string CampsiteName { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
        public bool ActiveOwner { get; set; }
    }
}
