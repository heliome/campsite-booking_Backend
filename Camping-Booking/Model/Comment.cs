namespace Camping_Booking.Model
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int CampsiteId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string CommentText { get; set; }
        public int Rating { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
