namespace Camping_Booking.Model
{
    public class UpdateEmailRequest
    {
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string Email { get; set; }
    }
}
