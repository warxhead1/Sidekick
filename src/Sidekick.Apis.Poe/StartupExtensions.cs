using Microsoft.Extensions.DependencyInjection;
using Sidekick.Apis.Poe.Authentication;
using Sidekick.Apis.Poe.Bulk;
using Sidekick.Apis.Poe.Clients;
using Sidekick.Apis.Poe.Clients.Cloudflare;
using Sidekick.Apis.Poe.Clients.Models;
using Sidekick.Apis.Poe.Clients.States;
using Sidekick.Apis.Poe.Leagues;
using Sidekick.Apis.Poe.Localization;
using Sidekick.Apis.Poe.Metadata;
using Sidekick.Apis.Poe.Modifiers;
using Sidekick.Apis.Poe.Parser;
using Sidekick.Apis.Poe.Parser.AdditionalInformation;
using Sidekick.Apis.Poe.Parser.Patterns;
using Sidekick.Apis.Poe.Pseudo;
using Sidekick.Apis.Poe.Stash;
using Sidekick.Apis.Poe.Static;
using Sidekick.Apis.Poe.Trade;
using Sidekick.Common;
using Sidekick.Common.Cloudflare;

namespace Sidekick.Apis.Poe
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSidekickPoeApi(this IServiceCollection services)
        {
            services.AddSingleton<IPoeApiClient, PoeApiClient>();
            services.AddSingleton<IStashService, StashService>();
            services.AddSingleton<IApiStateProvider, ApiStateProvider>();
            services.AddSingleton<PoeApiHandler>();

            services.AddTransient<CloudflareHandler>();

            services.AddHttpClient(ClientNames.TradeClient)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
                .AddHttpMessageHandler<CloudflareHandler>();

            services.AddHttpClient(ClientNames.PoeClient)
                .ConfigurePrimaryHttpMessageHandler((sp) =>
                {
                    var authenticationService = sp.GetRequiredService<IAuthenticationService>();
                    var apiStateProvider = sp.GetRequiredService<IApiStateProvider>();
                    var handler = new PoeApiHandler(authenticationService, apiStateProvider);
                    return handler;
                })
                .AddHttpMessageHandler<CloudflareHandler>();

            services.AddTransient<IPoeTradeClient, PoeTradeClient>();

            services.AddTransient<FilterResources>();
            services.AddTransient<TradeCurrencyResources>();

            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IItemParser, ItemParser>();
            services.AddSingleton<ITradeSearchService, TradeSearchService>();
            services.AddSingleton<ILeagueProvider, LeagueProvider>();
            services.AddSingleton<ITradeFilterService, TradeFilterService>();
            services.AddSingleton<IBulkTradeService, BulkTradeService>();

            return services;
        }
    }
}
