using System.IO;
using System.Text.Json;

namespace Sidekick.Core.Configuration
{
    public class Configuration
    {
        public const string FileName = "appsettings.json";

        private static readonly Configuration Defaults = new Configuration()
        {
            UILanguage = "English",
            LeagueId = string.Empty,
            CharacterName = string.Empty,
            CurrentWikiSettings = WikiSetting.PoeWiki,
            RetainClipboard = true,
            KeyCloseWindow = "Space",
            KeyPriceCheck = "Ctrl+D",
            KeyHideout = "F5",
            KeyItemWiki = "Alt+W",
            KeyFindItems = "Ctrl+F",
            KeyLeaveParty = "F4",
            KeyOpenSearch = "Alt+Q",
            KeyOpenLeagueOverview = "F6",
        };

        public string UILanguage { get; set; } = Defaults.UILanguage;

        public string LeagueId { get; set; } = Defaults.LeagueId;

        public WikiSetting CurrentWikiSettings { get; set; } = Defaults.CurrentWikiSettings;

        public string CharacterName { get; set; } = Defaults.CharacterName;

        public bool RetainClipboard { get; set; } = Defaults.RetainClipboard;

        public string KeyCloseWindow { get; set; } = Defaults.KeyCloseWindow;

        public string KeyPriceCheck { get; set; } = Defaults.KeyPriceCheck;

        public string KeyHideout { get; set; } = Defaults.KeyHideout;

        public string KeyItemWiki { get; set; } = Defaults.KeyItemWiki;

        public string KeyFindItems { get; set; } = Defaults.KeyFindItems;

        public string KeyLeaveParty { get; set; } = Defaults.KeyLeaveParty;

        public string KeyOpenSearch { get; set; } = Defaults.KeyOpenSearch;

        public string KeyOpenLeagueOverview { get; set; } = Defaults.KeyOpenLeagueOverview;

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            var defaults = JsonSerializer.Serialize(Defaults);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

            // Backup old settings
            if (File.Exists(filePath))
            {
                File.Copy(filePath, filePath.Replace(".json", "_old.json"), true);
            }

            // TODO: Refactor this to use the new using syntax in Csharp 8
            using (var fileStream = File.Create(filePath))
            {
                using (var writer = new Utf8JsonWriter(fileStream, options: new JsonWriterOptions
                {
                    Indented = true
                }))
                {
                    using (var document = JsonDocument.Parse(json, new JsonDocumentOptions
                    {
                        CommentHandling = JsonCommentHandling.Skip
                    }))
                    {
                        using (var defaultsDocument = JsonDocument.Parse(defaults, new JsonDocumentOptions
                        {
                            CommentHandling = JsonCommentHandling.Skip
                        }))
                        {
                            var root = document.RootElement;
                            var defaultsRoot = defaultsDocument.RootElement;

                            if (root.ValueKind == JsonValueKind.Object)
                            {
                                writer.WriteStartObject();
                            }
                            else
                            {
                                return;
                            }

                            foreach (var property in root.EnumerateObject())
                            {
                                if (defaultsRoot.GetProperty(property.Name).Equals(property.Value))
                                {
                                    continue;
                                }

                                property.WriteTo(writer);
                            }

                            writer.WriteEndObject();
                            writer.Flush();
                        }
                    }
                }
            }
        }
    }
}
