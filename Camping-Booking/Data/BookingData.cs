using System;
using System.Collections.Generic;
using Camping_Booking.Model;
using MySqlConnector;

namespace Camping_Booking.Data
{
    public class BookingData : IBooking
    {
        private readonly Database _database;

        public BookingData(Database database)
        {
            _database = database;
        }

        public List<Booking> GetBookingsByUser(int userId)
        {
            var bookings = new List<Booking>();

            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = @"
                        SELECT b.bookingid, b.campsiteid, b.userid, b.timestart, b.timeend, c.name as CampsiteName, c.image_url, b.active, b.active_owner
                        FROM booking b
                        JOIN campsite c ON b.campsiteid = c.campsiteid
                        WHERE b.userid = @UserId
                        ORDER BY b.timestart";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var booking = new Booking
                                {
                                    BookingId = reader.GetInt32("bookingid"),
                                    CampsiteId = reader.GetInt32("campsiteid"),
                                    UserId = reader.GetInt32("userid"),
                                    TimeStart = reader.GetDateTime("timestart"),
                                    TimeEnd = reader.GetDateTime("timeend"),
                                    CampsiteName = reader.GetString("CampsiteName"),
                                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? null : reader.GetString("image_url"),
                                    Active = reader.GetBoolean("active"),
                                    ActiveOwner = reader.GetBoolean("active_owner")
                                };

                                bookings.Add(booking);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetBookingsByUser error: {ex.Message}");
                throw;
            }

            return bookings;
        }

        public bool CancelBooking(int bookingId, int userId)
        {
            try
            {
                using (var connection = _database.GetConnection())
                {
                    string query = "UPDATE booking SET active = 0 WHERE bookingid = @BookingId AND userid = @UserId AND timestart > @Today";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookingId", bookingId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@Today", DateTime.Now);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CancelBooking error: {ex.Message}");
                throw;
            }
        }
    }
}
