using System;
using System.ComponentModel.DataAnnotations;

namespace RestAPI.Model
{
    
    public class PartTemplateRevision
    {
        [Key]
        public Guid ID { get; set; }

        public string Name { get; set; }

        public bool NameUnique { get; set; }
    }
}
