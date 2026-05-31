# Gameplay Tuning Brief — Staff Cap, Burnout & Difficulty

## Ziel
Zwei aktuelle Balancing-Probleme sollen behoben werden:

1. **Personal-Cap skaliert zu hart**: Bei steigender Popularität läuft eine Filiale zu schnell in Kapazitäts-/Personalprobleme. Burnout/Überlastung darf nicht automatisch der Punkt sein, an dem ein erfolgreicher Laden abschmiert.
2. **Schwierigkeitsstufen fühlen sich gleich an**: Zwischen Einfach und Unmöglich muss besonders beim Konkurrenzverhalten ein klar spürbarer Unterschied entstehen.

## Current Balance Model
Aktueller Code-Stand laut Analyse:

- `GameEngine.maxEmployeesForShop(shop)` ist statisch nach CityTier:
  - Kleinstadt: 3
  - Mittelstadt: 5
  - Großstadt: 7
  - Metropole: 10
- Es gibt noch kein klares Filial-Erweiterungssystem, das den Mitarbeiter-Cap erhöht.
- Konkurrenz nutzt `competitorAggressivenessMultiplier`, aber die Unterschiede wirken vermutlich zu schwach:
  - Easy: 0.75
  - Normal: 1.00
  - Hard: 1.20
  - Impossible: 1.45
- Konkurrenten-Aktionen unterscheiden sich aktuell hauptsächlich über Spawn-Bonus, Aktionstimer, Aktionchance und Marktanteilsgewichtung.

## Key Variables
### Filialgröße / Staff Cap
Neue oder explizit konfigurierbare Werte:

- `shop.expansionLevel` oder `shop.sizeLevel`
- `shopExpansionCapacityBonus`
- `shopExpansionEmployeeCapBonus`
- `shopExpansionCost`
- `shopExpansionRentMultiplier`
- `shopExpansionBuildDays` optional später

### Burnout / Überlastung
Falls Burnout existiert oder eingeführt wird:

- `capacityUtilization`
- `overloadThreshold`
- `burnoutRiskPerDay`
- `burnoutRiskDifficultyMultiplier`
- `burnoutMitigationFromExpansion`
- `burnoutMitigationFromManagers`

### Difficulty / Competition
Konfigurierbare Werte:

- `competitorSpawnCountBonus`
- `competitorActionIntervalDays`
- `competitorActionChance`
- `competitorExpansionChance`
- `competitorPriceWarChance`
- `competitorReputationGrowth`
- `competitorMaxShopsPerCity`
- `competitionPressureMin`

## First-Pass Tables

## 1. Filial-Erweiterung / Staff Cap

Jede Filiale bekommt eine Größe. Größe erhöht Mitarbeiter-Cap, Kapazität und Miete.

| Size Level | Name | Staff Cap Bonus | Capacity Bonus | Rent Multiplier | Upgrade Cost | Purpose |
|---:|---|---:|---:|---:|---:|---|
| 1 | Kleiner Imbiss | +0 | +0% | 1.00x | 0 € | Startzustand |
| 2 | Erweiterter Laden | +2 | +20% | 1.15x | 8.000 € | Erste Popularitätsprobleme lösen |
| 3 | Großer Laden | +4 | +45% | 1.35x | 25.000 € | Erfolgreiche Stadtfiliale stabilisieren |
| 4 | Flagship-Filiale | +6 | +75% | 1.70x | 75.000 € | Metropole/Endgame |

Base Cap bleibt nach Stadtgröße, aber wird erweitert:

| City Tier | Base Staff Cap | Level 2 | Level 3 | Level 4 |
|---|---:|---:|---:|---:|
| Kleinstadt | 3 | 5 | 7 | 9 |
| Mittelstadt | 5 | 7 | 9 | 11 |
| Großstadt | 7 | 9 | 11 | 13 |
| Metropole | 10 | 12 | 14 | 16 |

## 2. Burnout / Overload

Burnout sollte Warnsignal sein, nicht Todesurteil.

| Utilization | Meaning | Effect |
|---:|---|---|
| 0–80% | Gesund | Kein Burnout-Risiko |
| 80–95% | Hohe Auslastung | Kleine Warnung, keine harte Strafe |
| 95–110% | Überlastung | Burnout-Risiko + leichte Rep-Bremse |
| 110%+ | Kritisch | deutlicher Burnout-/Kündigungs-/Rep-Druck |

Empfohlene Formel:

```text
utilization = potentialCustomers / max(1, capacity)
```

Burnout-Risiko nur bei `utilization > 0.95`:

```text
burnoutRisk = (utilization - 0.95) * 0.12 * difficultyBurnoutMultiplier
```

Difficulty Burnout Multiplier:

| Difficulty | Multiplier |
|---|---:|
| Easy | 0.35 |
| Normal | 1.00 |
| Hard | 1.45 |
| Impossible | 2.10 |

Wichtig: Expansion reduziert nicht direkt Burnout, sondern erhöht Kapazität und Staff Cap. Dadurch sinkt Utilization natürlich.

## 3. Difficulty Tuning — Competition

