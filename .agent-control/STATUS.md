# STATUS

## Overall
Unity-Port laeuft. Logik-Layer wird verifiziert (`dotnet test`) portiert; UX-Spec
fuer den Vertical Slice ist geschrieben.

## Product Direction
Unity Management-/Progression-Spiel mit Premium 2.5D/3D City Map.
Arcade Cooking ist verworfen (`docs/UNITY_MVP_ARCADE_PLAN.md` = DEPRECATED).

## Claude Code (Planner/Reviewer)
State: reviewed SaveService foundation; queued SaveService API/fixture polish for Codex (2026-06-05)
Done:
- Current Claude run 2026-06-05 11:04: reviewed commit `1ca92f3`
  ("Add Unity save service foundation"). Result: accepted as foundation.
  The commit adds `docs/UNITY_SAVE_COMPAT.md`, a UnityEngine-free
  `SaveService` JSON string roundtrip for the current MVP `GameState`, links the
  `Save` folder into the logic test project, and adds focused tests for
  non-trivial state roundtrip plus Dart-compatible enum strings. No PlayerPrefs,
  filesystem persistence, UI mutation, Buy/Upgrade, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Current worktree already
  contains uncommitted SaveService polish; it was not reverted. Since
  `REVIEW_QUEUE.md` was `Status: empty`, a new concrete Codex review item was
  queued: SaveService API hardening + fixture-shaped JSON.
- Current Claude run 2026-06-05 10:59: reviewed commit `f61dc4d`
  ("Add Unity restaurant detail shell"). Result: accepted.
  `RestaurantDetailView` is presentation-only, opens from
  `RestaurantDetailRequestedEvent`, looks up the owned shop from `GameState.Shops`
  via the hotspot/shop id, and is wired through `CityMapBootstrap` using the
  existing `GameController`/`EventBus`. It shows shop/location identity and
  read-only `Sortiment`, `Ausbau`, `Equipment`, `Personal`, `Marketing`
  sections; close and a new location selection only close the detail shell.
  No price, Shop, SizeTier, Cash, Buy, SaveService, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Since `REVIEW_QUEUE.md`
  was `Status: empty`, a new concrete Codex review item was queued:
  Unity SaveService compatibility foundation.
- Current Claude run 2026-06-05 10:55: reviewed commit `1591e9d`
  ("Add Unity city map buy dialog shell"). Result: accepted. `BuyDialogView`
  is presentation-only, opens from `BuyDialogRequestedEvent`, is wired through
  `CityMapBootstrap` using the existing `GameController`/`EventBus`, and keeps
  the confirm action disabled as `NOCH NICHT AKTIV`. It displays Standort,
  Lage/Stadtteil, Kaution, Wochenmiete and Kapital nach Kaution from existing
  hotspot/state data. Abbrechen and a new non-locked Location selection close
  only the dialog. No Buy-/Cash-/Shop-mutation, RestaurantDetail implementation,
  Upgrade, SaveService, GameEngine/Day-Sim, Arcade-Cooking or realtime Serving
  logic was added. Since `REVIEW_QUEUE.md` was `Status: empty`, a new concrete
  Codex review item was queued: CityMap RestaurantDetail shell via
  GameController.
- Current Claude run 2026-06-05 10:50: reviewed commit `e7a1b48`
  ("Add Unity city map focus tween"). Result: accepted. Camera focus tween is
  presentation-only, is driven by `LocationSelectedEvent`, clamps through the
  existing x/z camera bounds, and manual pan cancels the tween. Locked hotspots
  remain Toast-only through `GameController.SelectLocation`; no Buy-State-
  mutation, RestaurantDetail, Upgrade, SaveService, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Since `REVIEW_QUEUE.md`
  was `Status: empty`, a new concrete Codex review item was queued: CityMap
  BuyDialog shell via GameController.
- Current Claude run 2026-06-05 10:46: reviewed commit `1af4408`
  ("Polish Unity available KPI order"). Result: accepted. Available KPI order
  now matches `UNITY_CITY_MAP_UX.md` section 3.2 (`TRAFFIC`, `MIETE`,
  `KAUTION`, `KONKURRENZ`); Owned order remains `MARKTANTEIL`, `TRAFFIC`,
  `MIETE`, `PROGNOSE`; locked selection remains Toast-only through
  `GameController`; no Arcade/Serving/Buy/Upgrade/SaveService/Day-Sim logic was
  added. Since `REVIEW_QUEUE.md` was `Status: empty`, a new concrete Codex
  review item was queued: CityMap selection focus tween.
