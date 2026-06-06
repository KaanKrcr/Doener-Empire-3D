# Unity Save Compatibility

This document pins the current MVP save shape for the Unity C# port. Field names
use Dart-style camelCase so the save path can stay compatible with the Flutter
model shape while the Unity implementation grows.

## GameState

- `companyName`, `founderName`, `cash`, `currentDay`, `currentHour`
- `shops`, `unlockedCityIds`, `competitors`, `loans`
- `totalRevenue`, `totalProfit`, `customersServedTotal`
- `difficulty` as Dart enum string: `easy`, `normal`, `hard`, `impossible`
- `brand`, `achievementIds`, `employeePool`, `lastEmployeePoolDay`
- `managerEmployeeIds`, `globalUpgradeIds`, `activeThemeId`, `prestigePoints`
- `tutorialDone`, `tutorialEnabled`, `tutorialStep`, `seenEventIds`

## Shop

- Identity/location: `id`, `name`, `customName`, `cityId`, `locationName`
- Metrics/state: `footTraffic`, `weeklyRent`, `isOpen`, `reputation`,
  `dayOpened`, `morale`, `regulars`
- Collections: `menu`, `equipment`, `employees`, `activeCampaigns`,
  `upgradeIds`
- Enums as Dart strings:
  - `personality`: `touristic`, `business`, `transit`, `residential`,
    `university`, `nightlife`
  - `sizeTier`: `klein`, `mittel`, `gross`, `flagship`
- Acquisition/admin: `autoHire`, `originalCompetitorName`, `wasAcquired`

## Shop Submodels

- `ShopProduct`: `productId`, `price`, `isActive`
- `ShopEquipment`: `equipmentId`
- `ActiveCampaign`: `campaignId`, `startDay`, `endDay`

## Employee

- `id`, `typeId`, `name`
- `speed`, `friendliness`, `reliability`, `experience`, `salaryPerDay`
- `traits` as Dart strings such as `charmer`, `workaholic`, `loyal`
- `daysEmployed`, `growthPotential`
- `origin` as Dart string, including multi-word values such as `hiddenGem`,
  `topTalent`, `juniorPotential`, `exCompetitor`, `teamContact`
- `shift` as `ganztags`, `frueh`, `mittag`, `abend`

## Competitor, Loan, Brand

- `Competitor`: `id`, `name`, `cityId`, `personality`, `shopCount`,
  `reputation`, `priceLevel`, `marketShare`, `daysSinceLastAction`
- `personality` uses Dart strings including `cheapMass`, `balanced`,
  `premium`, `aggressive`, `traditional`
- `Loan`: `id`, `amount`, `interestRate`, `durationDays`, `dayTaken`,
  `amountPaid`
- `BrandStats`: `brandAwareness`, `cityReputation`

## Current Boundary

`SaveService` exposes an instance API for app code:

- `Serialize(GameState state)` returns JSON text.
- `Deserialize(string json)` returns a usable `GameState`.

The service serializes/deserializes strings only. It does not touch
`UnityEngine`, `PlayerPrefs`, files, UI, Buy/Shop/Cash mutation, GameEngine,
Day-Sim, arcade-cooking, or realtime serving logic.

## Verified Real-World Compatibility (2026-06-06)

`unity-logic-tests/.../FlutterSaveCompatibilityTests.cs` lädt eine
Flutter-Style-Fixture (`Fixtures/flutter_save_mvp.json`) und prüft 12
Aspekte — alle grün:

- Top-Level-Skalare (companyName, cash, currentDay, totalRevenue, …)
- `unlockedCityIds`, `globalUpgradeIds`, `achievementIds`, `seenEventIds`
- `brand` mit `brandAwareness` + `cityReputation`-Map
- Shop mit Menü, Equipment, Mitarbeiter, aktiven Kampagnen
- Konkurrent, Kredit, Mitarbeiter-Pool
- Difficulty-Enum-Mapping (Dart `"normal"` → C# `GameDifficulty.Normal`)
- Personality-Enum-Mapping (Dart `"business"` → `LocationPersonality.Business`)
- CompetitorPersonality (Dart `"cheapMass"` → `CheapMass`)
- CandidateOrigin (Dart `"hiddenGem"` → `HiddenGem`)

## Known Asymmetries (Stand 2026-06-06)

Diese Lücken sind dokumentiert UND in den Tests gepinnt — wenn der Test
`FlutterFixture_KnownGapsAreStillGaps_2026_06_06` rot wird, ist die
Lücke geschlossen.

### Felder, die Flutter NICHT in `toJson()` schreibt (aber C# erwartet)

C# muss beim Laden sinnvolle Defaults setzen, statt zu crashen:

| Modell | Feld | C#-Default beim Load |
|---|---|---|
| `Shop`     | `morale`     | `0.75` |
| `Shop`     | `regulars`   | `0.0`  |
| `Shop`     | `sizeTier`   | `"klein"` → `ShopSizeTier.Klein` |
| `Employee` | `shift`      | `"ganztags"` → `EmployeeShift.Ganztags` |

→ Beim Re-Save schreibt C# diese Felder mit; ein Flutter-Re-Load würde
sie ignorieren. Solange Spielstände nur in EINE Richtung wandern
(Flutter → Unity), ist das problemlos.

### Felder, die Flutter schreibt, C# (noch) NICHT roundtrippt

C# `SaveService` ignoriert diese aktuell stillschweigend — Daten gehen
beim Load verloren:

- `history` (Liste der `DailyRecord`s pro Tag)
- `missions`
- `stocks` (Aktien)
- `facilities` (Produktionsanlagen)
- `hrManager`, `hrStrategy`, `hrCandidates`
- `globalPrices`, `cityPrices`
- `activeCityCampaigns`, `activeGlobalCampaigns`
- `completedChapterIds`, `activeComboIds`, `productQuality`

**Codex-To-Do (M4–M6):** sobald die jeweilige Engine portiert ist
(z.B. CampaignEngine → activeGlobalCampaigns, MissionEngine → missions,
CorporateEngine → stocks/facilities), die DTO-Erweiterung gleich mit
hinzunehmen + Roundtrip-Test grünziehen.

### Fixture-Pfad

```
unity-logic-tests/DoenerEmpire.Logic.Tests/Fixtures/flutter_save_mvp.json
```

Die Fixture wird über `csproj` `<None CopyToOutputDirectory>` neben das
Test-Assembly kopiert. Wenn du ein echtes Flutter-Save als zusätzliche
Fixture beistellen willst (z.B. aus `adb shell run-as` der Android-App
exportiert), packe es daneben und füge einen weiteren Test hinzu.