Die Schwierigkeit muss beim Konkurrenzverhalten sehr stark spürbar sein.

| Difficulty | Spawn Count | Action Interval | Action Chance | Expansion Chance | Price War | Max Shops | Pressure Min |
|---|---:|---:|---:|---:|---:|---:|---:|
| Easy | -1 | 8–12 Tage | 35% | 10% | 5% | 3 | 0.80 |
| Normal | 0 | 5–8 Tage | 55% | 25% | 15% | 5 | 0.65 |
| Hard | +1 | 3–5 Tage | 75% | 40% | 30% | 7 | 0.52 |
| Impossible | +2 | 1–3 Tage | 90% | 60% | 50% | 10 | 0.40 |

## Pacing Curve

### Early Game
- Staff Cap darf limitieren, aber nicht bestrafen.
- Spieler soll lernen: Mehr Popularität → mehr Kapazitätsbedarf → Laden erweitern.
- Erste Erweiterung sollte nach 7–14 Spieltagen erreichbar sein.

### Mid Game
- Expansion wird strategische Entscheidung:
  - erweitern = mehr Cap, höhere Miete
  - zweite Filiale = mehr Marktabdeckung, mehr Management
  - Marketing = mehr Nachfrage, aber Risiko von Überlastung

### Late Game
- Große Filialen, Manager und HR-Systeme müssen nötig werden.
- Auf Hard/Impossible reagiert Konkurrenz aggressiv auf erfolgreiche Städte.

## Risky Numbers
- Wenn Staff Cap zu niedrig bleibt, fühlt sich Erfolg wie Strafe an.
- Wenn Expansion zu billig ist, ignoriert der Spieler Standortstrategie.
- Wenn Burnout zu früh greift, wird Popularität negativ wahrgenommen.
- Wenn Impossible nur 45% aggressiver ist, fühlt es sich kaum anders an.
- Wenn Easy Konkurrenzdruck unter 0.8 fällt, kann Anfängerfrust entstehen.

## Exploit / Degenerate Strategy Check
Zu prüfen:

1. Kann Spieler mit billigen kleinen Filialen Expansion komplett ignorieren?
2. Ist es immer besser, Personal statt Equipment/Expansion zu kaufen?
3. Werden Metropolen durch hohe Staff Caps zu leicht?
4. Kann man auf Easy Preise maximal erhöhen, weil Konkurrenz egal ist?
5. Wird Impossible unfair, weil Konkurrenz zu schnell expandiert, bevor Spieler reagieren kann?

## Recommended Tuning Changes

### A. Shop Expansion System einführen
Minimaler MVP:

- `Shop` bekommt `sizeLevel` mit Default `1`.
- Savegame-Migration: fehlendes `sizeLevel` → `1`.
- `GameEngine.maxEmployeesForShop(shop)` addiert Bonus aus `sizeLevel`.
- Kapazitätsberechnung bekommt Bonus aus `sizeLevel`.
- Tagesmiete oder Upgrade-Kosten berücksichtigen Größe.
- Provider-Methode: `expandShop(String shopId)`.
- UI minimal: Button in ShopDetailScreen oder später Premium Bottom Sheet.

### B. Burnout entschärfen / an Utilization koppeln
- Burnout darf erst bei echter Überlastung greifen.
- Popularität allein darf keinen Laden zerstören.
- Kapazitätswarnungen müssen vor Burnout kommen.

### C. DifficultyConfig expliziter machen
Nicht nur Multiplikator. Besser ein eigenes Config-Objekt:

```dart
class CompetitorDifficultyConfig {
  final int spawnCountBonus;
  final int minActionIntervalDays;
  final int maxActionIntervalDays;
  final double actionChanceMultiplier;
  final double expansionChance;
  final double priceWarChance;
  final int maxShopsPerCompetitor;
  final double competitionPressureMin;
}
```

### D. Tests ergänzen
Mindestens:

- Easy spawnt weniger/handelt seltener als Impossible.
- Impossible kann mehr Konkurrentenfilialen aufbauen.
- Expansion erhöht Staff Cap.
- Expansion erhöht Kapazität.
- Burnout/Overload-Risk unter 80–95% bleibt niedrig/aus.

## Playtest Metrics
Beim Testen loggen/beobachten:

- Tage bis erste Kapazitätswarnung
- Tage bis erste sinnvolle Filialerweiterung
- Lost customers durch Capacity Limit
- Durchschnittliche Utilization pro Shop
- Burnout-/Kündigungsfälle pro 30 Tage
- Competitor actions pro 30 Tage nach Difficulty
- Player market share nach 30/60/90 Tagen pro Difficulty

## Open Questions
- Soll Filialerweiterung sofort passieren oder Bauzeit haben?
- Soll Erweiterung visuell auf City Map sichtbar werden?
- Soll Staff Cap eher von Fläche, Ausstattung oder Manager-Level abhängen?
- Soll Burnout einzelne Mitarbeiter treffen oder die Filiale als Stresswert?
- Soll Impossible fair aber hart sein oder bewusst challenge-/roguelike-artig?
