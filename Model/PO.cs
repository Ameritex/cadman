using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class PO
    {
     
        [Key]
        public int PO_ID { get; set; }

        public string PONum { get; set; }
        public DateTime DateEnt { get; set; }

    }
}
