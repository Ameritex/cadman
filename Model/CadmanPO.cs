using System.Collections.Generic;

namespace RestAPI.Model
{
    public class CadmanDocument
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
    }
    public class Tag
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
    public class Operation
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public DueDate DueDate { get; set; }
    }
    public class DueDate
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Min { get; set; }
        public int Sec { get; set; }
        public int MSec { get; set; }
        public int TZ { get; set; }
    }
    public class PartType
    {
        public string ID { get; set; }
        public string MaterialID { get; set; }
        public string Thickness { get; set; }
        public string Unit { get; set; }
        public string ImportFileName { get; set; }
        public List<Tag> Tags { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public List<CadmanDocument> DocumentList { get; set; }
    }

    public class ProductionOrder
    {
        public string ID { get; set; }
        public string PartID { get; set; }
        public string PartRev { get; set; }
        public string PartSubRev { get; set; }
        public double Quantity { get; set; }
        public string Project { get; set; }
        public List<Operation> OperationList { get; set; }
        public int Priority { get; set; }
        public List<Tag> Tags { get; set; }
        public List<CadmanDocument> DocumentList { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
    }

    public class CadmanPO
    {      
        public int PODet_ID { get; set; }
        public List<PartType> PartTypeList { get; set; }
        public List<ProductionOrder> ProductionOrderList { get; set; }
    }
}
