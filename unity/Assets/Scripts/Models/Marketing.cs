// Döner Empire 3D — Marketing
// Port aus lib/models/marketing_model.dart (MarketingCampaign, ActiveCampaign).
// MarketingScope/MarketingRisk liegen in Core/Enums.cs.

using DoenerEmpire.Core;

namespace DoenerEmpire.Models
{
    public sealed class MarketingCampaign
    {
        public string Id;
        public string Name;
        public string Description;
        public string Emoji;
        public double Cost;
        public int DurationDays;            // 0 = einmalig/permanent
        public double CustomerBoost;        // z.B. 0.20 = +20%
        public double ReputationBoostPerDay;
        public double ReputationBoostOnce;
        public double AvgOrderValueMod;     // z.B. -0.30 = 2-für-1
        public double ViralChance;          // 0..1
        public MarketingScope Scope;
        public MarketingRisk Risk = MarketingRisk.Low;
        public double BrandAwarenessDelta;

        public bool HasDuration => DurationDays > 0;
        public double CostPerDay => DurationDays > 0 ? Cost / DurationDays : Cost;
    }

    /// <summary>Eine laufende Kampagne (Instanz im Shop).</summary>
    public sealed class ActiveCampaign
    {
        public string CampaignId;
        public int StartDay;
        public int EndDay;   // exclusive

        public bool IsActive(int currentDay) => currentDay >= StartDay && currentDay < EndDay;

        public int RemainingDays(int currentDay)
        {
            var v = EndDay - currentDay;
            return v < 0 ? 0 : (v > 999 ? 999 : v);
        }

        public double Progress(int currentDay)
        {
            var total = EndDay - StartDay;
            if (total <= 0) return 1.0;
            var elapsed = currentDay - StartDay;
            if (elapsed < 0) elapsed = 0;
            if (elapsed > total) elapsed = total;
            var p = (double)elapsed / total;
            return p < 0.0 ? 0.0 : (p > 1.0 ? 1.0 : p);
        }

        public ActiveCampaign Clone() => new()
        {
            CampaignId = CampaignId, StartDay = StartDay, EndDay = EndDay,
        };
    }
}
