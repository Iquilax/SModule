﻿using System;
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
        public double price { get; set; }
        public List<String> trackedPlaces { get; set; }
        public List<TrackedUpdate> updates { get; set; } = new List<TrackedUpdate>();
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
    }
}