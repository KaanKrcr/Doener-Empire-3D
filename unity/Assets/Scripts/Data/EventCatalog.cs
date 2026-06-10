// Döner Empire 3D — Event-Katalog (Krisen/Chancen-Pool)
// AUTO-GENERIERT aus lib/models/event_model.dart via tools/gen_event_catalog.py.
// Nicht von Hand editieren — bei Änderungen am Dart-Katalog neu generieren.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Data
{
    public static class EventCatalog
    {
        public static readonly IReadOnlyList<GameEvent> All = new List<GameEvent>
        {
            new GameEvent
            {
                Id = "hygiene_inspection",
                Title = "Lebensmittelkontrolle!",
                Description = "Ein Lebensmittelkontrolleur steht plötzlich vor der Tür einer deiner Filialen. Was tust du?",
                Emoji = "🔍",
                Category = EventCategory.Bad,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Ordnungsgemäß empfangen",
                        Effect = new EventEffect
                        {
                            CashDelta = -300,
                            ReputationDelta = 0.1,
                            ResultMessage = "Kleinere Mängel — 300 € Bußgeld, aber die Kontrolle lobt die Sauberkeit.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "500 € \"Trinkgeld\" anbieten",
                        Effect = new EventEffect
                        {
                            CashDelta = -500,
                            ReputationDelta = -0.5,
                            BrandAwarenessDelta = -2.0,
                            ResultMessage = "Bestechung angenommen — aber das Gerücht macht die Runde. Reputation und Marke leiden.",
                        },
                        Cost = 500,
                    },
                    new EventChoice
                    {
                        Label = "Kontrolleur ignorieren",
                        Effect = new EventEffect
                        {
                            CashDelta = -2000,
                            ReputationDelta = -0.8,
                            ResultMessage = "Geschäft musste schließen für einen Tag. 2.000 € Strafe, schwerer Reputationsverlust.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "employee_sick",
                Title = "Mitarbeiter krank gemeldet",
                Description = "Einer deiner Mitarbeiter ist krank. Wie reagierst du?",
                Emoji = "🤒",
                Category = EventCategory.Bad,
                Weight = EventWeight.Common,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Krankschreibung akzeptieren",
                        Effect = new EventEffect
                        {
                            CashDelta = -150,
                            ReputationDelta = 0.05,
                            ResultMessage = "Lohnfortzahlung kostet, aber das Team weiß es zu schätzen.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Aushilfe für 1 Tag mieten (400 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -400,
                            ResultMessage = "Aushilfe gefunden — Tagesbetrieb läuft ohne Einbußen.",
                        },
                        Cost = 400,
                    },
                    new EventChoice
                    {
                        Label = "Selbst einspringen",
                        Effect = new EventEffect
                        {
                            CashDelta = 100,
                            ReputationDelta = -0.05,
                            ResultMessage = "Du sparst Geld, aber bist erschöpft — Service leidet leicht.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "supplier_problem",
                Title = "Lieferant streikt",
                Description = "Dein Fleisch-Lieferant kann heute nicht liefern. Was tust du?",
                Emoji = "🚛",
                Category = EventCategory.Bad,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Notlieferant beauftragen (+50% Kosten)",
                        Effect = new EventEffect
                        {
                            CashDelta = -800,
                            ResultMessage = "Teurer Notdeal, aber der Laden läuft weiter.",
                        },
                        Cost = 800,
                    },
                    new EventChoice
                    {
                        Label = "Mit weniger Auswahl arbeiten",
                        Effect = new EventEffect
                        {
                            CashDelta = -1200,
                            ReputationDelta = -0.2,
                            ResultMessage = "Kunden enttäuscht — 1.200 € Umsatzverlust.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "rival_open",
                Title = "Konkurrenz eröffnet nebenan",
                Description = "Ein neuer Imbiss eröffnet in der Nachbarschaft einer Filiale.",
                Emoji = "🆚",
                Category = EventCategory.Bad,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Rabatt-Aktion starten",
                        Effect = new EventEffect
                        {
                            CashDelta = -500,
                            ReputationDelta = 0.2,
                            ResultMessage = "Stammkundschaft bleibt dank Aktion treu.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Qualität betonen, keine Aktion",
                        Effect = new EventEffect
                        {
                            CashDelta = -800,
                            ReputationDelta = -0.1,
                            ResultMessage = "Einige Kunden probieren die Konkurrenz aus — kleiner Verlust.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "meat_price_shock",
                Title = "Fleischpreis-Schock",
                Description = "Die Lammfleisch-Preise sind diese Woche um 30% gestiegen. Wie reagierst du?",
                Emoji = "📈",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinDay = 20 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Preise erhöhen (+0,50 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = 0,
                            ReputationDelta = -0.10,
                            ResultMessage = "Kunden grummeln, aber Margen bleiben stabil.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Marge schlucken",
                        Effect = new EventEffect
                        {
                            CashDelta = -1500,
                            ReputationDelta = 0.10,
                            ResultMessage = "Stammkunden danken es — aber das tut weh.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Auf günstigeres Hähnchen umsteigen",
                        Effect = new EventEffect
                        {
                            CashDelta = 500,
                            ReputationDelta = -0.20,
                            ResultMessage = "Geld gespart, aber Connaisseure schmecken den Unterschied.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "bad_review",
                Title = "Vernichtende Online-Bewertung",
                Description = "Ein Kunde hat einen wütenden Google-Review hinterlassen (\"schlimmster Döner meines Lebens\"). Wie gehst du damit um?",
                Emoji = "⚠️",
                Category = EventCategory.Bad,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Professionell antworten + Gutschein anbieten",
                        Effect = new EventEffect
                        {
                            CashDelta = -50,
                            ReputationDelta = 0.10,
                            ResultMessage = "Andere Kunden sehen deine ruhige Reaktion — Vertrauensgewinn.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Zurückkeilen (\"Lügner!\")",
                        Effect = new EventEffect
                        {
                            CashDelta = 0,
                            ReputationDelta = -0.40,
                            BrandAwarenessDelta = 3.0,
                            ResultMessage = "Shitstorm. Mehr Bekanntheit — aber peinliche.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Ignorieren",
                        Effect = new EventEffect
                        {
                            CashDelta = 0,
                            ReputationDelta = -0.15,
                            ResultMessage = "Der Review bleibt stehen und schreckt einige ab.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "viral_tiktok",
                Title = "Viraler TikTok!",
                Description = "Ein Influencer hat deinen Imbiss auf TikTok gezeigt — Tausende von Aufrufen! 🎉",
                Emoji = "📱",
                Category = EventCategory.Good,
                Weight = EventWeight.Rare,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Auf der Welle reiten",
                        Effect = new EventEffect
                        {
                            CashDelta = 1500,
                            ReputationDelta = 0.5,
                            BrandAwarenessDelta = 5.0,
                            ResultMessage = "Viele neue Kunden strömen — Tagesumsatz +1.500 €, Marke deutlich bekannter.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "food_blogger",
                Title = "Food-Blogger anwesend",
                Description = "Ein bekannter Food-Blogger isst gerade bei dir. Bietest du ihm ein Sonder-Tasting an?",
                Emoji = "✍️",
                Category = EventCategory.Opportunity,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Sonder-Tasting anbieten (kostenlos)",
                        Effect = new EventEffect
                        {
                            CashDelta = -100,
                            ReputationDelta = 0.4,
                            BrandAwarenessDelta = 2.0,
                            ResultMessage = "Tolle Bewertung im Blog — Reputation steigt deutlich.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Normal bedienen",
                        Effect = new EventEffect
                        {
                            CashDelta = 20,
                            ReputationDelta = 0.05,
                            ResultMessage = "Nette Erwähnung im Blog — kleine Reputations-Steigerung.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "football_event",
                Title = "Fußball-WM-Übertragung",
                Description = "Heute spielt die deutsche Mannschaft. Eine Sport-Bar nebenan fragt nach Catering.",
                Emoji = "⚽",
                Category = EventCategory.Opportunity,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Großes Catering liefern (Aufwand)",
                        Effect = new EventEffect
                        {
                            CashDelta = 2500,
                            ReputationDelta = 0.2,
                            ResultMessage = "Catering perfekt geliefert — 2.500 € extra Umsatz!",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Klein bleiben",
                        Effect = new EventEffect
                        {
                            CashDelta = 300,
                            ResultMessage = "Ein paar Walk-ins durch die Sport-Welle.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "newspaper_feature",
                Title = "Lokalzeitung will Story",
                Description = "Die lokale Zeitung will eine Story über Familienbetriebe machen. Du bist im Gespräch.",
                Emoji = "📰",
                Category = EventCategory.Opportunity,
                Requirements = new EventRequirements { MinDay = 10 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Familien-Story erzählen",
                        Effect = new EventEffect
                        {
                            ReputationDelta = 0.3,
                            BrandAwarenessDelta = 4.0,
                            ResultMessage = "Touching Porträt erscheint. Stadt-Reputation deutlich gesteigert.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Höflich ablehnen (keine Zeit)",
                        Effect = new EventEffect
                        {
                            ResultMessage = "Kein Risiko, keine Belohnung.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "wedding_catering",
                Title = "Hochzeit-Catering-Anfrage",
                Description = "Stammkunde fragt: kannst du seine Hochzeit (200 Gäste) catern?",
                Emoji = "💒",
                Category = EventCategory.Opportunity,
                Requirements = new EventRequirements { MinShops = 2 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Annehmen — Vorbereitung 3.000 €",
                        Effect = new EventEffect
                        {
                            CashDelta = 5500,
                            ReputationDelta = 0.4,
                            BrandAwarenessDelta = 3.0,
                            ResultMessage = "Catering erfolgreich! 8.500 € brutto, abzgl. Kosten = +5.500 €.",
                        },
                        Cost = 3000,
                    },
                    new EventChoice
                    {
                        Label = "Höflich absagen",
                        Effect = new EventEffect
                        {
                            ResultMessage = "Kunde versteht — keine Schäden.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "star_chef_visit",
                Title = "Sterne-Koch besucht",
                Description = "Ein bekannter TV-Koch isst inkognito bei dir — und gibt sich danach zu erkennen!",
                Emoji = "👨‍🍳",
                Category = EventCategory.Good,
                Weight = EventWeight.Rare,
                Requirements = new EventRequirements { MinShops = 3, MinDay = 30 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Gemeinsames Foto + Social-Post",
                        Effect = new EventEffect
                        {
                            ReputationDelta = 0.5,
                            BrandAwarenessDelta = 8.0,
                            ResultMessage = "Foto geht durch die Decke. Marke bundesweit bekannter!",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Diskret bleiben",
                        Effect = new EventEffect
                        {
                            ReputationDelta = 0.2,
                            ResultMessage = "Klassische Diskretion — der Koch empfiehlt dich privat weiter.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "heatwave",
                Title = "Hitzewelle",
                Description = "38 °C in der Stadt! Die Leute wollen heute weniger heißen Döner, aber dafür kalte Getränke.",
                Emoji = "☀️",
                Category = EventCategory.Neutral,
                Weight = EventWeight.Common,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Ayran-Aktion fahren",
                        Effect = new EventEffect
                        {
                            CashDelta = 800,
                            ResultMessage = "Ayran-Verkauf explodiert — 800 € Extra!",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Normal weitermachen",
                        Effect = new EventEffect
                        {
                            CashDelta = -400,
                            ResultMessage = "Weniger Kunden bei der Hitze — 400 € weniger.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "rainy_day",
                Title = "Dauerregen",
                Description = "Den ganzen Tag schüttet es. Wenige Passanten heute.",
                Emoji = "🌧️",
                Category = EventCategory.Neutral,
                Weight = EventWeight.Common,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Lieferdienst anbieten (300 € Aufbau)",
                        Effect = new EventEffect
                        {
                            CashDelta = 600,
                            ResultMessage = "Lieferaufträge gleichen den Ausfall aus — +600 € netto.",
                        },
                        Cost = 300,
                    },
                    new EventChoice
                    {
                        Label = "Personal eher gehen lassen",
                        Effect = new EventEffect
                        {
                            CashDelta = -200,
                            ResultMessage = "Halber Tag = weniger Lohnkosten, aber auch weniger Umsatz.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "oktoberfest",
                Title = "Stadtfest am Wochenende",
                Description = "In deiner Stadt findet ein großes Volksfest statt. Massen sind unterwegs!",
                Emoji = "🎪",
                Category = EventCategory.Opportunity,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Pop-up-Stand aufstellen (1.500 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = 3800,
                            ReputationDelta = 0.2,
                            ResultMessage = "Pop-up erfolgreich! +3.800 € netto durch das Fest.",
                        },
                        Cost = 1500,
                    },
                    new EventChoice
                    {
                        Label = "Normal bleiben, Walk-ins mitnehmen",
                        Effect = new EventEffect
                        {
                            CashDelta = 800,
                            ResultMessage = "800 € extra durch Spontan-Besucher.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "employee_quit_threat",
                Title = "Mitarbeiter droht zu kündigen",
                Description = "Dein bester Mitarbeiter hat ein Angebot von der Konkurrenz.",
                Emoji = "😤",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinShops = 2, MinDay = 15 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Gehaltserhöhung anbieten (500 € jetzt)",
                        Effect = new EventEffect
                        {
                            CashDelta = -500,
                            ReputationDelta = 0.05,
                            ResultMessage = "Mitarbeiter bleibt. Loyalität gesichert.",
                        },
                        Cost = 500,
                    },
                    new EventChoice
                    {
                        Label = "Loslassen — Glückwunsch zur neuen Stelle",
                        Effect = new EventEffect
                        {
                            CashDelta = 0,
                            ReputationDelta = -0.15,
                            ResultMessage = "Du verlierst Erfahrung. Andere Mitarbeiter sind nervös.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "vegan_trend",
                Title = "Veganer-Trend in der Stadt",
                Description = "Lokale Medien feiern den Vegan-Trend. Stadt-Reputation für Imbisse mit veganem Angebot steigt.",
                Emoji = "🥗",
                Category = EventCategory.Opportunity,
                Requirements = new EventRequirements { MinDay = 14 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Vegane Karte erweitern (800 € Marketing)",
                        Effect = new EventEffect
                        {
                            CashDelta = -800,
                            ReputationDelta = 0.35,
                            BrandAwarenessDelta = 2.5,
                            ResultMessage = "Trendsetter-Image! Hippe Kundschaft kommt.",
                        },
                        Cost = 800,
                    },
                    new EventChoice
                    {
                        Label = "Ignorieren — wir bleiben klassisch",
                        Effect = new EventEffect
                        {
                            ReputationDelta = -0.05,
                            ResultMessage = "Du verpasst die Welle, aber Stammkunden bleiben.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "tax_audit",
                Title = "Finanzamt-Prüfung",
                Description = "Das Finanzamt schaut sich deine Bücher an.",
                Emoji = "🧾",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinShops = 2, MinDay = 40 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Vollständig kooperieren",
                        Effect = new EventEffect
                        {
                            CashDelta = -800,
                            ResultMessage = "Saubere Bücher — nur Kleinigkeiten zu beanstanden.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Steuerberater einschalten (1.500 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -1500,
                            ReputationDelta = 0.10,
                            ResultMessage = "Profi findet Optimierungen — und das Image als seriös bleibt erhalten.",
                        },
                        Cost = 1500,
                    },
                },
            },
            new GameEvent
            {
                Id = "kitchen_fire",
                Title = "Feuer in der Küche!",
                Description = "Ein Fettbrand ist in einer Filiale ausgebrochen. Niemand verletzt, aber die Küche ist beschädigt. Wie reagierst du?",
                Emoji = "🔥",
                Category = EventCategory.Bad,
                Weight = EventWeight.Rare,
                Requirements = new EventRequirements { MinDay = 15 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Sofort professionell sanieren (2.500 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -2500,
                            ReputationDelta = 0.10,
                            ResultMessage = "Schnell und sauber wieder eröffnet — Kunden honorieren die Professionalität.",
                        },
                        Cost = 2500,
                    },
                    new EventChoice
                    {
                        Label = "Notbetrieb mit halber Küche",
                        Effect = new EventEffect
                        {
                            CashDelta = -4000,
                            ReputationDelta = -0.30,
                            ResultMessage = "Tagelang eingeschränkt — Umsatzausfall und genervte Stammkunden.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "social_scandal",
                Title = "Shitstorm auf Social Media",
                Description = "Ein Mitarbeiter-Video sorgt für einen Skandal — der Hashtag trendet, und nicht im Guten. Wie gehst du damit um?",
                Emoji = "🌪️",
                Category = EventCategory.Bad,
                Weight = EventWeight.Rare,
                Requirements = new EventRequirements { MinShops = 2, MinDay = 12 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Ehrliche Entschuldigung + Team-Schulung (1.200 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -1200,
                            ReputationDelta = 0.10,
                            BrandAwarenessDelta = -1.0,
                            ResultMessage = "Die offene Reaktion kommt an — der Sturm legt sich schneller als gedacht.",
                        },
                        Cost = 1200,
                    },
                    new EventChoice
                    {
                        Label = "Aussitzen und schweigen",
                        Effect = new EventEffect
                        {
                            ReputationDelta = -0.50,
                            BrandAwarenessDelta = -4.0,
                            ResultMessage = "Das Schweigen wird als Arroganz gelesen — Reputation und Marke leiden deutlich.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Anwalt einschalten, hart kontern (3.000 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -3000,
                            ReputationDelta = -0.10,
                            ResultMessage = "Teuer, und die Öffentlichkeit findet es kleinlich — aber das Thema verschwindet.",
                        },
                        Cost = 3000,
                    },
                },
            },
            new GameEvent
            {
                Id = "night_robbery",
                Title = "Einbruch über Nacht",
                Description = "Diebe haben nachts die Kasse einer Filiale geknackt. Was nun?",
                Emoji = "🚨",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinDay = 20, MinCash = 25000 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Anzeige + Versicherung regeln",
                        Effect = new EventEffect
                        {
                            CashDelta = -800,
                            ResultMessage = "Ein Teil ist versichert — der Schaden hält sich in Grenzen.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Sicherheitssystem nachrüsten (2.500 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -2500,
                            ReputationDelta = 0.05,
                            BrandAwarenessDelta = 0.5,
                            ResultMessage = "Investition in Sicherheit — Mitarbeiter und Kunden fühlen sich wohler.",
                        },
                        Cost = 2500,
                    },
                },
            },
            new GameEvent
            {
                Id = "power_outage",
                Title = "Stromausfall im Viertel",
                Description = "Ein Kabelschaden legt das ganze Viertel lahm. Kühlung und Grill stehen still.",
                Emoji = "⚡",
                Category = EventCategory.Bad,
                Weight = EventWeight.Common,
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Notstromaggregat mieten (600 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -600,
                            ResultMessage = "Der Laden läuft weiter, als wäre nichts gewesen.",
                        },
                        Cost = 600,
                    },
                    new EventChoice
                    {
                        Label = "Spontan-Aktion: Holzkohlegrill auf den Gehweg",
                        Effect = new EventEffect
                        {
                            CashDelta = 300,
                            ReputationDelta = 0.20,
                            BrandAwarenessDelta = 1.0,
                            ResultMessage = "Improvisierter Grill-Abend wird zum Stadtgespräch — sympathisch und lukrativ!",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Tag abschreiben, schließen",
                        Effect = new EventEffect
                        {
                            CashDelta = -1500,
                            ResultMessage = "Verderbliche Ware verloren, Umsatz futsch.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "food_poisoning_rumor",
                Title = "Gerücht: Lebensmittelvergiftung",
                Description = "Im Netz kursiert die Behauptung, jemand sei nach dem Essen bei dir krank geworden. Beweise gibt es keine. Wie reagierst du?",
                Emoji = "🤢",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinShops = 2, MinDay = 25 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Transparenz-Offensive + freiwilliger Labortest (1.500 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -1500,
                            ReputationDelta = 0.25,
                            BrandAwarenessDelta = 1.0,
                            ResultMessage = "Der Labortest entlastet dich öffentlich — am Ende stärkt es das Vertrauen.",
                        },
                        Cost = 1500,
                    },
                    new EventChoice
                    {
                        Label = "Knapp dementieren",
                        Effect = new EventEffect
                        {
                            ReputationDelta = -0.20,
                            ResultMessage = "Das Dementi verpufft — ein Teil der Zweifel bleibt.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Ignorieren, wird schon vorbeigehen",
                        Effect = new EventEffect
                        {
                            ReputationDelta = -0.50,
                            BrandAwarenessDelta = -3.0,
                            ResultMessage = "Das Gerücht frisst sich fest — spürbarer Reputations- und Markenschaden.",
                        },
                    },
                },
            },
            new GameEvent
            {
                Id = "employee_theft",
                Title = "Griff in die Kasse",
                Description = "Das neue Kassensystem deckt auf: ein Mitarbeiter hat über Wochen Geld abgezweigt. Wie gehst du vor?",
                Emoji = "🕵️",
                Category = EventCategory.Bad,
                Requirements = new EventRequirements { MinShops = 2, MinDay = 20 },
                Choices = new List<EventChoice>
                {
                    new EventChoice
                    {
                        Label = "Fristlos kündigen + Anzeige",
                        Effect = new EventEffect
                        {
                            CashDelta = -300,
                            ResultMessage = "Klare Kante. Das Team versteht die Botschaft, etwas Geld ist futsch.",
                        },
                    },
                    new EventChoice
                    {
                        Label = "Zentrales Kassensystem konzernweit aufrüsten (2.000 €)",
                        Effect = new EventEffect
                        {
                            CashDelta = -2000,
                            ReputationDelta = 0.05,
                            BrandAwarenessDelta = 0.5,
                            ResultMessage = "Investition in Kontrolle — künftig hat Schwund keine Chance mehr.",
                        },
                        Cost = 2000,
                    },
                },
            },
        };

        public static GameEvent ById(string id) => All.FirstOrDefault(e => e.Id == id);
    }
}
