using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Vertical.SpectreLogger;

var console = AnsiConsole.Create(new AnsiConsoleSettings());
console.Write(
    new FigletText("Tanka Docs")
        .LeftJustified()
        .Color(Color.Green)
        );

var services = new ServiceCollection();
services.AddSingleton<IAnsiConsole>(console);
services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddSpectreConsole();
});

// steps 
services.AddDefaultPipeline();

var provider = services.BuildServiceProvider();
//todo: clean this
Infra.Initialize(provider.GetRequiredService<ILoggerFactory>());

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);
app.Configure(config =>
{
#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif

    config.AddCommand<BuildSiteCommand>("build");
    config.AddCommand<DevCommand>("dev");
});

await app.RunAsync(args);
