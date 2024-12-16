using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Models
{
    public class Tag
    {
        public long Id { get; set; } // Unique identifier for the tag
        public string Name { get; set; }// Name of the tag
        public string Description { get; set; } // Optional description for the tag
    }
}
