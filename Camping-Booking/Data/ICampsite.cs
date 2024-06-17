using Camping_Booking.Model;

namespace Camping_Booking.Data
{
    public interface ICampsite
    {
        List<Campsite> GetCampsitesByOwner(int ownerId, int pageNumber, int pageSize);
        void AddCampsite(int ownerId, string name, string description, string country, string town, string address, string imageUrl);
        void AddFeatureToCampsite(int campsiteId, int featureId);
        void AddTerrainToCampsite(int campsiteId, int terrainId);
        int GetLastInsertedCampsiteId(int ownerId);
        bool IsUserOwner(int ownerId);
        bool ChangeCampsiteStatus(int campsiteId, bool isActive);
        bool TerminateCampsite(int campsiteId);
        (List<Campsite> campsites, bool hasMore) SearchCampsites(List<int> featureIds, List<int> terrainIds, string searchTerm, int pageNumber, int pageSize);
        Campsite GetCampsiteById(int campsiteId);
        void BookCampsite(int userId, int campsiteId, DateTime dateStart, DateTime dateEnd);
    }
}
