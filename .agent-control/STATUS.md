# STATUS

## Overall
Unity-Port laeuft. Logik-Layer wird verifiziert (`dotnet test`) portiert; UX-Spec
fuer den Vertical Slice ist geschrieben.

## Product Direction
Unity Management-/Progression-Spiel mit Premium 2.5D/3D City Map.
Arcade Cooking ist verworfen (`docs/UNITY_MVP_ARCADE_PLAN.md` = DEPRECATED).

## Claude Code (Planner/Reviewer)
State: review done (2026-06-02, #12)
Done:
- Scope-Review: Richtung = Management-Spiel bestaetigt; Arcade-Plan deprecated.
- `docs/UNITY_CITY_MAP_UX.md` erstellt.
- Verifizierter C#-Logik-Port auf 86 gruene Tests.
- Review Codex CityMap Vertical Slice Schritte 1-3: BESTANDEN (HANDOFF_LOG #5, #11, #12).
  Alle vorherigen Flags (Kamerawinkel, xBounds) behoben; verbleibende Non-Blocker
  fuer Schritt 4: KPI-Labels (RUF/DRUCK statt MARKTANTEIL/PROGNOSE), Locked-Toast
  fehlt, kein Fokus-Tween, IMGUI statt UI Toolkit (erwartet Schritt 7).
- Bereit fuer Unity-Editor-Test: Bootstrap auto-fires, keine manuellen Scene-Objekte.
Next:
- Codex: Schritte 4-5 (BuyDialog, RestaurantDetail Sortiment/Ausbau) erst
  umsetzen, wenn GameController/EventBus abgestimmt ist.
- Claude Code: SaveService (JSON-Roundtrip, Dart-kompatibel) -> GameEngine-Tagessim.
- UI-Toolkit-Migration (IMGUI ersetzen) geplant fuer Schritt 7 (Premium-Polish).
- Vor Schritt 4: KPI-Labels korrigieren (Owned: MARKTANTEIL/PROGNOSE;
  Available: KONKURRENZ); Locked-Tap auf Toast-only umstellen.

## Codex (Implementation)
State: complete - review queue empty, validation green (2026-06-02, current run)
Done:
- Current Codex run: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run: `REVIEW_QUEUE.md` was already `Status: empty`; no open
  Claude review items were present to implement.
- Restored the missing `LocationPersonality` extension import in
  `lib/ui/screens/open_shop_screen.dart` so the requested validation passes.
- `docs/UNITY_CITY_MAP_UX.md` Section 10 Schritte 1-3 umgesetzt:
  `unity/Assets/Scenes/CityMap.unity`, Runtime-Bootstrap, isometrische Kamera
  mit Pan/Zoom, statische Hotspots mit `owned`, `available`, `locked`,
  `competitor`, und read-only `LocationSheet` gegen Dummy/current State.
- Nachpruefung: `CityMapView` nutzt den Bootstrap-`GameState` fuer eigene
  Filiale/Konkurrenzdaten; Hotspot-Auswahl erhaelt die urspruengliche Marker-Skalierung.
- Mini-Fix 2026-06-02: `LocationSheetView.Initialize` nimmt den aktuellen
  `GameState` entgegen; HUD zeigt Company/Cash/Tag aus dem Dummy/current State.
- Kamerawinkel und Bounds verifiziert: Bootstrap nutzt Euler(30, 45, 0),
  `CityMapCameraController.xBounds` ist auf [-6, 6] erweitert.
- `lib/ui/screens/open_shop_screen.dart` importiert wieder die bestehende
  `LocationPersonality`-Extension, damit `flutter analyze` gruen ist.
- Keine Buy-/Upgrade-/Simulate-Day-Aktionen implementiert; Buttons sind bewusst
  nicht interaktiv und verweisen auf GameController/Intent-Folgearbeit.
- Keine Arcade-Cooking- oder Echtzeit-Serving-Systeme hinzugefuegt.
- Revalidation 2026-06-02: Scope 1-3 gegen aktuelle lokale Dateien geprueft;
  keine zusaetzlichen Code-Aenderungen noetig.
- Verification 2026-06-02 (current Codex run): vorhandene CityMap-Implementierung
  deckt den angeforderten Scope weiter ab; keine zusaetzlichen CityMap-Code-Aenderungen
  noetig.
Next:
- Review der Map-/Sheet-Praesentation in Unity.
- Danach erst Section 10 Schritt 4+ (BuyDialog/echte State-Mutation), wenn
  GameController/EventBus-Anbindung abgestimmt ist.

## Last Validation
- `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
  -> 86 bestanden, 0 Fehler (net8.0).
- `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
  -> No issues found.
- `git diff --check` clean (nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG,
  `open_shop_screen.dart` und Flutter-Windows-Generated-Dateien).
- Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
  Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien clean.
- Flutter-Spiellogik unveraendert; nur fehlender UI-Import fuer Analyzer repariert.
- Revalidation 2026-06-02:
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Revalidation 2026-06-02 (current Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
- Revalidation 2026-06-02 (latest Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Verification 2026-06-02 (this Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Validation 2026-06-02 (review queue empty run):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer Flutter-Windows-Generated-Dateien.
- Validation 2026-06-02 (current review queue empty run):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
