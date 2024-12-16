using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Models
{
    public class Activity
    {
        public long Id { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
