#if NET10_0_OR_GREATER
using Microsoft.Extensions.Configuration;

namespace LowlandTech.TinyTools.Tests.SPEC3315.Server.US02.TemplateApi;

/// <summary>
/// Configuration that a test can change while the host runs, which is how key rotation is proven
/// without a redeploy. The provider reads the dictionary on every lookup.
/// </summary>
public sealed class MutableSettingsSource(IDictionary<string, string?> values) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new MutableSettingsProvider(values);

    private sealed class MutableSettingsProvider(IDictionary<string, string?> values) : ConfigurationProvider
    {
        public override bool TryGet(string key, out string? value) => values.TryGetValue(key, out value);
    }
}
#endif
