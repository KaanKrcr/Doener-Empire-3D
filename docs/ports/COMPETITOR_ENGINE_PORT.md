# Port-Spec: CompetitorEngine (Dart → C#)

**Adressat:** Codex
**Reviewer:** Claude
**Meilenstein:** M4a (`UNITY_REWRITE_PLAN.md`)
**Vorlage:** `lib/services/competitor_engine.dart` (206 LOC)
**Zielort:** `unity/Assets/Scripts/Simulation/CompetitorEngine.cs`
**Test-Zielort:** `unity-logic-tests/DoenerEmpire.Logic.Tests/CompetitorEngineTests.cs`

---

## 1. Zweck

Steuert KI-Konkurrenz in jeder Stadt:
1. **Spawn** beim ersten Eröffnen einer Filiale in einer Stadt (1–3 + Difficulty-Bonus).
2. **Tägliches Verhalten:** Expansion, Preisanpassung, Reputationspflege —
   Aktion abhängig von `CompetitorPersonality` × Difficulty.
3. **Marktanteils-Verteilung** pro Stadt aus Reputation × Preisniveau × Filialdichte.
4. **Druckmaß** auf eine Spieler-Filiale (0.55–1.10).

---

## 2. Öffentliche API (verbindlich)

```csharp
namespace DoenerEmpire.Simulation
{
    public sealed class CompetitorEngine
    {
        // Constructor mit injizierbarem RNG für deterministische Tests.
        // Default: new Random() (nicht-seeded, wie Flutter).
        public CompetitorEngine(Random rng = null);

        // Spawn nur wenn noch keine Konkurrenten in der Stadt.
        // Mutiert nicht — gibt neue Liste zurück (Funktional, wie Flutter).
        public IReadOnlyList<Competitor> EnsureCompetitorsForCity(
            IReadOnlyList<Competitor> existing,
            string cityId,
            GameDifficulty difficulty);

        // Tägliches Update; mutiert die Competitor-Instanzen in state.Competitors
        // (Flutter macht das auch in-place). Gibt die Liste zurück, damit sie
        // testbar ist. Vor + nach Markt-Share-Berechnung pro Stadt.
        public IReadOnlyList<Competitor> ProcessDay(GameState state);

        // Druckmaß auf eine Spieler-Filiale.
        // Rückgabe ∈ [0.55, 1.10]; kleiner = mehr Druck.
        public double CompetitionPressure(GameState state, string cityId, double playerShopRep);
    }
}
```

**Wichtig:**
- Keine `static`-API → DI-tauglich. Flutter nutzt `static`, das ist für Unity
  unpraktisch (Test-Isolation, Seed-Injection).
- `IReadOnlyList<>` als Rückgabetyp signalisiert Immutability nach außen.
- `processDay` mutiert die existierenden `Competitor`-Instanzen — KEIN deep-clone,
  damit der GameState konsistent bleibt (wie Flutter).

---

## 3. Algorithmus (1:1 aus `competitor_engine.dart` portieren)

### 3.1 EnsureCompetitorsForCity

```
existingInCity = existing.Where(cityId)
if (existingInCity any) return existing

city = GameData.AllCities.First(c => c.Id == cityId) ?? Fallback first
count = city.Tier switch {
    Klein     => 1,
    Mittel    => 2,
    Gross     => 3,
    Metropole => 3 + rng.Next(2)
}
spawnBonus = difficulty.Modifiers.CompetitorAggressivenessMultiplier
extra = Math.Clamp(Round((spawnBonus - 1.0) * 2), 0, 2)
count += extra

for i in 0..count: new Competitor via CompetitorFactory.Create(
    id = $"comp_{cityId}_{microsecondsSinceEpoch}_{i}",
    cityId = cityId)
return existing + newCompetitors
```

**ID-Erzeugung:** Flutter nutzt `DateTime.now().microsecondsSinceEpoch`.
In C# äquivalent: `DateTimeOffset.UtcNow.Ticks` (100-ns-Auflösung — noch granularer).
Für Tests: nimm `IClock`-Interface oder akzeptiere Test-Drift (IDs müssen nicht
identisch zu Flutter sein — nur eindeutig).

### 3.2 ProcessDay

```
foreach c in state.Competitors:
    c.DaysSinceLastAction += 1
    MaybeAct(c, state, aggressiveness)

byCity = state.Competitors.GroupBy(CityId)
foreach (cityId, list) in byCity:
    RecomputeMarketShares(list, state, cityId)

return state.Competitors
```

