using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Models
{
    public class GroupPost
    {
        public List<PostDetailRaw> data { get; set; }
    }
    public class PostDetailRaw
    {
        public String id { get; set; }
        public String message { get; set; }
        [JsonProperty("updated_time")]
        public String updatedTime { get; set; }
        [JsonProperty("full_picture")]
        public String fullPicture { get; set; }
        [JsonProperty("permalink_url")]
        public String permalinkUrl { get; set; }
    }
    public class PostDetailParsed
    {
        public String id { get; set; }
        public String product { get; set; }
        public String location { get; set; }
        public String updatedTime { get; set; }
        public String price { get; set; }
        public String fullPicture { get; set; }
        public String permalinkUrl { get; set; }
        public String description { get; set; }

    }
}