using System.Text;
using HandlebarsDotNet;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using FileSystemPath = Tanka.FileSystem.FileSystemPath;

namespace Tanka.DocsTool.UI
{
    public class HandlebarsUiBundle : IUiBundle
    {
        public static FileSystemPath PartialsPath = "partials";

        private readonly Dictionary<string, string> _templates =
            new Dictionary<string, string>();

        private readonly Dictionary<string, string> _partials =
            new Dictionary<string, string>();

        private readonly Site _site;
        private readonly Section _uiBundle;
        private readonly IFileSystem _output;

        public HandlebarsUiBundle(Site site, Section uiBundle, IFileSystem output)
        {
            _site = site ?? throw new ArgumentNullException(nameof(site));
            _uiBundle = uiBundle ?? throw new ArgumentNullException(nameof(uiBundle));
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            foreach (var (path, contentItem) in _uiBundle.GetContentItems("**/*.hbs"))
            {
                await using var input = await contentItem.File.OpenRead();
                using var reader = new StreamReader(input, Encoding.UTF8);

                var template = await reader.ReadToEndAsync();

                // is partial or view?
                if (path.StartsWith(PartialsPath))
                    _partials[path] = template;
                else
                    _templates[path] = template;
            }
        }

        public async Task PrepareAssets(DocsSiteRouter router)
        {
            foreach (var (path, contentItem) in _uiBundle.GetContentItems(new FileSystemPath[]
            {
                "**/*.js",
                "**/*.css",
                "**/*.png",
                "**/*.jpg",
                "**/*.gif"
            }))
            {
                var xref = new Xref(_uiBundle.Version, _uiBundle.Id, path);
                FileSystemPath route = router.GenerateRoute(xref)
                    ?? throw new InvalidOperationException($"Could not generate route for '{xref}'.");

                await using var inputStream = await contentItem.File.OpenRead();

                await _output.GetOrCreateDirectory(route.GetDirectoryPath());
                var outputFile = await _output.GetOrCreateFile(route);
                var outputStream = await outputFile.OpenWrite();

                await inputStream.CopyToAsync(outputStream);
            }
        }

        public IPageRenderer GetPageRenderer(string template, DocsSiteRouter router)
        {
            if (string.IsNullOrEmpty(template))
                template = DefaultTemplate;

            var templateHbs = _templates[template];

            return new HandlebarsPageRenderer(templateHbs, _partials, router);
        }

        public string DefaultTemplate { get; } = "article.hbs";
    }

    internal class HandlebarsPageRenderer : IPageRenderer
    {
        private readonly string _templateHbs;
        private readonly IHandlebars _handlebars;

        public HandlebarsPageRenderer(string templateHbs, IReadOnlyDictionary<string, string> partials, DocsSiteRouter router)
        {
            _templateHbs = templateHbs;
            _handlebars = Handlebars.Create();
            _handlebars.RegisterHelper("link_to", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                    throw new InvalidOperationException($"link_to requires one argument of type DisplayLink");

                var target = arguments[0];

                string? url = null;
                string? title = null;
                if (target is DisplayLink displayLink)
                {
                    title = displayLink.Title ?? "";
                    url = displayLink.Link.Xref != null
                        ? router.GenerateRoute(displayLink.Link.Xref.Value)
                        : displayLink.Link.Uri;
                }

                if (url == null)
                    url = "[TODO: MISSING LINK TARGET]";

                options.Template(in output, new
                {
                    title,
                    url
                });
            });

            _handlebars.RegisterHelper("xref", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                    throw new InvalidOperationException($"xref requires one argument of type xref");

                var target = arguments[0];

                string? url = null;

                if (target is string xrefStr)
                {
                    target = LinkParser.Parse(xrefStr);
                }

                if (target is Link link)
                {
                    if (link.IsExternal)
                        url = link.Uri;
                    else
                        target = link.Xref!.Value;

                }

                if (target is Xref xref)
                {
                    url = router.GenerateRoute(xref);
                }

                if (url == null)
                    url = "[TODO: MISSING LINK TARGET]";

                options.Template(in output, new
                {
                    url
                });
            });

            _handlebars.RegisterHelper("section", (output, options, context, arguments) =>
            {
                if (arguments.Length != 2)
                    throw new InvalidOperationException($"Section requires version and id. " +
                                                        $"Example: section \"head\" \"root\"");

                var version = arguments[0]?.ToString()
                              ?? throw new ArgumentNullException("arguments[0]");

                var id = arguments[1]?.ToString()
                         ?? throw new ArgumentNullException("arguments[1]");

                var section = router.Site.GetSection(version, id);

                if (section == null)
                    throw new InvalidOperationException(
                        $"Could not find section '{id}@{version}'");

                options.Template(in output, new
                {
                    Id = section.Id,
                    Title = section.Title,
                    Type = section.Type,
                    Version = section.Version,
                    IndexPage = section.IndexPage
                });
            });

            _handlebars.RegisterHelper("sections", (output, options, context, arguments) =>
            {
                if (arguments.Length != 1)
                    throw new InvalidOperationException($"Sections requires version. " +
                                                        $"Example: sections \"head\"");

                var version = arguments[0]?.ToString()
                              ?? throw new ArgumentNullException("arguments[0]");

                var sections = router.Site.GetSectionsByVersion(version)
                    .Where(s => s.Type == "doc")
                    .OrderBy(s => s.Path.ToString());

                foreach (var section in sections)
                {
                    options.Template(in output, new
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Type = section.Type,
                        Version = section.Version,
                        IndexPage = section.IndexPage
                    });
                }
            });

            _handlebars.RegisterHelper("versions", (output, options, context, arguments) =>
            {
                var versions = router.Site.Versions;

                foreach (var version in versions)
                {
                    options.Template(in output, new
                    {
                        Version = version,
                        Sections = router.Site.GetSectionsByVersion(version)
                            .Where(s => s.Type == "doc")
                            .Select(section => new
                            {
                                Id = section.Id,
                                Title = section.Title,
                                Type = section.Type,
                                Version = section.Version,
                                IndexPage = section.IndexPage

                            })
                            .ToList()
                    });
                }
            });

            foreach (var (name, partialTemplate) in partials)
            {
                _handlebars.RegisterTemplate(name, partialTemplate);
            }
        }

        public string Render(PageRenderingContext context)
        {
            var compiledTemplate = _handlebars.Compile(_templateHbs);
            return compiledTemplate(context);
        }
    }
}