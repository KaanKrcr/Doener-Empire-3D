# Döner Empire – Findings

---

## Unity-Port-Findings (Stand 2026-06-06, Claude — Senior-Review)

### Toolchain-Status
- **Git LFS:** installiert (3.7.1), Repo-aktiviert, Root-`.gitattributes`
  ergänzt um vollständigen Binär-Asset-Pattern (FBX/PNG/PSD/WAV/TTF/APK/…).
- **.NET 8 SDK:** 8.0.421 — vorhanden.
- **Unity Hub:** vorhanden.
- **Unity 6 LTS (6000.4.9f1) Editor:** installiert.
- **Unity Android Build Support:** ⚠ NICHT installiert — pausierte Downloads
  vorhanden, Hub-CLI-Bug verworfen sie. **Manueller Hub-GUI-Install
  durch Owner ausstehend.**

### Code-Stand Logikschicht
- 31 C#-Dateien unter `unity/Assets/Scripts/` (M1+M2 + Beginn M3).
- Eigenständige `unity-logic-tests/`-Testharness (.NET 8 xUnit).
- **Baseline: 103 grüne Tests** vor heutiger Session.
- **Nach heutiger Session: 115 grüne Tests** (+12 Flutter-Save-Compat).

### Architektur-Verbesserungen (heute, kollisionsfrei)

1. **Assembly Definitions** angelegt für die 5 reinen Logic-Layer
   (`Core`, `Models`, `Data`, `Save`, `Simulation`), alle mit
   `noEngineReferences: true`. Damit ist die Logik-Schicht
   **compile-time-geschützt** gegen versehentliche UnityEngine-Imports.
   App/UI/View3D bleiben bewusst in Default-Assembly, weil App↔UI
   aktuell bidirektional referenziert (würde Asmdef-Zirkel auslösen).

2. **UI-Theme-Foundation** unter `unity/Assets/UI/Theme/Theme.uss`:
   alle Token aus `UI_STYLE_GUIDE.md` (Farben, Typo, Spacing, Radii,
   Motion) als USS Custom Properties + Basis-Komponentenklassen
   (`.decision-sheet`, `.metric-tile`, `.btn--primary`, `.badge`,
   `.fab`, `.bottom-nav`, …). Codex baut UXML-Layouts auf dieser
   Foundation auf, OHNE neue Hex-Werte einzubauen.

3. **Flutter-Save-Compat-Test** + Fixture
   `unity-logic-tests/.../Fixtures/flutter_save_mvp.json`. Verifiziert
   12 Aspekte (Top-Level-Scalars, Shops, Menü, Equipment, Mitarbeiter,
   Konkurrent, Loan, Brand, Difficulty, Enum-Mapping, fehlende
   Felder→Defaults). Befunde dokumentiert in
   `docs/UNITY_SAVE_COMPAT.md` § „Known Asymmetries".

4. **CI-Gate** `.github/workflows/unity-logic-tests.yml` für
   `dotnet test`. Läuft bei jedem Push/PR auf Logic-Pfade.

### Bekannte Save-Asymmetrien (in Tests gepinnt)
Flutter `toJson()` schreibt diese Felder NICHT, C# vergibt Defaults
beim Laden:
- `Shop.morale` → `0.75`
- `Shop.regulars` → `0.0`
- `Shop.sizeTier` → `Klein`
- `Employee.shift` → `Ganztags`

~~Flutter `toJson()` schreibt diese Felder, C#-DTO ignoriert sie~~
**Geschlossen 2026-06-09 (SaveService erweitert, 9 Round-Trip-Tests):**
- ✅ `stocks`, `facilities`, `hrManager`, `hrStrategy`, `hrCandidates`
- ✅ `activeCityCampaigns`, `activeGlobalCampaigns`
- ✅ `completedChapterIds`, `activeComboIds`, `productQuality`
- Legacy-Saves ohne diese Felder laden mit sicheren Defaults (Test gepinnt).

- ✅ `history` (DailyRecord-Liste) — jetzt voll round-trip-fähig.

