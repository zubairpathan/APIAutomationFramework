using System.Text.Json;
using RestSharp;

namespace ApiTests.Utils
{
    public class JsonUtility
    {
        public static object ReadJson(string fileName)
        {
            var createJson = TestData.GetFileContents(fileName);
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(createJson) ?? [];
        }
        public static T JsonToObjectModel<T>(RestResponse resp)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(resp.Content);
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(dict))!;
        }
    }
}