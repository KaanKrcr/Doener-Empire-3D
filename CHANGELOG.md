# Changelog

## Unreleased - Flutter-Fokus, Deps & CI-Härtung (2026-06-10)

### Strategie

- Entscheidung: Unity-Voll-Port pausiert (bleibt als Referenz im Repo),
  Premium-Optik stattdessen als **isometrische 2.5D-City-Map direkt in Flutter**
  (Render-Schicht; Spiellogik unverändert). Sprite-Pipeline mit Vektor-Fallback.

### Dependencies (Major-Updates, je einzeln CI-verifiziert)

- `intl` 0.19 → 0.20.2, `flutter_lints` 3 → 6
- `fl_chart` 0.68 → 1.2.0 (genutzte Chart-APIs unverändert)
- `go_router` 13 → 17.3.0 (Config nutzte bereits moderne API)
- `flutter_riverpod` 2 → 3.3.2 (Kern auf `Notifier`; `StateProvider` via
  `legacy.dart`)
- Patch-Bumps: `shared_preferences_android`/`url_launcher_android` auf
  Built-in Kotlin migriert (KGP-Warnung nur noch `audioplayers`, upstream)
- `cupertino_icons` ergänzt → Font-Manifest-Build-Warnung behoben

### CI / Qualitätssicherung

- **Flutter-CI-Gate** (`flutter analyze` + `flutter test`) auf jedem PR
- **Branch-Protection** auf `main`: grüner Check Pflicht, keine Force-Pushes
- GitHub-Actions auf Node-24-Majors (checkout v6, setup-dotnet v5, cache v5,
  upload-artifact v7)
- Render-Smoke-Tests für alle 6 Haupt-Tabs (Dashboard/Cities/Stats/Corporate/
  Finance/Bank) + Finanz-Chart (fl_chart 1)

## Unreleased - Unity-Port (2026-06-09)

### Engine-Port (Flutter → Unity C#)

- Vollständige Spiellogik nach C# portiert (`unity/Assets/Scripts/`):
  Competitor-, Location-, Campaign-, Mission-, Hr-, Marketing-,
  Facility-, M&A-, Stocks-, Auto-Management-Engines
- `DayProcessing.ProcessDay`: vollständiger Tagesabschluss (Umsatz/Kosten,
  Reputation, Employee-XP, Kampagnen, Loans, Brand, City-Unlocks, Stocks,
  Facilities, HR-Progress, Auto-Pricing/Auto-Hire)
- Preis-Empfehlung (`RevenueOptimalPrice`) portiert
- Shop-Eröffnung wendet globale/stadtweite Preis-Overrides an

### Save-Kompatibilität

- Unity-`SaveService` lädt alle Flutter-`toJson`-Felder round-trip-fähig
  (stocks, facilities, hr*, history, missions, campaigns, combos,
  productQuality, globalPrices, cityPrices); Legacy-Saves mit Defaults

### Content-Systeme (C#)

- Achievements (19 Trophäen) + Unlock-Auswertung
- Start-Szenarien, Marken-Skins, Tutorial-Schritte
- Events/Krisen (24 Events, generiert via `tools/gen_event_catalog.py`) +
  gewichtete Auswahl & Anwendung
- `EndOfDayService`: vollständiger Tagesabschluss (Wirtschaft + Missionen,
  Kampagne, Achievements, Event, Quartal/Woche, Steuer, Daily Challenge)
- Analyse-Schicht: Produkt-Profitabilität, Filial-Ranking, Marktanteil,
  Unternehmens-Gesundheit, Warnungen, Kundenbewertungen, Preis-Empfehlung,
  stündlicher Tick

### Sonstiges

- Android App-Label `doener_empire` → `Döner Empire`
- 507 Unity-Logik-Tests (xUnit) grün; Flutter 86 Tests + analyze clean

## 1.1.0-internal - 2026-05-30

### Optik & Game-Feel

- Gebündelte Schriften (Baloo 2 / Inter) + app-weite Typografie
- Animationen (flutter_animate), Tap-Feedback (Pressable), Seiten-Übergänge
- Tagesabschluss-Konfetti, Splash-Politur
- Sound-Effekte (Kenney CC0) + Haptik, Mute-Schalter im Spielmenü
- Aktienkurs-Chart auf fl_chart umgestellt

### Story & Progression

- Story-Kampagne: 8 Kapitel mit Zielen, Cash-Belohnungen und permanenten
  Konzern-Perks; Perk-Übersicht; Kapitel-Abschluss-Feier
- Trophäen-Galerie (eigener Screen)
- Filial-Branding / Skins über Trophäen freischaltbar
- Start-Szenarien (Klassisch, Schuldenstart, Hardcore, High-Roller)

### Gameplay-Tiefe

- Kombo-Menüs / Mittagsangebote (an Produkt-/Equipment-Progression gekoppelt)
- Produkt-Qualitätsstufen (Günstig/Standard/Premium)
- Tagesspecial (täglich rotierendes Gericht mit Bonus-Nachfrage)
- Krisen-Events (Küchenbrand, Shitstorm, Einbruch, Stromausfall, u. a.)
- Quartalssteuer (12 % auf den Monatsgewinn)

### Analyse & Komfort

- Produkt-Profitabilität + Filial-Ranking (Finanzen)
- Dashboard-Hinweise (Verluste/Ruf/Liquidität)
- Unternehmens-Gesundheit (Health-Score) im Imperium-Tab
- Wochen-Report alle 7 Tage
- Empire-Share-Card (teilbare Zusammenfassung)

### Behoben

- Neues-Spiel zeigte veraltetes Startkapital (50 Mio. statt echtem Wert)

### Weitere Gameplay-/Analyse-Features

- Daily Challenges (tägliche, relativ gestellte Ziele mit Belohnung)
- Jahreszeiten (saisonale Kategorie-Nachfrage)
- Kunden-Bewertungen (prozedural aus Reputation/Preis/Qualität)
- Marktanteil-Visualisierung pro Stadt
- Preis-Empfehlung (umsatzoptimale Preise per Knopfdruck)
- 4 neue Trophäen (Langzeit-Ziele)
- Dashboard entzerrt (Tagesspecial + Tagesaufgabe als eine „Heute"-Karte)

### Geprüft

- `flutter analyze` ohne Findings
- `flutter test`: 83 Tests erfolgreich (inkl. 60-Tage-Integrationstest aller Systeme)
- `flutter build apk --debug` erfolgreich

## 1.0.0-internal.1 - 2026-05-25

### Hinzugefügt

- Interne Release-Dokumentation für den ersten Testlauf:
  - Setup-/Build-Anleitung in der README
  - Liste bekannter Risiken/Bugs
  - Vorschläge für GitHub-Issues, Labels und Milestones
  - Manueller Testplan für interne Tester

### Geprüft

- `flutter analyze` ohne Findings
- `flutter test` erfolgreich
- Android Build:
  - `flutter build apk --debug` erfolgreich
  - `flutter build apk --release` erfolgreich

### Hinweise

- Release-Signing nutzt aktuell Debug-Key (nur für interne Verteilung geeignet).
- Build-Warnung zur Kotlin-Plugin-Migration (`shared_preferences_android`) bleibt offen.
