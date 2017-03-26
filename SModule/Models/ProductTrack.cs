using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Models
{
    public class ProductTrack
    {
        public String title { get; set; }
        public String categoryId { get; set; }
        public List<String> tags { get; set; }
        public List<TrackedAttempt> trackedAttempts { get; set; }
        public Dictionary<String,TrackedUpdate> updates { get; set; } = new Dictionary<String, TrackedUpdate>();
    }
    public class TrackedAttempt
    {
        public String id { get; set; }
        public double price { get; set; }
        public List<String> trackedPlaces { get; set; }
    }
    public class ProductTrackContainer
    {
        public List<ProductTrack> products { get; set; }
    }
    public class TrackedUpdate
    {
        public String trackedPlaces { get; set; }
        public double price { get; set; }
        public String url { get; set; }
        public String location { get; set; }
        public String description { get; set; }
        public DateTime lastUpdate { get; set; }
        public String id { get; set; }
        public String fullPicture { get; set; }
    }
}