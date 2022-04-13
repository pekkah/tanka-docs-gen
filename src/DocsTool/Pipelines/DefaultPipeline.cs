using Microsoft.Extensions.DependencyInjection;

namespace Tanka.DocsTool.Pipelines;

public static class DefaultPipeline
{
    public static PipelineBuilder UseDefault(this PipelineBuilder builder)
    {
        return builder
            .Use<InitializeFileSystems>()
            .Use<AggregateContent>()
            .Use<CollectSections>()
            .Use<BuildSite>()
            .Use<BuildUi>();
    }

    public static IServiceCollection AddDefaultPipeline(this IServiceCollection services)
    {
        services.AddSingleton<InitializeFileSystems>();
        services.AddSingleton<AggregateContent>();
        services.AddSingleton<CollectSections>();
        services.AddSingleton<BuildSite>();
        services.AddSingleton<BuildUi>();

        return services;
    } 
}
