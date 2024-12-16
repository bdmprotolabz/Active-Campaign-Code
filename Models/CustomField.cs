using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Models
{
    public class CustomField
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }  // e.g., text, dropdown, etc.
        public string Description { get; set; }
        public string Options { get; set; }  // Options for dropdown type fields, if applicable
        public string Value { get; set; }  // You need to add this if you want to store the field's value
        public string Group { get; set; }  // Add the Group property to store the field group
    }

    

}
