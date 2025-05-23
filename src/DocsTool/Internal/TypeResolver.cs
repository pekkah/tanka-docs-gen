using Spectre.Console.Cli;

namespace Tanka.DocsTool.Internal;

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider) => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object? Resolve(Type? type) // CS8767
    {
        if (type == null)
            return null;

        return _provider.GetService(type); // CS8603 handled by nullable return type
    }

    public void Dispose()
    {
        if (_provider is IDisposable disposable)
            disposable.Dispose();
    }
}
