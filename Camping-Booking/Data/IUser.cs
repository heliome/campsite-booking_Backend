using Camping_Booking.Model;

namespace Camping_Booking.Data
{
    public interface IUser
    {
        bool IsEmailUsed(string email);
        bool CreateNewUser(User newUser);
        int? VerifyEmailAndPassword(LoginRequest loginRequest);
        User GetUserInfo(int userId);
        bool UpdateUserInfo(User user);
        bool UpdateUserEmail(User user);
        bool UpdateUserPassword(User user);
    }
}
