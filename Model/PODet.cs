using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class PODet
    {
     
        [Key]
        public int PODet_ID { get; set; }

        public string PONum { get; set; }
        public string JobNo { get; set; }
        public string PartNo { get; set; }
        public double QtyOrd { get; set; }
        public string Unit { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }

        public bool? Cadman_Generated { get; set; }
    }
}
