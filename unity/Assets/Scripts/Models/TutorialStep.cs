// Döner Empire 3D — Tutorial-Schritte + Metadaten
// Port aus lib/models/tutorial_model.dart.

namespace DoenerEmpire.Models
{
    public enum TutorialStep
    {
        OpenFirstShop,
        UnderstandLocationValues,
        ChangeProductPrice,
        HireFirstEmployee,
        EndFirstDay,
        ReadDayReport,
        ViewDashboardMetrics,
        OpenEmpireMenu,
        UnderstandHrCompetitionGrowth,
        FinishTutorial,
    }

    public static class TutorialInfo
    {
        public const int StepCount = 10;

        public static TutorialStep FromIndex(int index)
        {
            if (index <= 0) return TutorialStep.OpenFirstShop;
            var values = System.Enum.GetValues(typeof(TutorialStep));
            if (index >= values.Length) return TutorialStep.FinishTutorial;
            return (TutorialStep)index;
        }

        public static string Title(TutorialStep s) => s switch
        {
            TutorialStep.OpenFirstShop => "Erste Filiale eröffnen",
            TutorialStep.UnderstandLocationValues => "Standortwerte verstehen",
            TutorialStep.ChangeProductPrice => "Produktpreis ändern",
            TutorialStep.HireFirstEmployee => "Mitarbeiter einstellen",
            TutorialStep.EndFirstDay => "Ersten Tag abschließen",
            TutorialStep.ReadDayReport => "Tagesbericht lesen",
            TutorialStep.ViewDashboardMetrics => "Dashboard-Kennzahlen ansehen",
            TutorialStep.OpenEmpireMenu => "Imperium-Menü öffnen",
            TutorialStep.UnderstandHrCompetitionGrowth => "HR, Konkurrenz, Wachstum",
            TutorialStep.FinishTutorial => "Tutorial abschließen",
            _ => "",
        };

        public static string Description(TutorialStep s) => s switch
        {
            TutorialStep.OpenFirstShop => "Wechsle zu Städte und eröffne deine erste Filiale.",
            TutorialStep.UnderstandLocationValues => "Prüfe Miete, Nachfrage und Konkurrenz am Standort.",
            TutorialStep.ChangeProductPrice => "Passe in einer Filiale mindestens einen Produktpreis an.",
            TutorialStep.HireFirstEmployee => "Stelle einen ersten Mitarbeiter aus dem Bewerberpool ein.",
            TutorialStep.EndFirstDay => "Beende den Tag für Umsatz, Kosten und Reputation.",
            TutorialStep.ReadDayReport => "Lies den Tagesabschluss und bestätige ihn.",
            TutorialStep.ViewDashboardMetrics => "Sieh dir Kasse, Kunden und Tagesprofit an.",
            TutorialStep.OpenEmpireMenu => "Öffne das Imperium-Menü für deinen Gesamtfortschritt.",
            TutorialStep.UnderstandHrCompetitionGrowth => "Prüfe kurz HR, Konkurrenz und Wachstumsoptionen.",
            TutorialStep.FinishTutorial => "Schließe das Tutorial ab und spiele frei weiter.",
            _ => "",
        };

        public static string Hint(TutorialStep s) => s switch
        {
            TutorialStep.OpenFirstShop => "Tipp: Tab \"Städte\"",
            TutorialStep.UnderstandLocationValues => "Vergleiche die Werte vor der Standortwahl.",
            TutorialStep.ChangeProductPrice => "Preise veränderst du in den Filialdetails.",
            TutorialStep.HireFirstEmployee => "Mitarbeiter findest du in der Filiale.",
            TutorialStep.EndFirstDay => "Nutze den goldenen Button im Dashboard.",
            TutorialStep.ReadDayReport => "Schließe den Tagesbericht nach dem Lesen.",
            TutorialStep.ViewDashboardMetrics => "Achte auf Gewinn, Kunden und aktuelle Kosten.",
            TutorialStep.OpenEmpireMenu => "Tipp: Tab \"Imperium\"",
            TutorialStep.UnderstandHrCompetitionGrowth => "Öffne den Konzern-Tab für HR und Expansion.",
            TutorialStep.FinishTutorial => "Du bist bereit für den Ausbau deines Imperiums.",
            _ => "",
        };

        /// <summary>Button-Label für Schritte mit expliziter Bestätigung (sonst null).</summary>
        public static string ActionLabel(TutorialStep s) => s switch
        {
            TutorialStep.UnderstandLocationValues => "Weiter",
            TutorialStep.ViewDashboardMetrics => "Kennzahlen angesehen",
            TutorialStep.UnderstandHrCompetitionGrowth => "Weiter",
            TutorialStep.FinishTutorial => "Tutorial beenden",
            _ => null,
        };

        public static string WhyItMatters(TutorialStep s) => s switch
        {
            TutorialStep.OpenFirstShop => "Ohne Filiale entstehen keine Einnahmen. Das ist dein Startpunkt für alles Weitere.",
            TutorialStep.UnderstandLocationValues => "Standortwerte bestimmen, wie schnell eine Filiale profitabel wird und wie viel Risiko du trägst.",
            TutorialStep.ChangeProductPrice => "Preisentscheidungen beeinflussen Nachfrage und Marge direkt. Das ist einer der wichtigsten Hebel.",
            TutorialStep.HireFirstEmployee => "Personal sorgt für Kapazität und Servicequalität. Ohne Team wächst die Filiale nicht stabil.",
            TutorialStep.EndFirstDay => "Der Tagesabschluss zeigt dir, ob dein Setup wirtschaftlich funktioniert.",
            TutorialStep.ReadDayReport => "Im Bericht erkennst du früh, ob Preise, Personal oder Kosten angepasst werden müssen.",
            TutorialStep.ViewDashboardMetrics => "Das Dashboard hilft dir, schnelle Entscheidungen datenbasiert zu treffen.",
            TutorialStep.OpenEmpireMenu => "Im Imperium-Bereich siehst du Fortschritt, Ziele und wichtige Langzeitwerte.",
            TutorialStep.UnderstandHrCompetitionGrowth => "HR, Konkurrenz und Wachstum sind zentrale Systeme für deinen langfristigen Erfolg.",
            TutorialStep.FinishTutorial => "Mit dem Abschluss kennst du die Kernmechaniken und kannst eigenständig optimieren.",
            _ => "",
        };

        /// <summary>Ziel-Tab des Schritts (Bottom-Nav-Index) oder null.</summary>
        public static int? TargetTabIndex(TutorialStep s) => s switch
        {
            TutorialStep.OpenFirstShop => 1,
            TutorialStep.UnderstandLocationValues => 1,
            TutorialStep.ChangeProductPrice => 1,
            TutorialStep.HireFirstEmployee => 1,
            TutorialStep.EndFirstDay => 0,
            TutorialStep.ReadDayReport => 0,
            TutorialStep.ViewDashboardMetrics => 0,
            TutorialStep.OpenEmpireMenu => 2,
            TutorialStep.UnderstandHrCompetitionGrowth => 3,
            TutorialStep.FinishTutorial => null,
            _ => null,
        };
    }
}
