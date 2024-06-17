namespace Camping_Booking.Model
{
    public class Campsite
    {
        public int CampsiteId { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string Town { get; set; }
        public string Address { get; set; }
        public bool Active { get; set; }
        public bool Terminated { get; set; }
        public int NumberOfBookings { get; set; }
        public string ImageUrl { get; set; }
        public List<Feature> Features { get; set; }
        public List<Terrain> Terrains { get; set; }
    }

    public class Feature
    {
        public int FeatureId { get; set; }
        public string Name { get; set; }
    }

    public class Terrain
    {
        public int TerrainId { get; set; }
        public string Name { get; set; }
    }
}