### 3.3 MaybeAct (private)

```
minDays = Math.Clamp(Round(5.0 / aggressiveness), 2, 9)
if c.DaysSinceLastAction < minDays: return

baseActionChance = c.Personality switch {
    Aggressive  => 0.40,
    CheapMass   => 0.25,
    Balanced    => 0.18,
    Premium     => 0.15,
    Traditional => 0.10
}
actionChance = Math.Clamp(baseActionChance * aggressiveness, 0.05, 0.90)
if rng.NextDouble() > actionChance: return

c.DaysSinceLastAction = 0
r = rng.NextDouble()
expansionChance = Math.Clamp(0.30 * aggressiveness, 0.15, 0.55)
priceChance     = Math.Clamp(0.30 + (aggressiveness - 1.0) * 0.10, 0.20, 0.50)

if (r < expansionChance && c.ShopCount < 5):
    c.ShopCount = Math.Clamp(c.ShopCount + 1, 1, 5)
    c.Reputation = Math.Clamp(c.Reputation - 0.05, 1.0, 5.0)   # dilution
elif (r < expansionChance + priceChance):
    hasPlayer = state.HasShopIn(c.CityId)
    if (c.Personality == Aggressive && hasPlayer):
        c.PriceLevel = Math.Clamp(c.PriceLevel - 0.05, 0.65, 1.4)
    elif (c.Personality == Premium):
        c.PriceLevel = Math.Clamp(c.PriceLevel + 0.04, 0.65, 1.4)
    elif (c.Personality == CheapMass):
        c.PriceLevel = Math.Clamp(c.PriceLevel - 0.02, 0.65, 1.4)
    else:  # balanced/traditional
        c.PriceLevel = Math.Clamp(c.PriceLevel + (rng.NextDouble() - 0.5) * 0.04, 0.65, 1.4)
else:
    delta = (rng.NextDouble() - 0.45) * 0.20
    c.Reputation = Math.Clamp(c.Reputation + delta, 1.0, 5.0)
```

### 3.4 RecomputeMarketShares (private)

```
playerShops = state.Shops.Where(s => s.CityId == cityId)
playerPower = sum over playerShops: (s.Reputation / 5.0) * 1.0

compPower = sum over competitors:
    priceScore = Math.Clamp(2.0 - c.PriceLevel, 0.5, 1.5)
    return (c.Reputation / 5.0) * priceScore * c.ShopCount * aggressiveness

totalPower = playerPower + compPower
if (totalPower <= 0) return

foreach c in competitors:
    priceScore = Math.Clamp(2.0 - c.PriceLevel, 0.5, 1.5)
    p = (c.Reputation / 5.0) * priceScore * c.ShopCount * aggressiveness
    c.MarketShare = Math.Clamp(p / totalPower, 0.0, 1.0)
```

### 3.5 CompetitionPressure

```
inCity = state.Competitors.Where(CityId == cityId)
if (inCity empty) return 1.0

aggressiveness = state.Difficulty.Modifiers.CompetitorAggressivenessMultiplier
avgRivalRep = inCity.Average(c => c.Reputation)
repDelta = playerShopRep - avgRivalRep
density = inCity.Sum(c => c.ShopCount) / 3.0

pressure = 1.0 - (density * 0.05 * aggressiveness)
defenseFactor = Math.Clamp(1.0 / aggressiveness, 0.6, 1.4)
pressure += repDelta * 0.04 * defenseFactor
return Math.Clamp(pressure, 0.55, 1.10)
```

---

## 4. Abhängigkeiten

Diese sind bereits vorhanden:

- `DoenerEmpire.Models.Competitor`
- `DoenerEmpire.Models.GameState` (mit `Competitors`, `Shops`, `Difficulty`,
  `CompetitorsIn(cityId)`, `HasShopIn(cityId)` — Helper-Methoden ergänzen,
  falls noch nicht vorhanden)
- `DoenerEmpire.Models.DifficultyModel.Modifiers.CompetitorAggressivenessMultiplier`
- `DoenerEmpire.Core.CompetitorPersonality`
- `DoenerEmpire.Core.CityTier`
- `DoenerEmpire.Data.GameData.AllCities`
- `DoenerEmpire.Models.CompetitorFactory` (existiert in `Competitor.cs`)

