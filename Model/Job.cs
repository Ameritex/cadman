using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class Job
    {      
        public int ID { get; set; } 
        public string JobNo { get; set; }
        public string CustDesc { get; set; }
        public string CustContact { get; set; }
        public string PONum { get; set; }
        public string OrderNo { get; set; }
        public string PartNo { get; set; }
        public string PartDesc { get; set; }     
        public DateTime? JobDue { get; set; }
        public int Qty { get; set; }
        public string ProdCode { get; set; }
        public string Comments { get; set; }

        public bool EmptyFile { get; set; }
    }
}
