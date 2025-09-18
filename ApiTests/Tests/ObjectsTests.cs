using ApiClient.Clients;
using ApiTests.Models;
using ApiTests.Setup;
using System.Text.Json;
using Xunit;

namespace ApiTests.Tests
{
    public class ObjectsTests
    {
        [Fact]
        public async System.Threading.Tasks.Task Get_Objects_ReturnsList()
        {
            var client = new HttpApiClient();
            var resp = await client.GetAsync("/objects");
            ReportManager.AddTest(nameof(Get_Objects_ReturnsList), resp.StatusCode == 200 ? "Passed" : $"Failed ({resp.StatusCode})", resp.Raw ?? string.Empty);

            Assert.Equal(200, resp.StatusCode);
            Assert.False(string.IsNullOrEmpty(resp.Raw));

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resp.Raw!);
                var root = doc.RootElement;
                Assert.True(root.ValueKind == System.Text.Json.JsonValueKind.Array, "Expected JSON array");

                var count = 0;
                foreach (var _ in root.EnumerateArray()) count++;
                // Expect at least the 13 items shown in the example
                Assert.True(count >= 13, $"Expected >=13 objects but got {count}");

                // Find the element with id == "7"
                System.Text.Json.JsonElement? elem7 = null;
                foreach (var el in root.EnumerateArray())
                {
                    if (el.TryGetProperty("id", out var idProp))
                    {
                        if (idProp.ValueKind == System.Text.Json.JsonValueKind.String && idProp.GetString() == "7")
                        {
                            elem7 = el;
                            break;
                        }
                    }
                }

                Assert.True(elem7.HasValue, "Could not find object with id '7' in response");

                // Deserialize only the found element into the typed model with relaxed options
                var opts = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                var model = System.Text.Json.JsonSerializer.Deserialize<ApiTests.Models.ObjectModel>(elem7.Value.GetRawText(), opts);
                Assert.NotNull(model);
                Assert.Equal("7", model!.Id);
                Assert.Equal("Apple MacBook Pro 16", model.Name);
                Assert.NotNull(model.Data);
                Assert.Equal(2019, model.Data!.Year);
                Assert.Equal(1849.99, model.Data.Price, 2);
                Assert.Equal("Intel Core i9", model.Data.CpuModel);
                Assert.Equal("1 TB", model.Data.HardDiskSize);
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Xunit.Sdk.XunitException($"Response JSON could not be parsed as array: {ex.Message} -- Raw: {resp.Raw}");
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task Get_Objects_ByIds_ReturnsList()
        {
            var client = new HttpApiClient();
            var query = "/objects?id=3&id=5&id=10";

            var resp = await client.GetAsync(query);
            ReportManager.AddTest(nameof(Get_Objects_ByIds_ReturnsList), resp.StatusCode == 200 ? "Passed" : $"Failed ({resp.StatusCode})", resp.Raw ?? string.Empty);

            Assert.Equal(200, resp.StatusCode);
            Assert.False(string.IsNullOrEmpty(resp.Raw));

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resp.Raw!);
                var root = doc.RootElement;
                Assert.True(root.ValueKind == System.Text.Json.JsonValueKind.Array, "Expected JSON array");

                // Expect exactly 3 items
                var items = root.EnumerateArray().ToArray();
                Assert.Equal(3, items.Length);

                var opts = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                // Validate id=3 (data contains "capacity GB": 512)
                var el3 = items[0];
                Assert.True(el3.TryGetProperty("id", out var id3) && id3.GetString() == "3");
                Assert.True(el3.TryGetProperty("name", out var name3) && name3.GetString() == "Apple iPhone 12 Pro Max");
                Assert.True(el3.TryGetProperty("data", out var data3) && data3.ValueKind == System.Text.Json.JsonValueKind.Object);
                // capacity GB may be numeric or string; handle both
                if (data3.TryGetProperty("capacity GB", out var capProp))
                {
                    double capVal = 0;
                    if (capProp.ValueKind == System.Text.Json.JsonValueKind.Number && capProp.TryGetDouble(out var d)) capVal = d;
                    else if (capProp.ValueKind == System.Text.Json.JsonValueKind.String && double.TryParse(capProp.GetString(), out var d2)) capVal = d2;
                    Assert.Equal(512, (int)capVal);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("Expected 'capacity GB' property in data for id=3");
                }

