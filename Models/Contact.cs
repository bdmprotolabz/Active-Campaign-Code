﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Models
{
    public class Contact
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<CustomField> CustomFields { get; set; } = new List<CustomField>();
    }
}
