# UNITY_PRODUCT_VISION — Döner Empire (Unity, Management-Spiel)

Stand: 2026-06-01
Engine: Unity 6 LTS, URP, Mobile-first (Android-Tablet zuerst).
Zielbild: `docs/assets/doener_empire_mobile_premium_ui.png`
Detailierter Bauplan/Meilensteine: [UNITY_REWRITE_PLAN.md](UNITY_REWRITE_PLAN.md)

> **Richtungswechsel (verbindlich):** Kein Arcade-Cooking. Keine Echtzeit-
> Kundenbedienung. Döner Empire bleibt ein **Management-/Progression-Spiel**.
> Unity verbessert ausschließlich **Präsentation, Karte und Standortauswahl**.
> Der frühere Arcade-Plan ist verworfen ([UNITY_MVP_ARCADE_PLAN.md](UNITY_MVP_ARCADE_PLAN.md), deprecated).

---

## 1. Revidierte Unity-Produktvision

**Pitch:** Döner Empire wird von einer mobilen Listen-/Dashboard-Wirtschaftssim
zu einer **premium präsentierten Tycoon-Management-Sim**: dieselben Spielsysteme
und dieselbe Progression wie in der Flutter-Version, aber die Stadt wird als
**realistische 2.5D/3D-Karte** erlebbar und Filialen werden als **anwählbare
Standorte mit Premium-Detailkarten** verwaltet.

**Spielerfantasie:** Vom ersten Laden in einer Kleinstadt zum deutschlandweiten
Döner-Konzern — Wachstum wird auf einer lebendigen Karte **sichtbar** (neue
Filialen, Konkurrenz, Marktanteil), nicht nur in Tabellen.

**Was sich ändert (ggü. Flutter):** Präsentation & Bühne.
**Was gleich bleibt:** Spiellogik, Wirtschaft, Progression, Balancing.

**Kern-Loop (unverändert, management-basiert):**
1. Karte öffnen → Stadtteil/Standort analysieren (Traffic, Miete, Konkurrenz)
2. Filiale eröffnen ODER bestehende auswählen
3. Verwalten: Preise, Menü, Equipment, Personal, Marketing, Ausbau
4. **Tag simulieren** (ein Klick, keine Echtzeit-Bedienung)
5. Tagesbericht lesen (Ursache→Wirkung)
6. Reinvestieren / expandieren / Konkurrenz kontern

---

## 2. Flutter-Features, die erhalten bleiben MÜSSEN

Quelle: `doener_empire/lib` (kanonische, am weitesten entwickelte Logik).

**Wirtschaft & Filialen**
- Städte + City-Tiers (klein/mittel/gross/metropole), Freischaltung über Umsatz
- Filialen: Menü, Preise, Equipment, Personal, Reputation, Moral, Stammkunden
- **ShopSizeTier** (Filialausbau klein→flagship, Cap = min(Stadt, Stufe))
- Tick-basierte **Tagessimulation** + Tagesabschluss mit Umsatz/Kosten/Gewinn
- Produkt-/Preisstrategie, Zutatenkosten, Marge, Döner-Index

**Wachstum & Konzern**
- Zweite Filiale / Stadtwechsel, Standort-Templates pro Tier
- Marketing: Shop-/City-/Global-Kampagnen
- **Konkurrenz** (CompetitorEngine): Marktanteil, Preisreaktion, Expansion
- **M&A / Buyout** (Konkurrenten aufkaufen → übernommene Filialen)
- HR: Einstellen, Manager/Auto-Pricing, Auto-Hire, Training
- Globale Upgrades / Konzern-Ebene

**Meta & Progression**
- Missionen, Achievements, Branding/Themes, Bank/Kredite, Finanzübersicht
- **Schwierigkeitsgrade** (8 Modifikatoren × 4 Stufen — bereits portiert)
- Tutorial/Onboarding, Szenarien, **Save/Load** (lokal)

**Nicht verlieren:** Balancing-Werte und Save-Kompatibilität (JSON-Feldnamen +
Dart-Enum-Strings exakt übernehmen — siehe `unity/Assets/Scripts/Core/Enums.cs`).

---

## 3. Reihenfolge der Portierung (welche Systeme zuerst)

Logik-Layer ist UnityEngine-frei und gegen die Flutter-Werte per `dotnet test`
verifizierbar. **Bereits portiert & grün (45 Tests):** Enums, Difficulty,
Competitor, ShopSizeTier, GameData (Städte/Produkte), Catalog (Mitarbeiter/
Equipment/Standort-Templates).

Nächste Reihenfolge:
1. **Stateful-Modelle** — ShopProduct, ShopEquipment, Employee, Shop, GameState (JSON-kompatibel)
2. **Save/Load** — JSON-Roundtrip gegen einen echten Flutter-Save (Fixture)
3. **GameEngine-Tagessimulation** — calculateShopStats, Kapazität, Tagesabschluss
4. **CompetitorEngine** — Marktanteil/Preisreaktion/Expansion (processDay)
5. **CorporateEngine** — Buyout, Auto-Hire, Manager; **HrEngine**
6. **Marketing/Mission/Campaign-Engines**
7. Erst danach Präsentation: **View3D City Map** + **Premium-UI** (parallel ab Schritt 3 möglich, sobald GameState steht)

