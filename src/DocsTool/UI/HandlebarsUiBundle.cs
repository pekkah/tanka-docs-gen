using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Tanka.DocsTool.Catalogs;
using Tanka.DocsTool.Navigation;
using Tanka.DocsTool.Pipelines;
using Tanka.FileSystem;
using Path = Tanka.FileSystem.Path;

namespace Tanka.DocsTool.UI
{
    public class HandlebarsUiBundle : IUiBundle
    {
        public static Path PartialsPath = "partials";

        private readonly Dictionary<string, string> _templates =
            new Dictionary<string, string>();

        private readonly Dictionary<string, string> _partials =
            new Dictionary<string, string>();

        private readonly IFileSystem _uiBundle;

        public HandlebarsUiBundle(IFileSystem uiBundle)
        {
            _uiBundle = uiBundle;
        }


        public async Task Initialize(CancellationToken cancellationToken)
        {
            var aggregator = new FileSystemAggregator(_uiBundle);

            await foreach (var file in aggregator.Aggregate("", new Path[]{ "**/*.hbs" }, cancellationToken))
            {
                await using var input = await file.OpenRead();
                using var reader = new StreamReader(input);

                var template = await reader.ReadToEndAsync();
                var name = file.Path.ToString();

                // is partial or view?
                if (file.Path.StartsWith(PartialsPath))
                    _partials[name] = template;
                else
                    _templates[name] = template;
            }
        }

        public IPageRenderer GetRenderer(string template, DocsSiteRouter router)
        {
            var templateHbs = _templates[template];

            return new HandlebarsPageRenderer(templateHbs, _partials, router);
        }

        public string DefaultTemplate { get; } = "article.hbs";
    }

    internal class HandlebarsPageRenderer : IPageRenderer
    {
        private readonly string _templateHbs;
        private IHandlebars _handlebars;

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

                options.Template(output, new
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

                options.Template(output, new
                {
                    url
                });
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