- Current Claude run 2026-06-05 06:52: reviewed commit `333ca76`
  ("Fix Unity city map presentation queue item"). Result: previous item accepted:
  owned KPI labels are `MARKTANTEIL`/`PROGNOSE`, available KPI 4 is
  `KONKURRENZ`, locked selection remains Toast-only through `GameController`,
  and no Arcade/Serving/Buy/Upgrade/Day-Sim logic was added. One small UX polish
  remains: available KPI tile order should match `UNITY_CITY_MAP_UX.md` section
  3.2 (`TRAFFIC`, `MIETE`, `KAUTION`, `KONKURRENZ`), so `REVIEW_QUEUE.md` was
  opened with that concrete item for Codex.
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
- Codex: Nur das offene Queue-Item "Unity SaveService API hardening +
  fixture-shaped JSON" umsetzen.
- Claude Code: Danach SaveService-Polish reviewen und erst dann GameEngine-
  Tagessim freigeben.
- UI-Toolkit-Migration (IMGUI ersetzen) geplant fuer Schritt 7 (Premium-Polish).
- Vor Schritt 4: KPI-Labels korrigieren (Owned: MARKTANTEIL/PROGNOSE;
  Available: KONKURRENZ); Locked-Tap auf Toast-only umstellen.

## Codex (Implementation)
State: complete - Unity SaveService API/fixture polish implemented, validation green (2026-06-05)
Done:
- Current Codex run 2026-06-05 16:06: Offenes Review-Item "Unity SaveService
  API hardening + fixture-shaped JSON" umgesetzt:
  - `SaveService` bleibt eine UnityEngine-freie Instanz-API mit
    `Serialize(GameState)` / `Deserialize(string)` und string-in/string-out.
  - `docs/UNITY_SAVE_COMPAT.md` dokumentiert die Instanz-API zusaetzlich zum
    aktuellen MVP-Feldvertrag und Dart-kompatiblen Enum-Strings.
  - `SaveServiceTests` pruefen jetzt explizit lower-camelCase JSON und keine
    PascalCase-Modellnamen fuer zentrale Felder.
  - Nicht-triviale Roundtrip-Abdeckung umfasst Shops, Menu, Equipment,
    Employees, EmployeePool, Competitors, Loans, Brand, ids/upgrades sowie
    Tutorial-/Event-Felder.
  - Missing/null optionale Collections werden ohne Exception zu nutzbaren
    leeren Collections/defaults deserialisiert.
  - Guardrail-Scan fuer SaveService ist frei von UnityEngine, PlayerPrefs,
    Dateisystem, GameEngine/Day-Sim, BuyDialog, Arcade und Serving.
