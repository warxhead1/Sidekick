using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Sidekick.Apis.GitHub;
using Sidekick.Apis.Poe;
using Sidekick.Apis.PoePriceInfo;
using Sidekick.Apis.PoeNinja;
using Sidekick.Apis.PoeWiki;
using Sidekick.Common;
using Sidekick.Common.Blazor;
using Sidekick.Common.Database;
using Sidekick.Common.Initialization;
using Sidekick.Common.Platform;
using Sidekick.Common.Platform.Interprocess;
using Sidekick.Common.Settings;
using Sidekick.Common.Ui.Views;
using Sidekick.Mock;
using Sidekick.Modules.Chat;
using Sidekick.Modules.Development;
using Sidekick.Modules.General;
using Sidekick.Modules.Maps;
using Sidekick.Modules.Settings;
using Sidekick.Modules.Trade;
using Sidekick.Modules.Wealth;
using Sidekick.Wpf.Services;

namespace Sidekick.Wpf;

public partial class App : Application
{
    private readonly ILogger<App> logger;
    private readonly ISettingsService settingsService;
    private readonly IInterprocessService interprocessService;

    public App()
    {
        try
        {
            // DeleteStaticAssets();
            ServiceProvider = GetServiceProvider();
            logger = ServiceProvider.GetRequiredService<ILogger<App>>();
            settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
            interprocessService = ServiceProvider.GetRequiredService<IInterprocessService>();
            logger.LogInformation("[App] Successfully initialized services");
        }
        catch (Exception ex)
        {
            File.WriteAllText("startup_error.log", ex.ToString());
            throw;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            logger.LogInformation("[App] Starting application");
            base.OnStartup(e);

            var currentDirectory = Directory.GetCurrentDirectory();
            var settingDirectory = settingsService.GetString(SettingKeys.CurrentDirectory).Result;
            if (string.IsNullOrEmpty(settingDirectory) || settingDirectory != currentDirectory)
            {
                logger.LogDebug("[Startup] Current Directory set to: {0}", currentDirectory);
                settingsService.Set(SettingKeys.CurrentDirectory, currentDirectory).Wait();
                settingsService.Set(SettingKeys.WealthEnabled, false).Wait();
            }

            if (interprocessService is IInitializableService initializableService)
            {
                initializableService.Initialize();
            }

            logger.LogInformation("[App] Opening main window");
            var viewLocator = ServiceProvider.GetRequiredService<IViewLocator>();
            viewLocator.Open("/");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[App] Error during startup");
            File.WriteAllText("startup_error.log", ex.ToString());
            throw;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            base.OnExit(e);
            if (interprocessService is IDisposable disposable)
            {
                disposable.Dispose();
            }
            logger.LogInformation("[App] Application exited");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[App] Error during exit");
            File.WriteAllText("exit_error.log", ex.ToString());
            throw;
        }
    }

    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    private void DeleteStaticAssets()
    {
        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (Directory.Exists(wwwrootPath))
        {
            Directory.Delete(wwwrootPath, true);
        }
    }

    private IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();

        services
            // Common
            .AddSidekickCommon()
            .AddSidekickCommonPlatform(options =>
            {
                options.WindowsIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/favicon.ico");
                options.OsxIconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/apple-touch-icon.png");
            })
            .AddSidekickCommonDatabase()
            .AddSidekickCommonBlazor()

            // Apis
            .AddSidekickGitHubApi()
            .AddSidekickPoeApi()
            .AddSidekickPoeNinjaApi()
            .AddSidekickPoePriceInfoApi()
            .AddSidekickPoeWikiApi()

            // Modules
            .AddSidekickChat()
            .AddSidekickDevelopment()
            .AddSidekickGeneral()
            .AddSidekickMaps()
            .AddSidekickSettings()
            .AddSidekickTrade()
            .AddSidekickWealth()

            // WPF
            .AddSidekickWpf();

        services.AddSingleton<IApplicationService, MockApplicationService>();
        services.AddSingleton<ITrayProvider, WpfTrayProvider>();
        services.AddSingleton<IViewLocator, WpfViewLocator>();
        services.AddSingleton(sp => (WpfViewLocator)sp.GetRequiredService<IViewLocator>());

        services.AddMudServices();

#pragma warning disable CA1416 // Validate platform compatibility
        services.AddWpfBlazorWebView();
        services.AddBlazorWebViewDeveloperTools();
#pragma warning restore CA1416 // Validate platform compatibility

        return services.BuildServiceProvider();
    }
}
