using Camping_Booking.Model;

namespace Camping_Booking.Data
{
    public interface IToken
    {
        Token GenerateJwtToken(int userId);
        void StoreTokenInDatabase(Token token);
        bool ValidateToken(string token);
        void InvalidateToken(string token);
        int GetUserIdFromToken(string token);
    }
}
