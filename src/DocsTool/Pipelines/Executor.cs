using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Definitions;
using Tanka.DocsTool.Navigation;
using Tanka.FileSystem;
using Tanka.FileSystem.Git;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Path = System.IO.Path;

namespace Tanka.DocsTool.Pipelines
{
    public class Executor
    {
        public Executor(
            SiteDefinition site,
            string currentPath)
        {
            CurrentPath = currentPath;
            Site = site;
            FileSystem = new PhysicalFileSystem(CurrentPath);
            CacheFileSystem = CreateFileSystem(currentPath, site.BuildPath);
            GitRoot = GitFileSystemRoot.Discover(CurrentPath);
        }

        public string CurrentPath { get; }

        public IFileSystem CacheFileSystem { get; }

        public SiteDefinition Site { get; }

        public IFileSystem FileSystem { get; }

        public GitFileSystemRoot GitRoot { get; }

        private static IFileSystem CreateFileSystem(string rootPath, string? inputPath)
        {
            rootPath = GetRootedPath(rootPath, inputPath);
            return new PhysicalFileSystem(rootPath);
        }

        private static string GetRootedPath(string rootPath, string? inputPath)
        {
            if (!string.IsNullOrEmpty(inputPath))
            {
                if (Path.IsPathRooted(inputPath))
                    rootPath = inputPath;
                else
                    rootPath = Path.GetFullPath(inputPath, rootPath);
            }

            return rootPath;
        }

        public async Task Execute(CancellationToken cancellationToken = default)
        {
            var contentCache  = await CacheFileSystem.Mount("content");
            var pageHtmlCache = await CacheFileSystem.Mount("content-html");

            /* Build catalog */
            var aggregator = new ContentAggregator(
                new MimeDbClassifier(),
                FileSystem,
                contentCache,
                GitRoot,
                Site);

            var catalog = new Catalog();
            await catalog.Add(aggregator.Enumerate(), cancellationToken);

            var yamlFiles = catalog
                .GetCollection(ContentTypes.TextYaml);

            // get sections
            var sectionDefinitions = new Dictionary<string, SectionDefinition>();
            foreach (var yamlFile in yamlFiles)
            {
                if (yamlFile.File.Path.GetFileName().ToString().StartsWith("tanka-docs-section"))
                {
                    var sectionDefinition = await yamlFile
                        .ParseYaml<SectionDefinition>();

                    sectionDefinitions.Add(sectionDefinition.Id, sectionDefinition);
                }
            }

            // quickly exit if no index section
            if (!Site.IndexSection.IsXref)
            {
                throw new InvalidOperationException(
                    $"Could not find index section: '{Site.IndexSection}'. " +
                    "Index section must be an xref.");
            }

            var indexSectionId = Site.IndexSection.Xref?.SectionId;

            if (string.IsNullOrEmpty(indexSectionId) || !sectionDefinitions.ContainsKey(indexSectionId))
            {
                throw new InvalidOperationException(
                    $"Could not find index section: '{Site.IndexSection}'. " +
                    "Check your site definition.");
            }
            

        }

    }

    public static class YamlExtensions
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new LinkConverter())
            .Build();

        public static async ValueTask<T> ParseYaml<T>(this ContentItem item)
        {
            await using var stream = await item.File.OpenRead();
            using var reader = new StreamReader(stream);

            return Deserializer.Deserialize<T>(reader);
        }
    }

    internal class LinkConverter: IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(Link) || type == typeof(Link?);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            // should be string
            var value = parser.Consume<Scalar>().Value;

            if (string.IsNullOrEmpty(value))
                return null;

            var link = LinkParser.Parse(value);
            return link;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}