Begründung: Erst eine getestete, save-kompatible Simulation — dann die Bühne
draufsetzen. So bleibt Balancing identisch und die UI hat echte Daten.

---

## 4. Karten- & Standortauswahl-UX (realistisch, premium)

**Hauptbühne: 2.5D/3D-Stadtkarte** (orthografische Iso-Kamera, Pinch-Zoom, Pan).

Flow:
```
[City Map]  realistische Stadt, Filialen + Konkurrenz sichtbar
   │  Tap auf Hotspot/Standort
   ▼
[Location Bottom-Sheet]  (wie Referenzbild)
   • Titel + Stadtteil/Stadt + „AKTIVE FILIALE"/„FREI"
   • Sterne-Reputation
   • 4 KPI-Kacheln: Marktanteil (Donut), Fußgänger/Tag, Wochenmiete, Prognose Gewinn
   • Primär: [OPTIMIEREN] (eigene Filiale)  /  [FILIALE ERÖFFNEN] (freier Hotspot)
   │
   ├─ freier Hotspot → [Eröffnen-Dialog]  Kaution/Miete/Empfehlung → bestätigen
   │
   ▼ eigene Filiale → [Restaurant-Detail]  (Premium-Karten, Tabs)
        • Sortiment (Preise/Marge)   • Equipment   • Personal
        • Marketing   • Ausbau (ShopSizeTier)   • Upgrades
   ▼
[Tag simulieren]  (HUD-Button)  → Tagesbericht-Dialog (Ursache→Wirkung)
```

**Karten-Elemente**
- Spieler-Filialen: 3D-Gebäude mit Neon-Outline, Modell je `sizeTier`.
- Konkurrenz: eigene Lade-Prefabs + Label (Name + ★), optional rote Einflusszone.
- Hotspots: Marker mit schwebendem Label + Mini-KPI.
- Day-End: Kundendots (Partikel) laufen zur Filiale (rein visuell, keine Bedienung).
- HUD: Top-Bar (Logo/Firma, Kontostand, Tag) + Bottom-Nav (Übersicht/Filialen/Manager/Forschung/Shop) wie Referenzbild.

**Realismus ohne Asset-Aufwand:** Asset-Store Low-Poly/Synty-City als Basis;
Stadtteil-Typen (Bahnhof/Uni/Innenstadt/Wohngebiet/Business/Nachtviertel) aus
den bestehenden `LocationPersonality`-Profilen ableiten.

---

## 5. MVP-Scope (Unity)

- Eine **3D/2.5D-Stadtkarte** für 1 Startstadt, Iso-Kamera, Zoom/Pan.
- **6 anwählbare Hotspots** mit Standort-Bottom-Sheet (KPIs wie Referenzbild).
- Filiale über Hotspot **eröffnen**; bestehende Filiale auswählen.
- **Restaurant-Detail** mit Premium-Karten: Sortiment/Preise, Equipment, Personal, Ausbau.
- **Tag simulieren** (ein Button) + **Tagesbericht** (Top-Treiber, Ursache→Wirkung).
- **Upgrades/Preise/Personal/Ausbau** wirken auf die (portierte) Simulation.
- **Mindestens 2. Filiale + Stadtfreischaltung** spielbar (Progression sichtbar).
- **Save/Load** lokal (JSON-kompatibel zur Flutter-Struktur).
- Premium-Theme (Farben/Typo aus [UI_STYLE_GUIDE.md](UI_STYLE_GUIDE.md)).
- Läuft auf Android-Tablet (Build) + im Editor.

**MVP-Definition of Done:** Ein Spieler kann in <10 Min eine Filiale über die
3D-Karte eröffnen, Preise setzen, einen Tag simulieren, den Bericht verstehen,
ein Upgrade/eine 2. Filiale kaufen — alles in Premium-Präsentation.

---

## 6. Out-of-Scope (Unity-MVP)

- ❌ Arcade-Cooking / Echtzeit-Kundenbedienung / manuelles Zubereiten
- ❌ Frei begehbare 3D-Stadt, Innenraum-Simulation, First-/Third-Person
- ❌ Multiplayer, Cloud-Save, Accounts
- ❌ Monetarisierung (IAP, Ads), Analytics
- ❌ Vollständige Konzern-Endgame-Tiefe (Börse etc.) im ersten MVP — kommt nach Kern-Loop
- ❌ Neue Spielsysteme erfinden — nur portieren + besser präsentieren
- ❌ Komplexe Pathfinding-KI für Kundendots (rein visuell)

---

## 7. Technische Architektur (Unity)

Vollständig in [UNITY_REWRITE_PLAN.md §1–§4](UNITY_REWRITE_PLAN.md); Kurzfassung:

