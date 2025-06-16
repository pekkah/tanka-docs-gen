using System;
using System.IO;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Tanka.DocsTool.Pipelines
{
    public static class YamlExtensions
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new LinkConverter())
            .Build();

        public static async Task<Result<T>> TryParseYaml<T>(this ContentItem item)
        {
            await using var stream = await item.File.OpenRead();
            using var reader = new StreamReader(stream);

            try
            {
                var value = Deserializer.Deserialize<T>(reader);
                return Result.Success(value);
            }
            catch (YamlException e)
            {
                return Result.Failure<T>($"Failed to parse YAML file '{item.File.Path}'. Reason: {e.Message}");
            }
        }

        public static Result<T> TryParseYaml<T>(this string text)
        {
            try
            {
                var value = Deserializer.Deserialize<T>(text);
                return Result.Success(value);
            }
            catch (YamlException e)
            {
                return Result.Failure<T>($"Failed to parse YAML. Reason: {e.Message}");
            }
        }

        internal class LinkConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return type == typeof(Link) || type == typeof(Link?);
            }

            public object? ReadYaml(IParser parser, Type type, ObjectDeserializer nestedObjectDeserializer)
            {
                // should be string
                var value = parser.Consume<Scalar>().Value;

                if (string.IsNullOrEmpty(value))
                    return null;

                var link = LinkParser.Parse(value);
                return link;
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer nestedObjectSerializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}