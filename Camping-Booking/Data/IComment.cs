using Camping_Booking.Model;

namespace Camping_Booking.Data
{
    public interface IComment
    {
        void AddComment(int userId, int campsiteId, string commentText, int rating);
        List<Comment> GetCommentsByCampsite(int campsiteId);

    }
}
