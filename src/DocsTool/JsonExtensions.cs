using System.Text.Json;

namespace Tanka.DocsTool
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(
            JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
        };

        public static string ToJson(this object? obj)
        {
            var json = JsonSerializer.Serialize(obj, Options);
            return json;
        }
    }
}