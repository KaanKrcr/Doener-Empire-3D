# HANDOFF_LOG

## Initial
Agent-Control Workflow wurde angelegt.

Ziel:
Kaan plant mit Clemens, Clemens gibt klare Aufgaben an Claude Code und Codex weiter.

Binding Direction:
Management-Spiel wie Flutter behalten. Unity verbessert Map, Standortauswahl und Premium-UI.

## 2026-06-02 — Claude Code (Planner/Reviewer)
Kontext gelesen: CURRENT_DECISION, MASTER_PLAN, CLAUDE_TASK, STATUS, HANDOFF_LOG.
Git: dieser Checkout == origin/main (65dadf5); enthält den verifizierten C#-Port
(479dff7 ist Vorfahre) + Agent-Control-Workflow. Working Tree war sauber.

Erledigt:
- Scope-Review bestanden: Richtung Management-Spiel, Arcade verworfen.
- NEU: docs/UNITY_CITY_MAP_UX.md — vollständige Vertical-Slice-UX-Spec
  (CityMap, Hotspot-Zustände locked/available/owned/competitor, LocationSheet
  owned+frei nach Referenzbild, BuyDialog, RestaurantDetail-Tabs, Tag-simulieren +
  DayReport, Daten-Bindings an den C#-Port, Akzeptanzkriterien, Codex-Reihenfolge).
- Bestätigt: Logik-Port steht bei 79 grünen dotnet-Tests (siehe STATUS).

Exakter nächster Schritt:
- Codex: UNITY_CITY_MAP_UX.md §10 Schritte 1–3 (Map-Szene + Kamera + Hotspot-Zustände
  + LocationSheet gegen Dummy-State). Kein Arcade.
- Claude Code (nächster Resume): GameState-Aggregat portieren
  (MVP-Felder zuerst: Cash, CurrentDay/Hour, Shops, UnlockedCityIds, Difficulty,
  Competitors, Tutorial-Flags), dann SaveService-Roundtrip, dann GameEngine-Tagessim
  — jeweils mit dotnet test. Endgame-Untertypen (Stocks/Production/HrManager) später.

Hinweis: Es existieren mehrere lokale Klone des Repos
(C:\Users\Kaan\Doener-Empire-3D und C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D).
Kanonisch ist dieser (Documents\GitHub) mit .agent-control. Vor Arbeit immer
`git fetch` + auf origin/main syncen, um Divergenz zu vermeiden.

## 2026-06-02 (2) — Claude Code (Port fortgesetzt)
Arbeitsverzeichnis: NUR C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D.
Vorher: git fetch + checkout main + pull --ff-only (== origin/main, sauber).

Erledigt:
- MVP-`GameState` portiert (unity/Assets/Scripts/Models/GameState.cs):
  Kernfelder + Initial()-Factory (3 Startstädte, Theme klassik) + Helper-Getter
  (ShopCount/EmployeeCount/CompetitorsIn/HasShopIn/ActiveLoansTotal/Modifiers)
  + deep Clone. Endgame-Felder (Missions/Stocks/Facilities/HrManager/HrStrategy/
  Maps) bewusst als TODO zurückgestellt.
- +7 Tests → `dotnet test` 86 grün. Commit 1346d1a gepusht.

Exakter nächster Schritt (Claude Code):
- SaveService (JSON) für GameState + Untermodelle, Feldnamen/Enum-Strings EXAKT
  wie Flutter (Dart enum.name). Roundtrip-Test, idealerweise gegen ein echtes
  Flutter-Save-Fixture. Optional vorab docs/UNITY_SAVE_COMPAT.md.
- Danach GameEngine-Tagessimulation (calculateShopStats/Kapazität/Tagesabschluss)
  mit Tests gegen die Flutter-Erwartungswerte.
- Codex parallel: UNITY_CITY_MAP_UX.md §10 Schritte 1–3 (UI/Map gegen Dummy-State).
