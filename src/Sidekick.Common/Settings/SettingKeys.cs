namespace Sidekick.Common.Settings;

public static class SettingKeys
{
    public const string Version = nameof(Version);

    public const string CurrentDirectory = nameof(CurrentDirectory);

    public const string BearerToken = nameof(BearerToken);
    public const string BearerExpiration = nameof(BearerExpiration);

    // Cloudflare settings
    public const string CloudflareCookie = nameof(CloudflareCookie);
    public const string CloudflareCookieExpiry = nameof(CloudflareCookieExpiry);

    public const string LanguageParser = nameof(LanguageParser);
    public const string LanguageUi = nameof(LanguageUi);
    public const string LeagueId = nameof(LeagueId);
    public const string LeaguesHash = nameof(LeaguesHash);

    public const string KeyClose = nameof(KeyClose);
    public const string KeyFindItems = nameof(KeyFindItems);
    public const string KeyOpenMapCheck = nameof(KeyOpenMapCheck);
    public const string KeyOpenPriceCheck = nameof(KeyOpenPriceCheck);
    public const string KeyOpenWealth = nameof(KeyOpenWealth);
    public const string KeyOpenWiki = nameof(KeyOpenWiki);

    public const string EscapeClosesOverlays = nameof(EscapeClosesOverlays);
    public const string RetainClipboard = nameof(RetainClipboard);
    public const string ChatCommands = nameof(ChatCommands);
    public const string PreferredWiki = nameof(PreferredWiki);
    public const string PoeNinjaLastClear = nameof(PoeNinjaLastClear);

    public const string MapCheckCloseWithMouse = nameof(MapCheckCloseWithMouse);
    public const string MapCheckDangerousRegex = nameof(MapCheckDangerousRegex);

    public const string PriceCheckCloseWithMouse = nameof(PriceCheckCloseWithMouse);
    public const string PriceCheckPredictionEnabled = nameof(PriceCheckPredictionEnabled);
    public const string PriceCheckItemCurrency = nameof(PriceCheckItemCurrency);
    public const string PriceCheckBulkCurrency = nameof(PriceCheckBulkCurrency);
    public const string PriceCheckBulkMinimumStock = nameof(PriceCheckBulkMinimumStock);
    public const string PriceCheckCurrencyMode = nameof(PriceCheckCurrencyMode);

    public const string WealthEnabled = nameof(WealthEnabled);
    public const string WealthTrackedTabs = nameof(WealthTrackedTabs);
    public const string WealthItemTotalMinimum = nameof(WealthItemTotalMinimum);

    public const string SavedFilterValues = nameof(SavedFilterValues);
}
