namespace Camping_Booking.Model
{
    public class AddCampsiteRequest
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string Address { get; set; }
        public IFormFile Image { get; set; }
        public List<int> FeatureIds { get; set; } = new List<int>();
        public List<int> TerrainIds { get; set; } = new List<int>();

    }
}
