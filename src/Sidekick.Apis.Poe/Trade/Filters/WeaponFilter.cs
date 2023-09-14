using System.Text.Json.Serialization;

namespace Sidekick.Apis.Poe.Trade.Filters
{
    internal class WeaponFilter
    {
        public SearchFilterValue? Damage { get; set; }

        [JsonPropertyName("crit")]
        public SearchFilterValue? CriticalStrikeChance { get; set; }

        [JsonPropertyName("aps")]
        public SearchFilterValue? AttacksPerSecond { get; set; }

        [JsonPropertyName("dps")]
        public SearchFilterValue? DamagePerSecond { get; set; }

        [JsonPropertyName("edps")]
        public SearchFilterValue? ElementalDps { get; set; }

        [JsonPropertyName("pdps")]
        public SearchFilterValue? PhysicalDps { get; set; }
    }
}