**Prüfen vor Port:** Gibt es `GameState.CompetitorsIn(string cityId)` und
`GameState.HasShopIn(string cityId)`? Wenn nicht, als private Extensions oder
direkt in `GameState.cs` ergänzen (kleiner chirurgischer Edit, KEINE
Verhaltensänderung).

---

## 5. Tests (Pflicht, Mindest-Coverage)

```csharp
public class CompetitorEngineTests
{
    // — EnsureCompetitorsForCity —
    [Fact] void EnsureSpawns1ForKleinTier();
    [Fact] void EnsureSpawns2ForMittelTier();
    [Fact] void EnsureSpawns3ForGrossTier();
    [Theory] void EnsureSpawnsExtraForHigherDifficulty();    // easy=0, hard/imp=+1..+2
    [Fact] void EnsureIsIdempotentWhenCityAlreadyHasCompetitors();

    // — ProcessDay —
    [Fact] void ProcessDayIncrementsDaysSinceLastAction();
    [Fact] void ProcessDayDoesNotActBeforeMinDays();
    [Fact] void ProcessDayRecomputesMarketSharesWithinClampRange();
    [Fact] void ProcessDayMarketSharesSumPlusPlayerPowerEqualsApprox1WhenNonZero();

    // — MaybeAct (über ProcessDay mit seeded RNG) —
    [Fact] void AggressiveCompetitorLowersPriceWhenPlayerPresent();
    [Fact] void PremiumCompetitorRaisesPrice();
    [Fact] void CheapMassCompetitorLowersPriceSlightly();
    [Fact] void ExpansionIncrementsShopCountAndLowersReputation();
    [Fact] void ExpansionCapsAtFiveShops();
    [Fact] void PriceLevelStaysWithin_0_65_to_1_4_Bounds();
    [Fact] void ReputationStaysWithin_1_0_to_5_0_Bounds();

    // — CompetitionPressure —
    [Fact] void PressureIs1WhenNoCompetitors();
    [Fact] void PressureDecreasesWithCompetitorDensity();
    [Fact] void PressureIncreasesWhenPlayerRepIsHigherThanRivals();
    [Fact] void PressureStaysWithin_0_55_to_1_10_Bounds();
    [Fact] void PressureLowerOnImpossibleVsEasy();   // spiegelt difficulty_system_test.dart:92-94
}
```

**Seed-Strategie:** alle Tests, die RNG-abhängiges Verhalten prüfen, injizieren
`new Random(42)` (oder einen anderen festen Seed) in `new CompetitorEngine(rng)`.
.NET 6+ garantiert plattformstabile Sequenz bei gesetztem Seed → keine
Plattform-Drift.

---

## 6. Surgical-Edits-Regel

**Erlaubt im Rahmen dieses Ports:**
- Anlegen `CompetitorEngine.cs` und `CompetitorEngineTests.cs`.
- Falls fehlend: `GameState.CompetitorsIn(cityId)` / `GameState.HasShopIn(cityId)`
  als kleine Helper-Methoden in `GameState.cs` ergänzen.
- Falls fehlend: `CompetitorFactory.Create(id, cityId)`-Overload in `Competitor.cs`.

**Nicht erlaubt:**
- Ändern der existierenden 115 grünen Tests.
- Refactoring von Models/Save/GameEngine außerhalb des oben Genannten.
- Einführen neuer Abhängigkeiten / Packages.
- UnityEngine-Referenzen in `Simulation/` (`DoenerEmpire.Simulation.asmdef`
  enforced `noEngineReferences: true` — würde Build brechen).

---

## 7. Definition of Done

- [ ] `unity/Assets/Scripts/Simulation/CompetitorEngine.cs` existiert
- [ ] `unity-logic-tests/DoenerEmpire.Logic.Tests/CompetitorEngineTests.cs` existiert
- [ ] Alle in §5 gelisteten Tests grün
- [ ] Alle 115 Bestands-Tests weiter grün
- [ ] `dotnet test` Gesamtanzahl ≥ 130 grün
- [ ] CI-Workflow `unity-logic-tests.yml` grün
- [ ] Kurze Review-Notiz in `findings.md` unter „M4a CompetitorEngine portiert"
- [ ] Bei Bedarf: `UNITY_TASK_BOARD.md` Eintrag 1.9 → 🟢
