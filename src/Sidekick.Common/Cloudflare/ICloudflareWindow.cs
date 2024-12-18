namespace Sidekick.Common.Cloudflare;

public interface ICloudflareWindow
{
    /// <summary>
    /// Shows the WebView2 window and handles the Cloudflare challenge for the given URI.
    /// </summary>
    /// <param name="uri">The URI that triggered the Cloudflare challenge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the challenge was completed successfully, false otherwise</returns>
    Task<bool> HandleChallenge(Uri uri, CancellationToken cancellationToken);
} 