using ActiveCampaignAPIWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Helpers
{
    public static class CustomFieldHelper
    {
        // Helper method to get options based on field type
        public static object GetOptionsForFieldType(CustomField customField)
        {
            switch (customField.Type.ToLower())
            {
                case "dropdown":
                case "multiselect":
                case "radio":
                    return string.IsNullOrEmpty(customField.Options) ? null : customField.Options.Split(',').Select(option => option.Trim()).ToArray();

                case "checkbox":
                    // Checkbox might just need to have two values: true/false or 1/0
                    return new[] { "True", "False" };

                case "hidden":
                case "datetime":
                case "text":
                case "textarea":
                case "date":
                
                    return null;

                default:
                    return null;
            }
        }
    }
}

