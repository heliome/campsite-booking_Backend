namespace Camping_Booking.Model
{
    public class SearchCampsitesRequest
    {
        public List<int> FeatureIds { get; set; }
        public List<int> TerrainIds { get; set; }
        public string SearchTerm { get; set; }
    }
}
