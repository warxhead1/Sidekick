using Sidekick.Apis.Poe.Parser;
using Sidekick.Common.Game.Items;
using Sidekick.Common.Initialization;

namespace Sidekick.Apis.Poe
{
    public interface IItemMetadataParser : IInitializableService
    {
        ItemMetadata? Parse(string? name, string? type);

        ItemMetadata? Parse(ParsingItem parsingItem);
    }
}