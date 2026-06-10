// Döner Empire 3D — Spiel-Events / Krisen (Entscheidungen am Tagesende)
// Modell-Port aus lib/models/event_model.dart. Der Event-Katalog selbst liegt
// in Data/EventCatalog.cs (generiert aus der Dart-Quelle).

using System.Collections.Generic;

namespace DoenerEmpire.Models
{
    public enum EventCategory { Good, Bad, Neutral, Opportunity }

    public enum EventWeight { Rare, Normal, Common }

    public sealed class EventRequirements
    {
        public int MinShops = 1;
        public int MinDay = 0;
        public double MinCash = 0;
        public bool NeedsMetropolitanShop = false;
    }

    public sealed class EventEffect
    {
        public double CashDelta;
        public double ReputationDelta;       // wirkt auf ALLE Filialen
        public double BrandAwarenessDelta;   // wirkt auf Marken-Bekanntheit
        public string ResultMessage;
    }

    public sealed class EventChoice
    {
        public string Label;
        public EventEffect Effect;
        public double? Cost;
    }

    public sealed class GameEvent
    {
        public string Id;
        public string Title;
        public string Description;
        public string Emoji;
        public EventCategory Category;
        public List<EventChoice> Choices = new();
        public EventRequirements Requirements = new();
        public EventWeight Weight = EventWeight.Normal;
    }
}
