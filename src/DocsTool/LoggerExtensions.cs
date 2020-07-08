using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tanka.DocsTool
{
    public static class LoggerExtensions
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        public static void LogInformationJson<T>(this ILogger logger, string title,  T obj)
        {
            var json = JsonSerializer.Serialize(obj, Options);
            var builder = new StringBuilder();
            builder.AppendLine(title);
            builder.AppendLine(json);
            logger.LogInformation(builder.ToString());
        }
    }
}