**Verbleibend (Modelle noch nicht in C#-GameState):**
- `missions` (Mission-Status — C# baut Missionen aus Template, kein
  Persistenz-Feld), `globalPrices`, `cityPrices`. Folgen, wenn die
  zugehörigen GameState-Felder portiert werden.

### Nächste Schritte (Verantwortlichkeiten klar)
- **Owner (du):** Android Build Support per Hub-GUI nachinstallieren.
  Dann meldest du dich → ich erzeuge `Packages/manifest.json` für URP
  und triggere Unity batchmode für initiale `ProjectSettings/`.
- ~~**Codex:** `CompetitorEngine`-Port als nächste Engine (M4a).~~
  **Erledigt 2026-06-09 (Claude übernimmt Codex):**
  - **M4a CompetitorEngine** — `Simulation/CompetitorEngine.cs` mit
    `EnsureCompetitorsForCity` / `ProcessDay` / `CompetitionPressure`
    + 16 Tests. RNG injizierbar.
  - **M4b LocationEngine** — `Simulation/LocationEngine.cs` +
    `Models/CityMapLocation.cs` (UnityEngine-frei, `MapPosition`-Struct)
    + 14 Tests.
  - **M4d MissionEngine** — `Simulation/MissionEngine.cs` +
    `Models/Mission.cs` (inkl. `MissionTemplates.Build()`)
    + 21 Tests. `CompanyPublic` bleibt 0 bis Stocks-Port (M5+).
  - **M4c CampaignEngine** — `Simulation/CampaignEngine.cs` +
    `Models/CampaignChapter.cs` (`CampaignData.Chapters`,
    `AggregatePerks`) + 13 Tests. GameState um `CompletedChapterIds`
    erweitert (Initial + Clone).
  - **M5-Foundation DemandUtils** — `Simulation/DemandUtils.cs` mit
    `Season`, `SeasonForDay`, `SeasonCategoryMultiplier`,
    `DailySpecialProductId`, `PriceDemandFactor` (+ `DailySpecialBoost`,
    `MonthlyTaxRate`-Konstanten) + 20 Tests. Reine pure Funktionen,
    Vorbereitung für `GameEngine.calculateShopStats`-Port.
  - **M4e HrEngine** — `Simulation/HrEngine.cs` + `Models/HrManager.cs`
    (`HrData.Archetypes`/`Strategies`, `HrEnumNames`) + 19 Tests.
    RecruitmentModifiers, Pool-Refresh, XP-Intervall, Kandidaten- &
    Manager-Generierung. RNG injizierbar. GameState um `HrManager`,
    `HrStrategy`, `HrCandidates` erweitert.
  - **M5a GameEngineCore** — `Simulation/GameEngineCore.cs` +
    `ShopDayStats.cs` + `Models/TimeProfile.cs`. `CalculateShopStats`
    (Kunden/Umsatz/Kapazität), Reputations-/Equipment-/Staff-Faktoren,
    deterministische `DailyVariation` (FNV-1a stable hash),
    `HourlyCustomerCurve`, `MaxEmployeesForShop` + 27 Tests.
  - **M5b Tageskosten + Quality + Combos** — `ComboService.cs`,
    `Models/MenuCombo.cs`, `Models/IngredientQuality.cs`,
    `CalculateDailyCostsBreakdown` (+`ShopCostBreakdown`),
    `UpdateReputation` + 34 Tests (Combo 13, DailyCosts 13, Reputation 8).
    GameState um `ActiveComboIds`, `ProductQuality` erweitert.
  - **M6-Foundation Stocks + DayProcessing** — `Models/StockState.cs`
    (IPO/Quartalsbericht), `DayProcessing.cs` (`CheckCityUnlocks`,
    `ActiveComboDailyCost`, `UpdateBrand`) + 16 Tests. GameState um
    `Stocks` erweitert; `MissionEngine.CompanyPublic` liest jetzt
    `Stocks.IsPublic`.
  - **M5c Upgrade-Katalog** — `Data/UpgradeData.cs` (`UpgradeCatalog`
    Shop+Global) + `Simulation/UpgradeService.cs` (Customer/AOV-Boost,
    Reputation, Brand, Tageskosten, Delivery-Provision mit
    `eigen_lieferdienst`-Rabatt) + 19 Tests. In GameEngineCore/
    DayProcessing verdrahtet.
  - **M6 Marketing-Katalog** — `Data/MarketingCatalog.cs`
    (Shop/City/Global-Kampagnen) + `Simulation/MarketingService.cs`
    (Customer-Boost/AOV-Mod/Reputation/Brand-Delta) + 15 Tests.
    GameState um `ActiveCityCampaigns`, `ActiveGlobalCampaigns`
    erweitert. In ShopStats/Reputation/Brand verdrahtet.
  - **M6b ProcessDay** — `DayProcessing.ProcessDay` (Kern-Tagesabschluss:
    Wettbewerb, Umsatz/Kosten-Aggregation, Reputation, Employee-XP,
    Kampagnen-Ablauf, Loan-Raten, History-Trim 60, Brand, City-Unlocks,
    HR-Salary) + `DayResult` + 17 Tests inkl. 60-Tage-Stabilität.
  - **M7a Stocks/IPO-Engine** — `Simulation/CorporateStocksEngine.cs`
    (`CanDoIpo`, `EstimateValuation`, `PerformIpo`, `UpdateDailyPrice`
    Random-Walk, `IsQuarterDue`, `GenerateQuarterlyReport`) + 14 Tests.
    `UpdateDailyPrice` in `ProcessDay` verdrahtet. RNG injizierbar.
  - **M7b Production-Facilities** — `Models/ProductionFacility.cs`
    (`FacilityCatalog`, Tier/Type-Infos) + `Simulation/FacilityEngine.cs`
    (`BuildFacility`, `FacilityDailyCosts`, `FacilityB2BRevenue`,
    `FacilitySavingForShop`) + 13 Tests. GameState um `Facilities`
    erweitert. In `GameEngineCore` (ingredientSaving) + `ProcessDay`
    (facilityNet auf Cash/TotalProfit/TotalRevenue) verdrahtet.
  - **M7c M&A-Engine** — `Simulation/MergersEngine.cs` (`AcquisitionPrice`,
    `AcquireCompetitor` → Konkurrenz-Filialen als Player-Shops mit
    Default-Menü + übernommener Reputation) + 7 Tests.
  - **M7d Auto-Management** — `Simulation/AutoManagementEngine.cs`
    (`ApplyManagerAutoPricing`, `ApplyAutoHire` mit Kandidaten-Scoring,
    Cash-Reserve, Hire-Fee-Multiplikator, City-Cap) +
    `Simulation/ManagerService.cs` (assign/unassign/ShopHasActiveManager)
    + 18 Tests (12 Auto-Mgmt, 6 Manager). RNG injizierbar.
  - **HR-Manager-XP-Progress** — `DayProcessing.UpdateHrManagerProgress`
    (XP skaliert mit Kundenzahl, Level alle 120 XP) in ProcessDay
    verdrahtet + Test.
  - **Suite: 115 → 398 grüne Tests (+283).**
  - **Verdrahtet & getestet:** Season/Special, Preis-Nachfrage,
    Equipment/Staff/Capacity, Combos, Quality, Upgrades (inkl. Delivery),
    Marketing (Shop/City/Global), Reputation, Brand, City-Unlocks,
    Tageskosten, **vollständiger ProcessDay** (inkl. Stocks-Preis,
    Facility-Net, HR-Progress, Auto-Pricing, Auto-Hire), IPO/Quartal,
    Facilities, M&A.
  - **GameEngine + CorporateEngine-Port logisch VOLLSTÄNDIG.** Verbleibend
    nur reine UI-Aktionen/Tick-Helfer (calculateHourly*, openShop etc.) —
    nicht teil der Tagessimulation und für die Logik-Schicht unkritisch.
- **Claude (ich):** Code-Review jedes Codex-Engine-Ports vor Merge,
  ggf. Save-DTO um neue Felder erweitern wenn Engine sie braucht.

---

## Ausgangslage

Aktueller Projektpfad:

```text
/data/.openclaw/workspace/projects/doener-empire
```

Aktueller GitHub-Stand laut Clone:

```text
1130c834f0f40a886bb3dd70b46a09381f6ff2b6
```

Übergabe von Codex:
- Lieferdienst-Fix/Provision umgesetzt.
- HR-Manager + Auto-Hire-Skalierung umgesetzt.
- Difficulty-Integration erweitert.
- Save-Migration/Stabilität ergänzt.
- `flutter analyze` und `flutter test` laut README_STATUS grün.

## Lokale Verifikation

Ausgeführt mit lokaler Flutter-SDK:

```text
/data/.openclaw/toolchains/flutter
Flutter 3.44.0 / Dart 3.12.0
```

Ergebnisse:
- `flutter analyze`: grün, keine Issues.
- `flutter test`: grün, alle Tests bestanden.
- `flutter build web --release`: erfolgreich, Ausgabe in `build/web`.

Hinweise:
- 11 Dependencies haben neuere Versionen, sind aber durch Constraints inkompatibel. Kein aktueller Fehler.
- Web-Build meldet erwartete Font-Warnung zu Cupertino Icons; bereits als Known Issue ähnlich dokumentiert.
- Android-Build wurde nicht ausgeführt, weil Android SDK auf diesem Server fehlt.

## Vergleich gegen Snapshot 2026-05-25

Alter Snapshot:

```text
/data/.openclaw/workspace-coding/doener-empire-source-review
```

Auffällige neue/erweiterte Bereiche:
- `lib/models/difficulty_model.dart`
- `lib/models/hr_manager_model.dart`
- `lib/models/tutorial_model.dart`
- `lib/services/hr_engine.dart`
- zusätzliche Tests: `branch_naming_test`, `difficulty_system_test`, `hr_system_test`, erweiterte Stability-/Regressionstests.

Diff-Größe gegen Basis `2452c6b`:
- 72 Dateien geändert
- ca. 5636 Insertions / 1136 Deletions

Bewertung:
- Umfang groß, aber Zielbereiche sind testseitig abgedeckt.
- Keine statischen Hinweise auf negative Lieferdienst-Bestellwertlogik.
- Save-Migrationen sind explizit getestet.

## Lieferdienst-Befunde

Bestätigt:
- `lieferdienst` ist globales Upgrade.
- Lieferdienst nutzt `deliveryRevenueFraction` + `deliveryCommissionRate` statt negativem `avgOrderValueBoost`.
- Keine negative `avgOrderValueBoost`-Angabe im Upgrade-Modell gefunden.
- `deliveryCommissionCosts` wird in `DailyRecord` serialisiert und mit Default `0` geladen.
- Legacy-Shop-Upgrade `lieferdienst` wird beim Laden in `globalUpgradeIds` migriert und aus Shops entfernt.
- `processDay` addiert Lieferprovision separat zu Kosten und History.
- Eigene Liefer-App senkt effektive Provision auf 8%.

Zusätzlicher E2E-Test:
- 50 Ingame-Tage mit 4 Shops, `lieferdienst` + `eigen_lieferdienst`.
- Ergebnis: grün.

## HR / Auto-Hire-Befunde

Bestätigt:
- Auto-Hire läuft pro Shop in einer begrenzten Schleife.
- Max-Hires pro Tag sind begrenzt und difficulty-/HR-modifiziert.
- Stop-Bedingungen vorhanden: Cap, leerer Kandidatenpool, Cash-Reserve, fehlender Bedarf.
- Kandidaten werden aus Pool entfernt, nicht unbegrenzt nachgefüllt.
- HR-Defaults für Alt-Saves sind sicher (`HrStrategy.balanced`, kein Manager, leerer Kandidatenpool).
- Difficulty beeinflusst Kandidatenqualität, Gehalt, Recruiting-Geschwindigkeit und Auto-Hire-Aggressivität.

Zusätzlicher E2E-Test:
- 50 Ingame-Tage mit Auto-Hire, großem Pool, Easy + FillFast.
- Ergebnis: grün.

## Bekannte verbleibende Risiken

- Android-Release-Signing nutzt laut `KNOWN_ISSUES.md` noch Debug-Konfiguration.
- Android SDK fehlt auf diesem Server; APK/AAB-Build muss in Android-fähiger Umgebung laufen.
- Manuelle Tests auf echten Geräten bleiben nötig: Save/Load, Navigation, Tagesabschluss, Performance.
- App-Label Android ist noch technisch (`doener_empire`).
- Dependency-Updates sollten nicht blind gemacht werden; mehrere Major/Constraint-Sprünge.

## Empfehlung

Dieser Stand sieht für den nächsten Schritt stabil genug aus. Sinnvoll ist jetzt:
1. E2E-Test + Plan-/Findings-Dateien committen.
2. Android Release-Vorbereitung angehen.
3. Manuelles internes Testpaket nach `docs/MANUAL_TESTPLAN_INTERNAL.md` durchführen.
4. Erst danach neue Gameplay-Features beginnen.
