// Döner Empire 3D — Teilbare Imperium-Zusammenfassung
// Port aus lib/services/share_util.dart (empireSummaryText).

using System.Globalization;
using System.Linq;
using System.Text;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class ShareService
    {
        private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");

        /// <summary>Erzeugt eine teilbare Text-Zusammenfassung des Imperiums.</summary>
        public static string EmpireSummaryText(GameState state)
        {
            var avgRep = state.Shops.Count == 0
                ? 0.0
                : state.Shops.Sum(s => s.Reputation) / state.Shops.Count;
            var chapters = CampaignEngine.CompletedCount(state);

            var sb = new StringBuilder();
            sb.Append("🥙 ").Append(state.CompanyName).Append('\n');
            sb.Append("Tag ").Append(state.CurrentDay).Append(" · ")
              .Append(Fmt(state.Cash)).Append(" € Kasse").Append('\n');
            sb.Append("🏪 ").Append(state.ShopCount).Append(" Filialen · 👥 ")
              .Append(state.EmployeeCount).Append(" Mitarbeiter").Append('\n');
            sb.Append("📢 Marke ").Append(state.Brand.BrandAwareness.ToString("0", De))
              .Append("/100 · ⭐ Ø ").Append(avgRep.ToString("0.0", De)).Append('\n');
            sb.Append("💰 Gesamtumsatz ").Append(Fmt(state.TotalRevenue)).Append(" €").Append('\n');
            sb.Append("🏆 ").Append(state.AchievementIds.Count).Append('/')
              .Append(AchievementCatalog.All.Count).Append(" Trophäen · 📖 Kapitel ")
              .Append(chapters).Append('/').Append(CampaignData.Chapters.Count).Append('\n');
            sb.Append("#DönerEmpire");
            return sb.ToString();
        }

        private static string Fmt(double value) => value.ToString("#,##0", De);
    }
}
