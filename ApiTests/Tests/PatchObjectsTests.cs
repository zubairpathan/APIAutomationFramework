using ApiClient.Clients;
using ApiTests.Models;
using Xunit;
using ApiTests.Utils;
using ApiTests.Setup;
using System.Net;

namespace ApiTests.Tests
{
    public class PatchObjectsTests
    {
        private readonly HttpApiClient client;
        public PatchObjectsTests()
        {
            client = new HttpApiClient();
        }

        [Fact]
        public async Task UpdatesName_WithValidParams_ShouldSuccess()
        {

            // Create a new object first to obtain a generated id (avoid reserved ids like 7)
            var addObjBody = JsonUtility.ReadJson("testdata/objects/add_object.json");
            var addObjResp = await client.PostJsonAsync("/objects", addObjBody);
            Assert.Equal(HttpStatusCode.OK, addObjResp.StatusCode);
            var addResponseModel = JsonUtility.JsonToObjectModel<AddObjectResponseModel>(addObjResp);

            //Patch call to update name only
            var updateObjBody = JsonUtility.ReadJson("testdata/objects/update_object.json");
            var updateObjResp = await client.PatchJsonAsync($"/objects/{addResponseModel.Id}", updateObjBody);
            Assert.Equal(HttpStatusCode.OK, updateObjResp.StatusCode);
            ReportManager.AddTest(nameof(UpdatesName_WithValidParams_ShouldSuccess), updateObjResp.StatusCode == HttpStatusCode.OK ? "Passed" : $"Failed ({updateObjResp.StatusCode})", updateObjResp.Content ?? string.Empty);
            var patchResponseModel = JsonUtility.JsonToObjectModel<PartialUpdateObjectResponseModel>(updateObjResp);

            Assert.NotNull(patchResponseModel);
            Assert.Equal(addResponseModel.Id, patchResponseModel!.Id);
            Assert.Equal("Updated Test Device", patchResponseModel.Name);
            Assert.NotNull(patchResponseModel.Data);
            Assert.Equal(2023, patchResponseModel.Data!.Year);
            Assert.Equal(1499.99, patchResponseModel.Data.Price, 2);
            Assert.Equal("Intel Core i7", patchResponseModel.Data.CpuModel);
            Assert.Equal("1TB", patchResponseModel.Data.HardDiskSize);
            Assert.NotNull(patchResponseModel.UpdatedAt);
        }
        
        [Fact]
        public async Task UpdatesReservedObject_ShouldReturnMethodNotAllowed()
        {

            // Try to update reserved ids like 7 to get MethodNotAllowed response
            var updateObjBody = JsonUtility.ReadJson("testdata/objects/update_object.json");
            var updateObjResp = await client.PatchJsonAsync($"/objects/7", updateObjBody);
            Assert.Equal(HttpStatusCode.MethodNotAllowed, updateObjResp.StatusCode);
            ReportManager.AddTest(nameof(UpdatesReservedObject_ShouldReturnMethodNotAllowed), updateObjResp.StatusCode == HttpStatusCode.MethodNotAllowed ? "Passed" : $"Failed ({updateObjResp.StatusCode})", updateObjResp.Content ?? string.Empty);
        }
    }
}
