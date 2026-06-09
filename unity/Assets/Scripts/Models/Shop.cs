// Döner Empire 3D — Filiale (zentrales Stateful-Modell)
// Port aus lib/models/shop_model.dart (Shop).
// Als POCO mit öffentlichen Feldern (Serializer-freundlich). Die Flutter-
// copyWith-Semantik wird vom Spiel-Controller/Engine erzeugt; hier reicht
// Clone() + abgeleitete Getter.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Core;

namespace DoenerEmpire.Models
{
    public sealed class Shop
    {
        public string Id;
        public string Name;               // Konzern-/Kettenname
        public string CustomName;         // optional
        public string CityId;
        public string LocationName;
        public int FootTraffic;
        public double WeeklyRent;
        public bool IsOpen = true;
        public List<ShopProduct> Menu = new();
        public List<ShopEquipment> Equipment = new();
        public List<Employee> Employees = new();
        public double Reputation = 3.0;   // 0..5
        public int DayOpened;
        public List<ActiveCampaign> ActiveCampaigns = new();
        public LocationPersonality Personality = LocationPersonality.Touristic;
        public List<string> UpgradeIds = new();
        public bool AutoHire = false;
        public string OriginalCompetitorName;
        public bool WasAcquired = false;
        public double Morale = 0.75;       // 0.2..1.0
        public double Regulars = 0.0;      // 0..0.5
        public ShopSizeTier SizeTier = ShopSizeTier.Klein;

        // ── abgeleitete Werte ────────────────────────────────────────────────
        public double DailyRent => WeeklyRent / 7.0;

        public bool HasCustomName => !string.IsNullOrWhiteSpace(CustomName);

        /// Legacy-Alias: 0..3 entspricht klein..flagship.
        public int ExpansionLevel => (int)SizeTier;

        public string DisplayName => HasCustomName ? $"{Name} - {CustomName.Trim()}" : Name;

        public string BrandingHint => HasCustomName ? Name : LocationName;

        public string AcquiredHint =>
            WasAcquired && OriginalCompetitorName != null ? $"ehemals {OriginalCompetitorName}" : null;

        public bool HasUpgrade(string upgradeId) => UpgradeIds.Contains(upgradeId);

        public bool HasEquipment(string equipmentId) =>
            Equipment.Any(e => e.EquipmentId == equipmentId);

        /// <summary>Tageszeit-Profil dieser Filiale (basierend auf Personality).</summary>
        public TimeProfile TimeProfile => TimeProfiles.For(Personality);

        public Shop Clone() => new()
        {
            Id = Id, Name = Name, CustomName = CustomName, CityId = CityId,
            LocationName = LocationName, FootTraffic = FootTraffic, WeeklyRent = WeeklyRent,
            IsOpen = IsOpen,
            Menu = Menu.Select(p => p.Clone()).ToList(),
            Equipment = Equipment.Select(e => e.Clone()).ToList(),
            Employees = new List<Employee>(Employees),
            Reputation = Reputation, DayOpened = DayOpened,
            ActiveCampaigns = ActiveCampaigns.Select(c => c.Clone()).ToList(),
            Personality = Personality,
            UpgradeIds = new List<string>(UpgradeIds),
            AutoHire = AutoHire, OriginalCompetitorName = OriginalCompetitorName,
            WasAcquired = WasAcquired, Morale = Morale, Regulars = Regulars,
            SizeTier = SizeTier,
        };
    }
}