- Current Codex run 2026-06-05 11:00: Offenes Review-Item "Unity SaveService
  compatibility foundation" umgesetzt:
  - `docs/UNITY_SAVE_COMPAT.md` mit Mapping fuer den aktuell portierten
    MVP-`GameState` und Untermodelle angelegt.
  - `unity/Assets/Scripts/Save/SaveService.cs` als UnityEngine-freier
    String-JSON-Roundtrip fuer `GameState` hinzugefuegt.
  - JSON-Feldnamen sind lower camelCase; Enum-Werte werden ueber vorhandene
    `EnumNames`/`EmployeeEnumNames` als Dart-kompatible Strings gemappt.
  - Save-Code in das Unity-Logik-Testprojekt eingebunden.
  - Fokussierte Roundtrip-/Enum-String-Tests fuer Shop/Menu/Equipment/
    Employees/SizeTier, Competitors, Loans, Difficulty und BrandStats ergaenzt.
  - Keine PlayerPrefs-/Dateisystem-Persistenz, keine Buy-/Cash-/Shop-Mutation,
    keine Preis-/Ausbau-Aktion, keine GameEngine-/Day-Sim- und keine
    Arcade-Cooking-/Realtime-Serving-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap RestaurantDetail
  shell via GameController" umgesetzt:
  - `RestaurantDetailView` als presentation-only IMGUI-Shell fuer
    `RestaurantDetailRequestedEvent` hinzugefuegt.
  - `CityMapBootstrap` verdrahtet die Detail-UI ueber den bestehenden
    `GameController`/`EventBus`.
  - Owned-CTA `OPTIMIEREN` bleibt ueber
    `GameController.RequestRestaurantDetail(selected)`.
  - Shell oeffnet nur fuer einen gueltigen Shop aus `GameState.Shops` und zeigt
    Shop-/Location-Identitaet.
  - Tabs/Sektionen fuer `Sortiment`, `Ausbau`, `Equipment`, `Personal` und
    `Marketing`; Stubs sind read-only markiert.
  - Close/Zurueck und neue Location-Auswahl schliessen nur die Detail-Shell.
  - Keine Preis-, Shop-, SizeTier-, Cash-, Buy-, SaveService-, Day-Sim/
    GameEngine- oder Arcade-Cooking-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap BuyDialog shell
  via GameController" umgesetzt:
  - `BuyDialogView` als IMGUI-Shell fuer `BuyDialogRequestedEvent`
    hinzugefuegt.
  - `CityMapBootstrap` verdrahtet die BuyDialog-UI ueber den bestehenden
    `GameController`/`EventBus`.
  - Available-Hotspots behalten das LocationSheet; `FILIALE EROEFFNEN`
    oeffnet den Dialog ueber den Controller-Intent.
  - Dialog zeigt Standortname, Lage/Stadt, Kaution, Wochenmiete und Kapital
    nach Kaution aus vorhandenen Hotspot-/State-Daten.
  - Abbrechen oder neue Location-Auswahl schliesst nur den Dialog.
  - Confirm ist sichtbar, aber deaktiviert/als "NOCH NICHT AKTIV" markiert.
  - Keine Buy-/Cash-/Shop-Mutation, kein RestaurantDetail, kein Upgrade, kein
    SaveService, keine Day-Sim/GameEngine- oder Arcade-Cooking-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap selection focus
  tween" umgesetzt:
  - `CityMapCameraController` kann per `FocusOn(Vector3)` weich auf einen
    Hotspot fokussieren und nutzt die bestehenden x/z-Bounds.
  - `CityMapBootstrap` verbindet `LocationSelectedEvent` mit dem
    Camera-Controller, damit nur nicht-locked Selections fokussieren.
  - Locked-Hotspots bleiben Toast-only ueber `GameController.SelectLocation`;
    kein `LocationSelectedEvent`, kein LocationSheet-Wechsel.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap available KPI
  order polish" umgesetzt:
  - `LocationSheetView` rendert KPI-Kacheln jetzt zentral ueber
    `MetricLabel(index)` / `MetricValue(index)`.
  - Available-Reihenfolge ist `TRAFFIC`, `MIETE`, `KAUTION`, `KONKURRENZ`.
  - Owned-Reihenfolge bleibt `MARKTANTEIL`, `TRAFFIC`, `MIETE`, `PROGNOSE`.
  - Competitor/locked Output bleibt erhalten; Locked-Auswahl bleibt Toast-only.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
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
- Validation 2026-06-05 16:06 (Codex SaveService API/fixture polish):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 92 bestanden, 0 Fehler.
  - `rg "UnityEngine|PlayerPrefs|File\.|Directory\." unity/Assets/Scripts/Save -n`
    -> keine Treffer.
  - `rg "GameEngine|EndDay|Simulate|BuyDialog|Arcade|Serving|CustomerSpawner|ServeInteraction" unity/Assets/Scripts/Save unity-logic-tests/DoenerEmpire.Logic.Tests/SaveServiceTests.cs -n`
    -> keine Treffer.
- Validation 2026-06-05 11:06 (Claude review of commit 1ca92f3):
  - `dotnet clean unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj; dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 90 bestanden, 0 Fehler on the current worktree.
  - Scope scan in `unity/Assets/Scripts` / `docs/UNITY_SAVE_COMPAT.md` for
    Arcade/Serving/Buy/GameEngine/Day-Sim/filesystem terms -> no forbidden
    Save implementation; expected existing BuyDialog presentation hits outside
    Save and boundary wording in docs.
- Validation 2026-06-05 11:00 (Codex Unity SaveService compatibility foundation):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 90 bestanden, 0 Fehler.
- Validation 2026-06-05 10:59 (Claude review of commit f61dc4d):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/
    RestaurantDetail/GameEngine/SaveService/Day-Sim terms -> no forbidden
    implementation; only expected controller intent/presentation shell hits and
    existing model fields/comments.
- Validation 2026-06-05 (Codex CityMap RestaurantDetail shell):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/
    RestaurantDetail/GameEngine/SaveService/Day-Sim terms -> only expected
    controller intent/presentation shell hits and existing model fields/comments.
- Validation 2026-06-05 10:55 (Claude review of commit 1591e9d):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no forbidden implementation; only expected
    BuyDialog/controller intent hits and existing model fields/comments.
- Validation 2026-06-05 (Codex CityMap BuyDialog shell):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> only expected controller intent/BuyDialog shell
    hits and existing model fields/comments.
- Validation 2026-06-05 10:50 (Claude review of commit e7a1b48):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/model names and unrelated model fields/comments found.
- Validation 2026-06-05 (Codex CityMap selection focus tween):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-05 10:46 (Claude review of commit 1af4408):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/model names and unrelated model fields found.
- Validation 2026-06-05 (Codex available KPI order polish):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-05 06:52 (Claude review of commit 333ca76):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/event names and unrelated model comments found.
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
