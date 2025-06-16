using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Tanka.DocsTool.Pipelines;

public delegate Task PipelineStep(BuildContext context);

public class PipelineBuilder
{
    private const string ServicesKey = "services";

    private readonly List<Func<PipelineStep, PipelineStep>> _components = new();
    private readonly List<string> _names = new();

    public PipelineBuilder(IServiceProvider serviceProvider) : this()
    {
        SetProperty(ServicesKey, serviceProvider);
    }

    protected PipelineBuilder(PipelineBuilder builder)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(builder.Properties, StringComparer.Ordinal);
    }

    protected PipelineBuilder(IDictionary<string, object?> properties)
    {
        Properties = new CopyOnWriteDictionary<string, object?>(properties, StringComparer.Ordinal);
    }

    private PipelineBuilder()
    {
        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public IServiceProvider ApplicationServices => GetProperty<IServiceProvider>(ServicesKey)!;

    public IDictionary<string, object?> Properties { get; }

    public PipelineBuilder New()
    {
        return new PipelineBuilder(this);
    }

    public PipelineBuilder Use(string name, Func<PipelineStep, PipelineStep> middleware)
    {
        _names.Add(name);
        _components.Add(next => context =>
        {
            if (!string.IsNullOrEmpty(name))
                ApplicationServices.GetRequiredService<IAnsiConsole>().Write(new Rule(name));
            return next(context);
        });
        _components.Add(middleware);
        return this;
    }

    public PipelineStep Build()
    {
        PipelineStep pipeline = _ => Task.CompletedTask;

        for (var c = _components.Count - 1; c >= 0; c--)
            pipeline = _components[c](pipeline);

        return pipeline;
    }

    protected T? GetProperty<T>(string key)
    {
        return Properties.TryGetValue(key, out var value) ? (T?)value : default;
    }

    protected void SetProperty<T>(string key, T value)
    {
        Properties[key] = value;
    }

    public PipelineBuilder Use<T>() where T : IMiddleware
    {
        var middleware = ApplicationServices.GetRequiredService<T>();
        return Use(middleware.Name, next => context => middleware.Invoke(next, context));
    }
}
