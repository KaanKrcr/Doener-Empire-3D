# STATUS

## Overall
Unity-Port läuft. Logik-Layer wird verifiziert (dotnet test) portiert; UX-Spec für
den Vertical Slice ist geschrieben.

## Product Direction
Unity Management-/Progression-Spiel mit Premium 2.5D/3D City Map.
Arcade Cooking ist verworfen (docs/UNITY_MVP_ARCADE_PLAN.md = DEPRECATED).

## Claude Code (Planner/Reviewer)
State: ready
Done:
- Scope-Review: Richtung = Management-Spiel bestätigt; Arcade-Plan deprecated.
- docs/UNITY_CITY_MAP_UX.md erstellt (Vertical-Slice-UX: Map → Standort →
  Bottom-Sheet → kaufen/upgraden → Tag simulieren → Bericht, nach Referenzbild).
- Verifizierter C#-Logik-Port (unity/Assets/Scripts/) auf 79 grüne Tests:
  Enums, Difficulty, Competitor, ShopSizeTier, GameData, GameCatalog,
  ShopProduct, ShopEquipment, Employee(+Factory), Marketing, Shop, Loan,
  DailyRecord, BrandStats.
Next:
- Review von Codex-Vertical-Slice-Schritten.
- Bei Bedarf docs/UNITY_SAVE_COMPAT.md schreiben (JSON-Feld-/Enum-Mapping).
- Port fortsetzen: GameState-Aggregat → SaveService → GameEngine-Tagessim.

## Codex (Implementation)
State: ready
Next: Vertical Slice gemäß docs/UNITY_CITY_MAP_UX.md §10, Schritte 1–3
(CityMap-Szene + Kamera Pan/Zoom + Hotspot-Zustände + LocationSheet gegen
Dummy/aktuellen State). Schritte 4–6 erst nach GameState/GameEngine-Port.
Kein Arcade. dotnet test grün halten.

## Last Validation
unity-logic-tests: `dotnet test` → 79 bestanden, 0 Fehler (net8.0).
Flutter-Spiellogik unverändert.
