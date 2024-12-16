using ActiveCampaignAPIWrapper.Authentication;
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
    public class ContactService
    {
        private readonly ActiveCampaignClient _client;

        public ContactService(ActiveCampaignClient client)
            {
                _client = client;
            }

        //Create Contact
        public async Task<ResponseViewModel> CreateContactAsync(Contact contact)
        {
            var contactData = new
            {
                contact = new
                {
                    email = contact.Email,
                    first_name = contact.FirstName,
                    last_name = contact.LastName,
                    phone = contact.Phone
                }
            };

            try
            {
                var response = await _client.PostAsJsonAsync("api/3/contacts", contactData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();

                    // Check for validation error
                    try
                    {
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                        var errors = errorObj?.errors;

                        if (errors != null && errors.Count > 0)
                        {
                            string validationError = errors[0]?.title?.ToString();
                            return new ResponseViewModel
                            {
                                ret = 0, // Failure
                                responseMessage = $"Validation error: {validationError}"
                            };
                        }
                    }
                    catch
                    {
                        // In case of unexpected response format, return generic error
                    }

                    return new ResponseViewModel
                    {
                        ret = 0, // Failure
                        responseMessage = $"Error creating contact: {errorResponse}"
                    };
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var createdContact = JsonConvert.DeserializeObject<dynamic>(rawJson);
                long contactId = long.Parse(createdContact.contact.id.ToString());

                return new ResponseViewModel
                {
                    ret = 1, // Success
                    responseMessage = "Contact created successfully!",
                    Id = contactId
                };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel
                {
                    ret = 0, // Failure
                    responseMessage = $"An error occurred while creating contact: {ex.Message}"
                };
            }
        }
         
        // Update an existing contact in ActiveCampaign
        public async Task<ResponseViewModel> UpdateContactAsync(Contact contact)
        {
            var contactData = new
            {
                contact = new
                {
                    email = contact.Email,
                    first_name = contact.FirstName,
                    last_name = contact.LastName,
                    phone = contact.Phone
                }
            };

            try
            {
                var response = await _client.PutAsJsonAsync($"api/3/contacts/{contact.Id}", contactData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return new ResponseViewModel
                    {
                        ret = 0, // Failure
                        responseMessage = $"Error updating contact: {errorResponse}"
                    };
                }

                return new ResponseViewModel
                {
                    ret = 1, // Success
                    responseMessage = "Contact updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel
                {
                    ret = 0, // Failure
                    responseMessage = $"An error occurred while updating contact: {ex.Message}"
                };
            }
        }

        // Delete a contact by its ID
        public async Task<ResponseViewModel> DeleteContactAsync(long contactId)
        {
            try
            {
                var response = await _client.DeleteAsync($"api/3/contacts/{contactId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return new ResponseViewModel
                    {
                        ret = 0, // Failure
                        responseMessage = $"Error deleting contact: {errorResponse}"
                    };
                }

                return new ResponseViewModel
                {
                    ret = 1, // Success
                    responseMessage = "Contact deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new ResponseViewModel
                {
                    ret = 0, // Failure
                    responseMessage = $"An error occurred while deleting contact: {ex.Message}"
                };
            }
        }

        // Fetch a contact by its ID
        public async Task<Contact> GetContactByIdAsync(long contactId)
        {
            try
            {
                // Fetch contact details, tags, and custom fields in parallel
                var contactTask = _client.GetAsync($"api/3/contacts/{contactId}");
                var tagsTask = _client.GetAsync($"api/3/contacts/{contactId}/contactTags");
                var customFieldsTask = _client.GetAsync($"api/3/contacts/{contactId}/fieldValues");

                // Await all tasks concurrently
                await Task.WhenAll(contactTask, tagsTask, customFieldsTask);

                // Handle contact data
                var contactResponse = await contactTask;
                if (!contactResponse.IsSuccessStatusCode)
                {
                    var errorResponse = await contactResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching contact: {errorResponse}");
                    return null;
                }

                var rawJson = await contactResponse.Content.ReadAsStringAsync();
                var contactData = JsonConvert.DeserializeObject<dynamic>(rawJson);
                var contact = JsonConvert.DeserializeObject<Contact>(contactData.contact.ToString());

                // Handle tags data
                var tagsResponse = await tagsTask;
                if (tagsResponse.IsSuccessStatusCode)
                {
                    var tagsJson = await tagsResponse.Content.ReadAsStringAsync();
                    var tagsData = JsonConvert.DeserializeObject<dynamic>(tagsJson);
                    contact.Tags = new List<Tag>();

         
                    // Fetch tag names by ID
                    foreach (var tag in tagsData.contactTags)
                    {
                        // Convert tag.id to long
                        if (long.TryParse(tag.id.ToString(), out long tagId))
                        {
                            var tagNameResponse = await _client.GetAsync($"api/3/contactTags/{tagId}/tag");
                            if (tagNameResponse.IsSuccessStatusCode)
                            {
                                var tagJson = await tagNameResponse.Content.ReadAsStringAsync();
                                var tagData = JsonConvert.DeserializeObject<dynamic>(tagJson);

                                // Correctly access the tag name and add it to the contact's tags
                                contact.Tags.Add(new Tag
                                {
                                    Id = tagId,
                                    Name = tagData.tag.tag.ToString()  // Access the 'tag' inside 'tag' object
                                });
                            }
                            else
                            {
                                Console.WriteLine($"Failed to fetch tag name for tag ID {tagId}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid tag ID: {tag.id}");
                        }
                    }

                }

                // Handle custom fields data
                var customFieldsResponse = await customFieldsTask;
                if (customFieldsResponse.IsSuccessStatusCode)
                {
                    var customFieldsJson = await customFieldsResponse.Content.ReadAsStringAsync();
                    var customFieldsData = JsonConvert.DeserializeObject<dynamic>(customFieldsJson);
                    contact.CustomFields = new List<CustomField>();

                    // Fetch custom field titles
                    foreach (var fieldValue in customFieldsData.fieldValues)
                    {
                        // Convert field.id to long
                        if (long.TryParse(fieldValue.field.ToString(), out long fieldId))
                        {
                            var fieldTitleResponse = await _client.GetAsync($"api/3/fields/{fieldId}");
                            if (fieldTitleResponse.IsSuccessStatusCode)
                            {
                                var fieldTitleJson = await fieldTitleResponse.Content.ReadAsStringAsync();
                                var fieldTitleData = JsonConvert.DeserializeObject<dynamic>(fieldTitleJson);
                                contact.CustomFields.Add(new CustomField
                                {
                                    Id = fieldId,
                                    Title = fieldTitleData.field.title,
                                    Value = fieldValue.value.ToString()
                                });
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Invalid field ID: {fieldValue.field}");
                        }
                    }
                }

                return contact;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching contact details: {ex.Message}");
                return null;
            }
        }

        // Fetch contacts modified since a specific date
        public async Task<List<Contact>> GetContactsByModifiedDateAsync(DateTime modifiedAfter)
        {
            var contacts = new List<Contact>();
            try
            {
                string dateFilter = modifiedAfter.ToString("yyyy-MM-ddTHH:mm:ss");
                string nextPageUrl = $"api/3/contacts?filters[modified_since]={dateFilter}";

                do
                {
                    var response = await _client.GetAsync(nextPageUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error fetching contacts: {errorResponse}");
                        break;
                    }

                    var rawJson = await response.Content.ReadAsStringAsync();
                    var contactsData = JsonConvert.DeserializeObject<dynamic>(rawJson);

                    contacts.AddRange(JsonConvert.DeserializeObject<List<Contact>>(contactsData.contacts.ToString()));
                    nextPageUrl = contactsData.meta.next_page?.ToString();
                }
                while (!string.IsNullOrEmpty(nextPageUrl));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching contacts: {ex.Message}");
            }

            return contacts;
        }

        // Search for contacts based on a query
        public async Task<List<Contact>> SearchContactsAsync(string query)
        {
            var contacts = new List<Contact>();
            try
            {
                var response = await _client.GetAsync($"api/3/contacts?search={query}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error searching contacts: {errorResponse}");
                    return contacts;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var contactsData = JsonConvert.DeserializeObject<dynamic>(rawJson);

                contacts.AddRange(JsonConvert.DeserializeObject<List<Contact>>(contactsData.contacts.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while searching contacts: {ex.Message}");
            }

            return contacts;
        }
           
        // Fetch activity for a specific contact
        public async Task<List<Activity>> GetActivityForContactAsync(long contactId, DateTime? after = null, string include = null, bool includeEmails = false)
        {
            var activities = new List<Activity>();
            try
            {
                // Build the query string with optional parameters
                var queryParams = new List<string> { $"contact={contactId}" };

                if (after.HasValue)
                {
                    queryParams.Add($"after={after.Value:yyyy-MM-ddTHH:mm:ssZ}");
                }

                if (!string.IsNullOrEmpty(include))
                {
                    queryParams.Add($"include={include}");
                }

                if (includeEmails)
                {
                    queryParams.Add("emails=true");
                }

                var queryString = string.Join("&", queryParams);
                var requestUrl = $"api/3/activities?{queryString}";

                var response = await _client.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching activities for contact: {errorResponse}");
                    return activities;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var activityData = JsonConvert.DeserializeObject<dynamic>(rawJson);

                foreach (var activity in activityData.activities)
                {
                    activities.Add(new Activity
                    {
                        Id = activity.id,
                        ActivityType = activity.reference_type,
                        Description = activity.reference_action ?? string.Empty,
                        CreatedDate = DateTime.Parse(activity.createdDate.ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching activities: {ex.Message}");
            }

            return activities;
        }

        // Fetch activity for all contacts within a date range
        public async Task<List<Activity>> GetActivityForAllContactsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var activities = new List<Activity>();
            try
            {
                string dateFilter = $"filters[date_created][from]={startDate:yyyy-MM-dd}&filters[date_created][to]={endDate:yyyy-MM-dd}";
                var response = await _client.GetAsync($"api/3/activities?{dateFilter}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error fetching activities for all contacts: {errorResponse}");
                    return activities;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var activityData = JsonConvert.DeserializeObject<dynamic>(rawJson);

                foreach (var activity in activityData.activities)
                {
                    activities.Add(new Activity
                    {
                        Id = activity.id,
                        ActivityType = activity.type,
                        Description = activity.description,
                        CreatedDate = DateTime.Parse(activity.createdDate.ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while fetching activities: {ex.Message}");
            }

            return activities;
        }
                
    }           
}


