# STATUS

## Overall
Unity-Port laeuft. Logik-Layer wird verifiziert (`dotnet test`) portiert; UX-Spec
fuer den Vertical Slice ist geschrieben.

## Product Direction
Unity Management-/Progression-Spiel mit Premium 2.5D/3D City Map.
Arcade Cooking ist verworfen (`docs/UNITY_MVP_ARCADE_PLAN.md` = DEPRECATED).

## Claude Code (Planner/Reviewer)
State: review item completed by Codex, ready for Claude review (2026-06-05)
Done:
- Current Claude run 2026-06-05: Pflichtdateien gelesen; `REVIEW_QUEUE.md`
  war `Status: empty`, daher ein konkretes Codex-Review-Item formuliert:
  CityMap pre-step-4 presentation fixes (KPI-Labels Owned/Available und
  Locked-Tap nur Toast). Keine Code-Implementierung gestartet.
- Scope-Review: Richtung = Management-Spiel bestaetigt; Arcade-Plan deprecated.
- `docs/UNITY_CITY_MAP_UX.md` erstellt.
- Verifizierter C#-Logik-Port auf 86 gruene Tests.
- Review Codex CityMap Vertical Slice Schritte 1-3: BESTANDEN (HANDOFF_LOG #5, #11, #12).
  Alle vorherigen Flags (Kamerawinkel, xBounds) behoben; verbleibende Non-Blocker
  fuer Schritt 4: KPI-Labels (RUF/DRUCK statt MARKTANTEIL/PROGNOSE), Locked-Toast
  fehlt, kein Fokus-Tween, IMGUI statt UI Toolkit (erwartet Schritt 7).
- Bereit fuer Unity-Editor-Test: Bootstrap auto-fires, keine manuellen Scene-Objekte.
Next:
- Claude Code: Review der Codex-Umsetzung fuer CityMap pre-step-4 presentation
  fixes.
- Codex: Schritte 4-5 (BuyDialog, RestaurantDetail Sortiment/Ausbau) erst
  umsetzen, wenn Claude Review/Freigabe vorliegt.
- Claude Code: SaveService (JSON-Roundtrip, Dart-kompatibel) -> GameEngine-Tagessim.
- UI-Toolkit-Migration (IMGUI ersetzen) geplant fuer Schritt 7 (Premium-Polish).
- Vor Schritt 4: KPI-Labels korrigieren (Owned: MARKTANTEIL/PROGNOSE;
  Available: KONKURRENZ); Locked-Tap auf Toast-only umstellen.

## Codex (Implementation)
State: complete - CityMap pre-step-4 presentation fixes implemented, validation green (2026-06-05)
Done:
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap pre-step-4
  presentation fixes" umgesetzt:
  - `LocationSheetView` zeigt fuer Owned KPI 1 `MARKTANTEIL` und KPI 4
    `PROGNOSE`.
  - `LocationSheetView` zeigt fuer Available KPI 4 `KONKURRENZ`.
  - Locked-Hotspots bleiben Toast-only ueber `GameController.SelectLocation`;
    kein `LocationSelectedEvent`, kein Sheet-Wechsel auf locked.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
- Current Codex run #1327b5f0: Pflichtdateien gelesen; `REVIEW_QUEUE.md`
  ist weiterhin `Status: empty`, daher keine Code-Aenderungen vorgenommen.
  Management-Spiel-Richtung bestaetigt; keine Arcade-Cooking- oder
  Echtzeit-Serving-Systeme hinzugefuegt.
- Current Codex run #61: GameController/EventBus-Vertrag fuer den Unity
  Vertical Slice festgelegt:
  - `Core/EventBus.cs` als UnityEngine-freier Publish/Subscribe-Bus.
  - `App/GameController.cs` als zentrale Intent-Grenze fuer Location-Auswahl,
    BuyDialog-Anfrage, RestaurantDetail-Anfrage und Toasts.
  - `CityMapBootstrap` verdrahtet Selection -> GameController -> Events.
  - `LocationSheetView` hoert auf Controller-Events und feuert nur Intents;
    keine direkte State-Mutation in der UI.
  - `EventBusTests.cs` ergaenzt.
  - Keine Buy-/Upgrade-/Day-Sim-Wirtschaftslogik implementiert; keine
    Arcade-Cooking- oder Echtzeit-Serving-Systeme hinzugefuegt.
- Current Codex run #60: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #59: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #58: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #57: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #56: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #55: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #54: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #53: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #52: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #51: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #50: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #49: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #48: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #47: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #46: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #45: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #44: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #43: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #42: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #41: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #40: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #39: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #38: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #37: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #36: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #35: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #34: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #33: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #32: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #31: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #30: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #29: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #28: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #27: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #26: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #25: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #24: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #23: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #22: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #21: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #20: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #19: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #18: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #17: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #16: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #15: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
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
- Validation 2026-06-05 (Codex CityMap presentation fixes):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #1327b5f0):
  - `git status --short`
    -> clean before control-file updates.
  - `dotnet test unity-logic-tests/DoenerEmpire.Logic.Tests/DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #61):
  - `git status --short`
    -> existing worktree now contains the GameController/EventBus contract edits.
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #60):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #59):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #58):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #57):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #56):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG after
       control-file updates.
- Validation 2026-06-04 (Codex run #55):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #54):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #53):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-04 (Codex run #52):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #51):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #50):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #49):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #48):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #47):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #46):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #45):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #44):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-03 (Codex run #43):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #42):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #41):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #40):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #39):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #38):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #37):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #36):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #35):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #34):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #33):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #32):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #31):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #30):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #29):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #28):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #27):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #26):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #25):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #24):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #23):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #22):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-02 (Codex run #21):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #20):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #19):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #18):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #17):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-02 (Codex run #16):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #15):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
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
