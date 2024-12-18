using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Sidekick.Apis.Poe.Clients.Models;
using Sidekick.Common.Game;
using Sidekick.Common.Game.Languages;

namespace Sidekick.Apis.Poe.Clients
{
    public class PoeTradeClient : IPoeTradeClient
    {
        private readonly ILogger logger;

        public PoeTradeClient(
            ILogger<PoeTradeClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            HttpClient = httpClientFactory.CreateClient(ClientNames.TradeClient);
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Powered-By", "Sidekick");
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Sidekick");

            Options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        public JsonSerializerOptions Options { get; }

        public HttpClient HttpClient { get; }

        public async Task<FetchResult<TReturn>> Fetch<TReturn>(GameType game, IGameLanguage language, string path)
        {
            var name = typeof(TReturn).Name;
            var url = language.GetTradeApiBaseUrl(game) + path;
            
            logger.LogInformation($"[Trade Client] Starting fetch for {name} at {url}");

            try
            {
                logger.LogDebug($"[Trade Client] Sending GET request to {url}");
                var response = await HttpClient.GetAsync(url);
                logger.LogDebug($"[Trade Client] Received response with status code: {response.StatusCode}");

                var content = await response.Content.ReadAsStreamAsync();
                logger.LogDebug($"[Trade Client] Successfully read response content");

                var result = await JsonSerializer.DeserializeAsync<FetchResult<TReturn>>(content, Options);
                if (result != null)
                {
                    logger.LogInformation($"[Trade Client] Successfully fetched and deserialized {name}");
                    return result;
                }

                logger.LogWarning($"[Trade Client] Deserialization resulted in null for {name}");
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, $"[Trade Client] HTTP request failed for {name} at {url}");
                throw;
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, $"[Trade Client] JSON deserialization failed for {name}");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[Trade Client] Unexpected error fetching {name} at {url}");
                throw;
            }

            var error = $"[Trade Client] Could not understand the API response for {name}";
            logger.LogError(error);
            throw new Exception(error);
        }
    }
}
