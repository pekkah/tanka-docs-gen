using Spectre.Console;

namespace Tanka.DocsTool.Pipelines;

public class TaskStatus
{
    public string? Name { get; set; } // CS8618

    public Status? Status { get; }

    public Progress? Progress { get; set; }
}
