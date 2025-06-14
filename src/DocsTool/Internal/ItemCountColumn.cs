using System.Globalization;
using Spectre.Console.Rendering;

namespace Tanka.DocsTool.Internal;

public sealed class ItemCountColumn : ProgressColumn
{
    /// <summary>
    /// Gets or sets the <see cref="CultureInfo"/> to use.
    /// </summary>
    public CultureInfo? Culture { get; set; }

    /// <inheritdoc/>
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var total = task.MaxValue;

        if (task.IsFinished)
        {
            return new Markup(string.Format(
                "[green]{0}[/]",
                total.ToString(Culture)));
        }
        else
        {
            var processedCount = task.Value;

            return new Markup(string.Format(
                "{0}[grey]/[/]{1}",
                processedCount.ToString(Culture),
                total.ToString(Culture)));
        }
    }
}
