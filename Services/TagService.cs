using ActiveCampaignAPIWrapper.Authentication;
using ActiveCampaignAPIWrapper.Models;
using ActiveCampaignAPIWrapper.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveCampaignAPIWrapper.Services
{
    public class TagService
    {
        private readonly ActiveCampaignClient _client;

        public TagService(ActiveCampaignClient client)
        {
            _client = client;
        }

        // Create a new tag
        public async Task<ResponseViewModel> CreateTagAsync(Tag tag)
        {
            var responseViewModel = new ResponseViewModel();
            try
            {
                var response = await _client.PostAsJsonAsync("api/3/tags", new
                {
                    tag = new
                    {
                        tag = tag.Name,  // Tag name
                        tagType = "contact",  // Tag type for contacts
                        description = tag.Description  // Optional: Add a description if required
                    }
                });

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    responseViewModel.ret = 0;
                    responseViewModel.responseMessage = $"Error creating tag: {errorResponse}";
                    return responseViewModel;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var createdTag = JsonConvert.DeserializeObject<dynamic>(rawJson);
                tag.Id = long.Parse(createdTag.tag.id.ToString());

                responseViewModel.ret = 1;
                responseViewModel.responseMessage = "Tag created successfully !";
                responseViewModel.Id = tag.Id; // Include the ID here
                return responseViewModel;
            }
            catch (Exception ex)
            {
                responseViewModel.ret = 0;
                responseViewModel.responseMessage = $"An error occurred while creating tag: {ex.Message}";
                return responseViewModel;
            }
        }
                
        // Add tags to a contact
        public async Task<ResponseViewModel> AddTagToContactAsync(long contactId, List<long> tagIds)
        {
            var responseViewModel = new ResponseViewModel();
            try
            {
                // Check if tagIds are empty or null
                if (tagIds == null || !tagIds.Any())
                {
                    responseViewModel.ret = 0;
                    responseViewModel.responseMessage = "No tags provided.";
                    return responseViewModel;
                }

                // Har tag ke liye alag request bhejna
                foreach (var tagId in tagIds)
                {
                    var data = new
                    {
                        contactTag = new
                        {
                            contact = contactId,
                            tag = tagId
                        }
                    };

                    // API ko data bhejna
                    var response = await _client.PostAsJsonAsync("api/3/contactTags", data);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        responseViewModel.ret = 0;
                        responseViewModel.responseMessage = $"Error adding tag {tagId} to contact: {errorResponse}";
                        return responseViewModel;
                    }
                }

                responseViewModel.ret = 1;
                responseViewModel.responseMessage = "Tags successfully added to contact!";
                return responseViewModel;
            }
            catch (Exception ex)
            {
                responseViewModel.ret = 0;
                responseViewModel.responseMessage = $"An error occurred while adding tags to contact: {ex.Message}";
                return responseViewModel;
            }
        }

        // Remove a tag from a contact
        public async Task<ResponseViewModel> RemoveTagFromContactAsync(long contactId, List<long> tagIds)
        {
            var responseViewModel = new ResponseViewModel();
            try
            {
                // Step 1: Get all tags for the contact
                var response = await _client.GetAsync($"api/3/contacts/{contactId}/contactTags");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    responseViewModel.ret = 0;
                    responseViewModel.responseMessage = $"Error fetching tags for contact: {errorResponse}";
                    return responseViewModel;
                }

                // Parse the response to find the correct association IDs for the provided tags
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(content);

                var associationIds = new List<long>();

                foreach (var contactTag in data.contactTags)
                {
                    if (tagIds.Contains((long)contactTag["tag"]))
                    {
                        associationIds.Add((long)contactTag["id"]);
                    }
                }

                if (!associationIds.Any())
                {
                    responseViewModel.ret = 0;
                    responseViewModel.responseMessage = "No associations found between the contact and the provided tags.";
                    return responseViewModel;
                }

                // Step 2: Delete each tag using the association ID
                foreach (var associationId in associationIds)
                {
                    var deleteResponse = await _client.DeleteAsync($"api/3/contactTags/{associationId}");

                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        var deleteErrorResponse = await deleteResponse.Content.ReadAsStringAsync();
                        responseViewModel.ret = 0;
                        responseViewModel.responseMessage = $"Error removing tag from contact: {deleteErrorResponse}";
                        return responseViewModel;
                    }
                }

                responseViewModel.ret = 1;
                responseViewModel.responseMessage = "Tag(s) successfully removed from contact!";
                return responseViewModel;
            }
            catch (Exception ex)
            {
                responseViewModel.ret = 0;
                responseViewModel.responseMessage = $"An error occurred while removing the tags: {ex.Message}";
                return responseViewModel;
            }
        }

    }

}
