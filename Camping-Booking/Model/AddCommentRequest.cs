namespace Camping_Booking.Model
{
    public class AddCommentRequest
    {
        public string Token { get; set; }
        public int CampsiteId { get; set; }
        public string CommentText { get; set; }
        public int Rating { get; set; }

    }
}
