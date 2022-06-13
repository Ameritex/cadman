using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{

    public class Materials
    {
        [Key]
        public int Materials_ID { get; set; }
        public string PartNo { get; set; }
        //public string Name { get; set; }
        public double? QTY { get; set; }
        public string Vendor { get; set; }
        //public string LotNumber { get; set; }
        public string SubPartNo { get; set; }
        public string Descrip { get; set; }


    }
}