                // Validate id=5
                var m5 = System.Text.Json.JsonSerializer.Deserialize<ApiTests.Models.ObjectModel>(items[1].GetRawText(), opts);
                Assert.NotNull(m5);
                Assert.Equal("5", m5!.Id);
                Assert.Equal("Samsung Galaxy Z Fold2", m5.Name);
                Assert.NotNull(m5.Data);
                Assert.Equal(689.99, m5.Data.Price, 2);

                // Validate id=10
                var el10 = items[2];
                Assert.True(el10.TryGetProperty("id", out var id10) && id10.GetString() == "10");
                Assert.True(el10.TryGetProperty("name", out var name10) && name10.GetString() == "Apple iPad Mini 5th Gen");
                Assert.True(el10.TryGetProperty("data", out var data10) && data10.ValueKind == System.Text.Json.JsonValueKind.Object);

                if (data10.TryGetProperty("Screen size", out var screenProp))
                {
                    double screenVal = 0;
                    if (screenProp.ValueKind == System.Text.Json.JsonValueKind.Number && screenProp.TryGetDouble(out var sd)) screenVal = sd;
                    else if (screenProp.ValueKind == System.Text.Json.JsonValueKind.String && double.TryParse(screenProp.GetString(), out var sd2)) screenVal = sd2;
                    Assert.Equal(7.9, screenVal, 3);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException("Expected 'Screen size' property in data for id=10");
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Xunit.Sdk.XunitException($"Response JSON could not be parsed as array: {ex.Message} -- Raw: {resp.Raw}");
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task Post_Add_Object_ReturnsCreated()
        {
            var client = new HttpApiClient();
            var bodyJson = ApiTests.Utils.TestData.GetFileContents("testdata/objects/create_object.json");
            var body = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(bodyJson) ?? new System.Collections.Generic.Dictionary<string, object?>();

            var resp = await client.PostJsonAsync("/objects", body);
            ReportManager.AddTest(nameof(Post_Add_Object_ReturnsCreated), resp.StatusCode == 200 ? "Passed" : $"Failed ({resp.StatusCode})", resp.Raw ?? string.Empty);

            Assert.Equal(200, resp.StatusCode);
            Assert.False(string.IsNullOrEmpty(resp.Raw));

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resp.Raw!);
                var root = doc.RootElement;
                Assert.True(root.ValueKind == System.Text.Json.JsonValueKind.Object);

                Assert.True(root.TryGetProperty("id", out var idProp) && !string.IsNullOrEmpty(idProp.GetString()));

                // load expectations from testdata so assertions follow the JSON fixture
                var expected = ApiTests.Utils.TestData.GetFileContentsAs<System.Text.Json.JsonElement>("testdata/objects/create_object.json");
                string? expectedName = null;
                if (expected.TryGetProperty("name", out var nameExp) && nameExp.ValueKind == System.Text.Json.JsonValueKind.String) expectedName = nameExp.GetString();

                Assert.True(root.TryGetProperty("name", out var nameProp) && nameProp.GetString() == expectedName);
                Assert.True(root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == System.Text.Json.JsonValueKind.Object);

                if (expected.TryGetProperty("data", out var ed) && ed.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (ed.TryGetProperty("year", out var yearExp) && yearExp.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        Assert.True(dataProp.TryGetProperty("year", out var yearProp) && yearProp.GetInt32() == yearExp.GetInt32());
                    }
                    if (ed.TryGetProperty("price", out var priceExp))
                    {
                        Assert.True(dataProp.TryGetProperty("price", out var priceProp) && (priceProp.ValueKind == System.Text.Json.JsonValueKind.Number || priceProp.ValueKind == System.Text.Json.JsonValueKind.String));
                    }
                    if (ed.TryGetProperty("CPU model", out var cpuExp) && cpuExp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        Assert.True(dataProp.TryGetProperty("CPU model", out var cpuProp) && cpuProp.GetString() == cpuExp.GetString());
                    }
                    if (ed.TryGetProperty("Hard disk size", out var hdExp) && hdExp.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        Assert.True(dataProp.TryGetProperty("Hard disk size", out var hdProp) && hdProp.GetString() == hdExp.GetString());
                    }
                }

                Assert.True(root.TryGetProperty("createdAt", out var createdAtProp));
                var createdAtStr = createdAtProp.GetString();
                Assert.False(string.IsNullOrEmpty(createdAtStr));
                DateTime parsed;
                Assert.True(DateTime.TryParse(createdAtStr, out parsed));
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Xunit.Sdk.XunitException($"Response JSON could not be parsed as object: {ex.Message} -- Raw: {resp.Raw}");
            }
        }
        
