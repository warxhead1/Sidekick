using Microsoft.Extensions.DependencyInjection;
using Sidekick.Common.Cloudflare;
using Sidekick.Wpf.Windows.Cloudflare;

namespace Sidekick.Wpf;

public static class StartupExtensions
{
    public static IServiceCollection AddSidekickWpf(this IServiceCollection services)
    {
        services.AddTransient<ICloudflareWindow, CloudflareWindow>();
        return services;
    }
} 