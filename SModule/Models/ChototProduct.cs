using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Models
{
    public class ChototProduct
    {
        public long adId { get; set; }
        public long listId { get; set; }
        public long listTime { get; set; }
        public long accountId { get; set; }
        public String accountName { get; set; }
        public String subject { get; set; }
        public int category { get; set; }
        public String region { get; set; }
        public double price { get; set; }
        public String image { get; set; }
    }
    public class ChototApiWrapper
    {
        public int total { get; set; }
        public List<ChototProduct> ads { get; set; }
    }
}