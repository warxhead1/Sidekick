using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Sidekick.Common.Cloudflare;
using Sidekick.Common.Settings;

namespace Sidekick.Apis.Poe.Clients.Cloudflare;

public class CloudflareHandler : DelegatingHandler
{
    private readonly ILogger<CloudflareHandler> logger;
    private readonly ISettingsService settingsService;
    private readonly ICloudflareWindow cloudflareWindow;
    private readonly SemaphoreSlim challengeSemaphore = new(1, 1);
    private bool isHandlingChallenge;

    public CloudflareHandler(
        ILogger<CloudflareHandler> logger,
        ISettingsService settingsService,
        ICloudflareWindow cloudflareWindow)
    {
        this.logger = logger;
        this.settingsService = settingsService;
        this.cloudflareWindow = cloudflareWindow;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // First try with existing cookies
        var response = await base.SendAsync(request, cancellationToken);

        // If we get a 403 and it's not already handling a challenge, we might need to solve one
        if (response.StatusCode == HttpStatusCode.Forbidden && !isHandlingChallenge)
        {
            logger.LogInformation("[CloudflareHandler] Received 403 response, attempting to handle Cloudflare challenge");
            
            try
            {
                await challengeSemaphore.WaitAsync(cancellationToken);
                isHandlingChallenge = true;

                // Show WebView2 window and wait for challenge completion
                var success = await cloudflareWindow.HandleChallenge(request.RequestUri!, cancellationToken);
                if (!success)
                {
                    logger.LogWarning("[CloudflareHandler] Failed to complete Cloudflare challenge");
                    return response;
                }

                // Retry the request with new cookies
                var retryResponse = await base.SendAsync(request, cancellationToken);
                if (retryResponse.IsSuccessStatusCode)
                {
                    logger.LogInformation("[CloudflareHandler] Successfully completed Cloudflare challenge and retried request");
                    return retryResponse;
                }
                
                logger.LogWarning("[CloudflareHandler] Request still failed after completing Cloudflare challenge: {StatusCode}", retryResponse.StatusCode);
                return retryResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[CloudflareHandler] Error handling Cloudflare challenge");
                return response;
            }
            finally
            {
                isHandlingChallenge = false;
                challengeSemaphore.Release();
            }
        }

        return response;
    }
} 