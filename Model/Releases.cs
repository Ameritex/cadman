using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class Releases
    {
        [Key]
        public int Releases_ID { get; set; }

        public string? OrderNo { get; set; } 
        public string? JobNo { get; set; }
        public string? PartNo { get; set; }
        public string? PartDesc { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PrevDueDate { get; set; }
        public DateTime? LastModDate { get; set; }
        public bool Processed { get; set; }
        public bool EmptyFile { get; set; }
        public int Qty { get; set; }
        public int? PrevQty { get; set; }
        public Int16 DelType { get; set; }
        public DateTime? DateComplete { get; set; }
        public string Comments { get; set; }

        public bool? Cadman_Generated { get; set; }
    }
}
