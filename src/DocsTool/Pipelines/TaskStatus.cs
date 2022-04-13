using Spectre.Console;

namespace Tanka.DocsTool.Pipelines;

public class TaskStatus
{
    public string Name { get; set; }

    public Status? Status { get; }

    public Progress? Progress { get; set; }
}
