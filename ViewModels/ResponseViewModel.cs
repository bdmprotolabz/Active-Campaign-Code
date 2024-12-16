using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.ViewModels
{
    public class ResponseViewModel
    {
        public int ret { get; set; }          // Indicates success (1) or failure (0)
        public string responseMessage { get; set; }  // The message detailing the outcome
        public long? Id { get; set; }
    }

}