        [Fact]
        public async System.Threading.Tasks.Task Patch_Object_UpdatesName()
        {
            var client = new HttpApiClient();

            // Create a new object first to obtain a generated id (avoid reserved ids like 7)
            var createJson = ApiTests.Utils.TestData.GetFileContents("testdata/objects/create_object.json");
            var createBody = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(createJson) ?? new System.Collections.Generic.Dictionary<string, object?>();
            var createResp = await client.PostJsonAsync("/objects", createBody);
            ReportManager.AddTest(nameof(Patch_Object_UpdatesName) + " - POST create", createResp.StatusCode == 200 ? "Passed" : $"Failed ({createResp.StatusCode})", createResp.Raw ?? string.Empty);
            Assert.Equal(200, createResp.StatusCode);

            string id;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(createResp.Raw!);
                var root = doc.RootElement;
                if (!root.TryGetProperty("id", out var idProp) || string.IsNullOrEmpty(idProp.GetString()))
                {
                    throw new Xunit.Sdk.XunitException($"Create response did not contain id: {createResp.Raw}");
                }

                id = idProp.GetString()!;
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new Xunit.Sdk.XunitException($"Could not parse create response JSON: {ex.Message} -- Raw: {createResp.Raw}");
            }

            var payloadJson = ApiTests.Utils.TestData.GetFileContents("testdata/objects/update_name.json");
            var payload = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(payloadJson) ?? new System.Collections.Generic.Dictionary<string, object?>();
            var expectedName = payload.TryGetValue("name", out var _n) ? _n?.ToString() : null;

            var resp = await client.PatchJsonAsync($"/objects/{id}", payload);
            
            ReportManager.AddTest(nameof(Patch_Object_UpdatesName), resp.StatusCode == 200 ? "Passed" : $"Failed ({resp.StatusCode})", resp.Raw ?? string.Empty);

            Assert.Equal(200, resp.StatusCode);

            ObjectModel? model = null;
            if (!string.IsNullOrEmpty(resp.Raw))
            {
                try
                {
                    model = JsonSerializer.Deserialize<ObjectModel>(resp.Raw);
                }
                catch (JsonException)
                {
                    model = null;
                }
            }

            Assert.NotNull(model);
            Assert.Equal(id, model!.Id);
            Assert.Equal(expectedName, model.Name);
            Assert.NotNull(model.Data);
            Assert.Equal(2023, model.Data!.Year);
            Assert.Equal(1499.99, model.Data.Price, 2);
            Assert.Equal("Intel Core i7", model.Data.CpuModel);
            Assert.Equal("1TB", model.Data.HardDiskSize);
            Assert.NotNull(model.UpdatedAt);
        }
    }
}
