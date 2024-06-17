namespace Camping_Booking.Model
{
    public class UpdatePasswordRequest
    {
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
