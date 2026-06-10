// Döner Empire 3D — Kosmetische Marken-Themen ("Skins")
// Port aus lib/models/branding_model.dart.
//
// Farben als ARGB-uint (wie Flutter Color(0xAARRGGBB)), damit die Logik-Schicht
// UnityEngine-frei bleibt. Die View-Schicht wandelt sie in UnityEngine.Color um.

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public sealed class BrandTheme
    {
        public string Id;
        public string Name;
        public string Emoji;
        public uint Accent;       // 0xAARRGGBB
        public uint AccentDark;   // 0xAARRGGBB
        /// <summary>Achievement-ID, die dieses Thema freischaltet. null = von Anfang an frei.</summary>
        public string UnlockAchievementId;

        public bool Unlocked(ISet<string> achievementIds)
            => UnlockAchievementId == null || achievementIds.Contains(UnlockAchievementId);
    }

    public static class BrandThemeCatalog
    {
        public static readonly IReadOnlyList<BrandTheme> All = new List<BrandTheme>
        {
            new() { Id = "klassik", Name = "Klassik", Emoji = "🥙",
                Accent = 0xFFE8743B, AccentDark = 0xFFC25728 },
            new() { Id = "bronze", Name = "Bronze-Boss", Emoji = "🥉",
                Accent = 0xFFCD7F32, AccentDark = 0xFF9C5F25, UnlockAchievementId = "first_week" },
            new() { Id = "tomate", Name = "Tomaten-Rot", Emoji = "🍅",
                Accent = 0xFFE3433B, AccentDark = 0xFFB12F29, UnlockAchievementId = "three_cities" },
            new() { Id = "gold", Name = "Gold-Standard", Emoji = "👑",
                Accent = 0xFFF2B53C, AccentDark = 0xFFC98F1F, UnlockAchievementId = "cash_250k" },
            new() { Id = "neon", Name = "Neon-Nacht", Emoji = "🌃",
                Accent = 0xFF22D3EE, AccentDark = 0xFF0E7490, UnlockAchievementId = "brand_40" },
            new() { Id = "platin", Name = "Platin-Imperium", Emoji = "💎",
                Accent = 0xFF9AA7C7, AccentDark = 0xFF6B7596, UnlockAchievementId = "twenty_shops" },
            new() { Id = "royal", Name = "Royal-Purpur", Emoji = "🟣",
                Accent = 0xFF8B5CF6, AccentDark = 0xFF6D28D9, UnlockAchievementId = "million_revenue" },
        };

        /// <summary>Thema nach Id; Fallback ist "klassik" (wie Dart).</summary>
        public static BrandTheme ById(string id)
        {
            foreach (var t in All) if (t.Id == id) return t;
            return All[0];
        }
    }
}
