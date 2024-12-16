using ActiveCampaignAPIWrapper.Authentication;
using ActiveCampaignAPIWrapper.Helpers;
using ActiveCampaignAPIWrapper.Models;
using ActiveCampaignAPIWrapper.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Services
{
   
    public class CustomFieldService
    {
        private readonly ActiveCampaignClient _client;

        public CustomFieldService(ActiveCampaignClient client)   


        {
            _client = client;
        }


        // Create a new custom field
        public async Task<ResponseViewModel> CreateCustomFieldAsync(CustomField customField)
        {
            try
            {
                // Validate the required fields
                if (string.IsNullOrEmpty(customField.Title))
                {
                    return new ResponseViewModel
                    {
                        ret = 0,
                        responseMessage = "Custom field title is required."
                    };
                }

                if (string.IsNullOrEmpty(customField.Type))
                {
                    return new ResponseViewModel
                    {
                        ret = 0,
                        responseMessage = "Custom field type is required."
                    };
                }

                // Validate options if required for specific field types
                if ((customField.Type == "dropdown" || customField.Type == "multiselect" || customField.Type == "radio")
                    && string.IsNullOrEmpty(customField.Options))
                {
                    return new ResponseViewModel
                    {
                        ret = 0,
                        responseMessage = "Options are required for dropdown, multiselect, or radio field types."
                    };
                }

                // Prepare the field data based on field type
                var fieldData = new
                {
                    field = new
                    {
                        title = customField.Title,
                        type = customField.Type,
                        description = customField.Description,
                        visible_in_forms = true, // Make sure this is true if you want it visible in forms
                        options = CustomFieldHelper.GetOptionsForFieldType(customField) // Ensure this helper handles option validation correctly
                    }
                };

                // Send the request to create the custom field
                var response = await _client.PostAsJsonAsync("api/3/fields", fieldData);

                // Handle response and errors
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return new ResponseViewModel
                    {
                        ret = 0,
                        responseMessage = $"Error creating custom field: {errorResponse}"
                    };
                }

                // Deserialize the response and get the field ID
                var rawJson = await response.Content.ReadAsStringAsync();
                var createdField = JsonConvert.DeserializeObject<dynamic>(rawJson);

                // Assign ID to custom field
                customField.Id = long.Parse(createdField.field.id.ToString());

                return new ResponseViewModel
                {
                    ret = 1,
                    responseMessage = "Custom field created successfully!",
                    Id = customField.Id // Include the ID here for reference
                };
            }
            catch (HttpRequestException httpEx)
            {
                return new ResponseViewModel
                {
                    ret = 0,
                    responseMessage = $"Network error occurred while creating custom field: {httpEx.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel
                {
                    ret = 0,
                    responseMessage = $"An unexpected error occurred while creating custom field: {ex.Message}"
                };
            }
        }

        // Update a custom field value for a contact
        public async Task<ResponseViewModel> UpdateCustomFieldValueAsync(long contactId, long customFieldId, string value)
        {
            try
            {
                var data = new
                {
                    fieldValue = new
                    {
                        contact = contactId, // Contact ID
                        field = customFieldId, // Custom field ID
                        value = value // Value to set for the custom field
                    }
                };

                var response = await _client.PostAsJsonAsync("api/3/fieldValues", data);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return new ResponseViewModel
                    {
                        ret = 0,
                        responseMessage = $"Error updating custom field value: {errorResponse}"
                    };
                }

                return new ResponseViewModel
                {
                    ret = 1,
                    responseMessage = "Custom field value updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel
                {
                    ret = 0,
                    responseMessage = $"An error occurred while updating custom field value: {ex.Message}"
                };
            }
        }

       
    }
}