**Strikte Trennung GameState / Economy(Simulation) / UI / Interaction/View:**
```
Assets/Scripts/
  Core/         Enums, Konstanten, EventBus, Utils      (UnityEngine-frei)
  Models/       Shop, Employee, Competitor, GameState…   (UnityEngine-frei)
  Data/         GameData, GameCatalog (Städte/Produkte/…)
  Simulation/   GameEngine, Competitor/Corporate/Hr…     (UnityEngine-frei)
  Save/         JSON-Speicher (kompatibel zu Flutter)
  View3D/       CityMap-Szene: Kamera, Hotspots, Shop-Prefabs, Kundendots
  UI/           UI-Toolkit (UXML/USS): HUD, Bottom-Sheet, Restaurant-Detail
  App/          Bootstrapping, Scene-Flow, GameController (Riverpod→Controller+Events)
```
- **Logik UnityEngine-frei** → per `dotnet test` gegen Flutter-Werte verifizierbar
  (Harness liegt unter `unity-logic-tests/`).
- **Presentation hört auf den GameController** (Events), mutiert nie direkt State.
- **Save-Kompat:** JSON-Feldnamen + Enum-Strings exakt wie Dart (`EnumNames`).
- Render: **URP**, GPU-Instancing für Kundendots, LODs, Mobile-Budget.

---

## 8. Docs/Dateien, die Codex anlegen/ändern soll

**Docs (anlegen/aktualisieren)**
- ✅ `docs/UNITY_PRODUCT_VISION.md` (dieses Dokument)
- ✅ `docs/UNITY_REWRITE_PLAN.md` (vorhanden — Architektur/Meilensteine; bei Bedarf aktualisieren)
- ⛔ `docs/UNITY_MVP_ARCADE_PLAN.md` (als deprecated belassen, nicht umsetzen)
- 🔲 `docs/UNITY_CITY_MAP_UX.md` — Detailspezifikation Karten-/Bottom-Sheet-/Detail-Flow (Wireframes, Zustände)
- 🔲 `docs/UNITY_SAVE_COMPAT.md` — JSON-Feld-/Enum-Mapping Flutter↔Unity + Migrationsregeln

**Logik-Port (anlegen — `unity/Assets/Scripts/`, UnityEngine-frei)**
- 🔲 `Models/ShopProduct.cs`, `Models/ShopEquipment.cs`, `Models/Employee.cs`
- 🔲 `Models/Shop.cs`, `Models/GameState.cs`
- 🔲 `Save/SaveService.cs`
- 🔲 `Simulation/GameEngine.cs` (Tagessimulation, Stats, Kapazität)
- 🔲 `Simulation/CompetitorEngine.cs`, `Simulation/CorporateEngine.cs`, `Simulation/HrEngine.cs`
- 🔲 `Simulation/CampaignEngine.cs`, `Simulation/MissionEngine.cs`

**Tests (anlegen — `unity-logic-tests/DoenerEmpire.Logic.Tests/`)**
- 🔲 `ShopTests.cs`, `GameStateSaveTests.cs`, `GameEngineDayTests.cs`,
  `CompetitorEngineTests.cs`, `CorporateBuyoutTests.cs`
  (Erwartungswerte spiegeln die Flutter-`test/`-Suite.)

**Presentation (anlegen — nach GameState/Engine, im Unity-Editor)**
- 🔲 `App/GameController.cs`, `App/GameBootstrap.cs`, `Core/EventBus.cs`
- 🔲 `View3D/CityMapController.cs`, `View3D/LocationHotspot.cs`, `View3D/ShopBuilding.cs`, `View3D/CustomerDots.cs`
- 🔲 `UI/Hud.uxml/.uss + HudController.cs`
- 🔲 `UI/LocationSheet.uxml + LocationSheetController.cs` (Bottom-Sheet nach Referenzbild)
- 🔲 `UI/RestaurantDetail.uxml + RestaurantDetailController.cs` (Tabs)
- 🔲 `UI/DayReport.uxml + DayReportController.cs`
- 🔲 Szenen: `Scenes/Boot.unity`, `Scenes/CityMap.unity`

**Bereits vorhanden (nicht neu anlegen):** `Core/Enums.cs`, `Models/DifficultyModel.cs`,
`Models/Competitor.cs`, `Models/ShopSizeTier.cs`, `Data/GameData.cs`, `Data/GameCatalog.cs`,
`unity-logic-tests/*` (45 Tests grün).

---

## Reihenfolge für Codex (kurz)
1. `UNITY_SAVE_COMPAT.md` + Stateful-Modelle (ShopProduct/Equipment/Employee/Shop/GameState) + Tests
2. `SaveService` + Roundtrip-Test gegen Flutter-Save-Fixture
3. `GameEngine` Tagessimulation + Tests (gegen Flutter-Erwartungen)
4. Restliche Engines (Competitor/Corporate/Hr/Campaign/Mission) + Tests
5. `GameController`/`EventBus` (App-Schicht)
6. `UNITY_CITY_MAP_UX.md` → CityMap-Szene + Premium-Bottom-Sheet (Referenzbild)
7. Restaurant-Detail + Day-Report-UI → MVP-Loop schließen → Tablet-Build

Jeder Logik-Schritt: `dotnet test` grün, bevor der nächste beginnt.
