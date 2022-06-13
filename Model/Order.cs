using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class Order
    {
        [Key]
        public int Orders_ID { get; set; }

        public string OrderNo { get; set; } 
        public string QuoteNo { get; set; }
        public string CustCode { get; set; }
        public string CustDesc { get; set; }
        public string PONum { get; set; }
        public string SalesID { get; set; }
        public DateTime? DateEnt { get; set; }

        public string ShipVia { get; set; }
    }
}
