using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Sidekick.Common.Cloudflare;
using Sidekick.Common.Settings;

namespace Sidekick.Wpf.Windows.Cloudflare;

public partial class CloudflareWindow : Window, ICloudflareWindow
{
    private readonly ILogger<CloudflareWindow> logger;
    private readonly ISettingsService settingsService;
    private TaskCompletionSource<bool>? challengeCompletion;
    private bool isInitialized;

    public CloudflareWindow(
        ILogger<CloudflareWindow> logger,
        ISettingsService settingsService)
    {
        this.logger = logger;
        this.settingsService = settingsService;
        InitializeComponent();
    }

    public async Task<bool> HandleChallenge(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            if (!isInitialized)
            {
                await InitializeWebView2();
                isInitialized = true;
            }

            challengeCompletion = new TaskCompletionSource<bool>();
            
            // Show window and navigate to challenge page
            Show();
            await webView.EnsureCoreWebView2Async();
            webView.Source = uri;

            // Wait for challenge completion or cancellation
            using var registration = cancellationToken.Register(() => 
            {
                challengeCompletion.TrySetResult(false);
                Dispatcher.Invoke(Hide);
            });

            var result = await challengeCompletion.Task;
            Dispatcher.Invoke(Hide);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CloudflareWindow] Error handling challenge");
            Dispatcher.Invoke(Hide);
            return false;
        }
    }

    private async Task InitializeWebView2()
    {
        try
        {
            await webView.EnsureCoreWebView2Async();
            
            // Handle navigation events to detect challenge completion
            webView.NavigationCompleted += WebView_NavigationCompleted;
            
            // Handle cookie changes by checking cookies after navigation
            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            loadingText.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CloudflareWindow] Error initializing WebView2");
            throw;
        }
    }

    private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        try
        {
            var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.pathofexile.com");
            var cfCookie = cookies.FirstOrDefault(c => c.Name == "cf_clearance");
            
            if (cfCookie != null)
            {
                // Store the Cloudflare cookie
                await settingsService.Set(SettingKeys.CloudflareCookie, cfCookie.Value);
                await settingsService.Set(SettingKeys.CloudflareCookieExpiry, DateTime.UtcNow.AddDays(1));
                
                logger.LogInformation("[CloudflareWindow] Obtained new Cloudflare clearance cookie");
                challengeCompletion?.TrySetResult(true);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CloudflareWindow] Error handling cookie check");
        }
    }

    private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        // Check if we're still on a Cloudflare page or if we've been redirected to the actual content
        var currentUri = webView.Source;
        if (currentUri != null && !currentUri.AbsolutePath.Contains("/cdn-cgi/"))
        {
            logger.LogInformation("[CloudflareWindow] Navigation completed to non-Cloudflare page, challenge likely completed");
            challengeCompletion?.TrySetResult(true);
        }
    }
} 