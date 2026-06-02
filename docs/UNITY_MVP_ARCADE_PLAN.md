# UNITY_MVP_ARCADE_PLAN — „Döner Rush" (3D-Arcade-Prototyp)

Stand: 2026-06-01
Autor-Rolle: Senior Unity Engineer
Engine: Unity 6 LTS (URP, 3D), Desktop/Editor zuerst, mobile-tauglich gedacht.

## Einordnung (wichtig)
Dies ist ein **eigenständiger, kleiner Echtzeit-Arcade-Prototyp** — NICHT die
große Tycoon-Simulation aus [UNITY_REWRITE_PLAN.md](UNITY_REWRITE_PLAN.md).
Ziel: in kurzer Zeit einen **spielbaren 3D-Core-Loop** im Editor haben
(Kunde → Bestellung → Produktion → Übergabe → Geld → Upgrade → Tagesziel).

Damit beide Stränge sich nicht entwirren:
- Eigene **Assembly Definition** `DoenerRush` und eigener Ordner `Assets/_Arcade/`.
- Eigene Szene `DoenerRush.unity`.
- Teilt sich nur das Projekt (URP-Settings, Packages). Keine Abhängigkeit auf
  den `DoenerEmpire.*`-Sim-Code und umgekehrt.

---

## 1. Technische Architektur (Unity 3D)

**Leitprinzipien**
- **Event-getrieben, lose gekoppelt.** Systeme reden über einen typsicheren
  `EventBus`, nicht über direkte Referenzen. UI hört nur zu.
- **Dünne MonoBehaviours, Logik in Services.** Spiellogik (Economy, Orders,
  Goal) sind reine C#-Services ohne schwere Unity-Abhängigkeit → Edit-Mode-testbar.
- **Tuning ausgelagert** in ScriptableObjects (kein Hardcoding von Werten).
- **Strikte Schichten:** `GameState` / `Economy` / `Customers` / `Production` /
  `Interaction` / `UI` — jede Schicht kennt nur den EventBus + ihre Configs.
- **Input abstrahiert:** ein `PointerInput`, das Maus (Editor/Desktop) UND Touch
  (Mobile) auf denselben Raycast mappt → von Anfang an mobiltauglich.

**Schichten & Verantwortlichkeiten**

| Schicht | Verantwortung | Kennt |
|---|---|---|
| Core | Bootstrap, EventBus, Service-Locator, GameState | nichts Spielspezifisches |
| Config | ScriptableObjects mit Tuning-Werten | — |
| Economy | Geld (Wallet), Preise, Upgrade-Käufe & -Effekte | EventBus, Config |
| Customers | Spawn, Warteschlange, Kunden-FSM, Geduld | EventBus, Config |
| Production | Stationen, Koch-Jobs (Timer), Kapazität | EventBus, Config |
| Interaction | PointerInput → Raycast → `IInteractable` | EventBus |
| Gameplay | DayTimer, GoalTracker (Tagesziel) | EventBus, Config |
| UI | HUD, Upgrade-Panel, Tagesende-Panel | EventBus (nur Lesen + Buttons feuern Intents) |

**Kommunikationsfluss (Beispiel)**
```
PointerInput ──tap──> ProductionStation.Interact()
   └─> EventBus.Raise(CookStarted)         → UI zeigt Fortschritt
ProductionStation (Timer fertig)
   └─> EventBus.Raise(DoenerReady)         → ReadyTray +1, UI update
PointerInput ──tap──> Customer.Interact()  (hat Döner-Bedarf, Tray>0)
   └─> OrderService.Deliver(customer)
        └─> Wallet.Add(price); EventBus.Raise(OrderDelivered, MoneyChanged)
GoalTracker hört MoneyChanged → prüft Tagesziel → ggf. EventBus.Raise(DayWon)
DayTimer (Zeit aus) → EventBus.Raise(DayEnded) → DayResultPanel
```

**EventBus (Empfehlung):** statische, typsichere Variante
```csharp
public static class EventBus {
    private static readonly Dictionary<Type, Delegate> _map = new();
    public static void Subscribe<T>(Action<T> h) { ... }
    public static void Unsubscribe<T>(Action<T> h) { ... }
    public static void Raise<T>(T evt) { ... }
}
```
Events sind kleine `readonly struct` (z.B. `MoneyChanged{int total,int delta}`).
Alle Subscriber im `OnDisable` abmelden (kein Leak).

---

## 2. Projektstruktur

```
Assets/
  _Arcade/
    DoenerRush.asmdef                  (Assembly, Namespace DoenerRush)
    Scenes/
      DoenerRush.unity                 (einzige Spielszene)
    Scripts/
      Core/
        GameBootstrap.cs               (Szenen-Setup, Service-Wiring)
        EventBus.cs
        GameEvents.cs                  (alle Event-Structs)
        GameState.cs                   (Phase: Playing/Won/Lost; aktueller Tag)
        ServiceRegistry.cs             (leichter Locator)
      Config/
        GameConfig.cs                  (SO: Tageslänge, Tagesziel, Startgeld)
        ProductDefinition.cs           (SO: Name, Kochzeit, Verkaufspreis)
        UpgradeDefinition.cs           (SO: Typ, Basiskosten, Kostenkurve, Effekt)
        CustomerProfile.cs             (SO: Geduld, Laufspeed, Spawn-Intervall)
      Economy/
        Wallet.cs                      (Geld + MoneyChanged-Event)
        UpgradeService.cs              (Kauf, Level, Effekt-Aggregation)
        EconomyModifiers.cs            (aktive Multiplikatoren: Preis, Kochzeit, Kapazität)
      Customers/
        CustomerSpawner.cs
        Customer.cs                    (MonoBehaviour + FSM)
        CustomerState.cs               (enum + State-Logik)
        QueueManager.cs                (Warteplätze am Tresen)
      Production/
        ProductionStation.cs           (IInteractable, Koch-Job-Timer)
        CookingJob.cs                  (reines C#: restzeit, fertig?)
        ReadyTray.cs                   (Anzahl fertiger Döner)
        OrderService.cs                (verknüpft Kunde ↔ fertige Bestellung)
      Interaction/
        PointerInput.cs                (Maus+Touch → Raycast)
        IInteractable.cs
      Gameplay/
        DayTimer.cs
        GoalTracker.cs
      UI/
        HudController.cs               (Geld, Restzeit, Ziel-Fortschritt)
        UpgradePanel.cs                (Buttons je UpgradeDefinition)
        DayResultPanel.cs              (Gewonnen/Verloren + Neustart)
    Prefabs/
      Customer.prefab
      ProductionStation.prefab
      CounterSlot.prefab
      MoneyPopup.prefab                (Worldspace-Text bei Bezahlung)
    Art/Placeholder/
      Mat_Player.mat / Mat_Customer.mat / Mat_Station.mat / Mat_Floor.mat
    Config/                            (SO-Assets-Instanzen)
      GameConfig.asset
      Product_Doener.asset
      Upgrade_FasterCook.asset / Upgrade_ExtraStation.asset / Upgrade_HigherPrice.asset
      Customer_Default.asset
  Tests/
    EditMode/
      DoenerRush.Tests.asmdef
      WalletTests.cs / UpgradeServiceTests.cs / GoalTrackerTests.cs / CookingJobTests.cs
```

---

## 3. Scenes, Prefabs, Scripts

### Scene: `DoenerRush.unity`
Hierarchie (alles aus Primitives):
- `--- ENV ---`
  - `Floor` (skaliertes Plane/Cube), `Counter` (Cube), `SpawnPoint` (leeres GO),
    `ExitPoint` (leeres GO)
- `--- GAMEPLAY ---`
  - `GameBootstrap` (Script) — verdrahtet Services, lädt Configs
  - `ProductionStation` (Prefab-Instanz, Cube + Glow-Material)
  - `CounterSlots` (3× `CounterSlot`, Wartepositionen)
  - `CustomerSpawner`
- `--- SYSTEMS ---`
  - `DayTimer`, `GoalTracker`, `PointerInput` (oder als Komponenten auf einem `Systems`-GO)
- `--- UI ---` (Canvas, Screen Space - Overlay)
  - `HUD` (Money-Text, Timer-Text, Goal-Bar)
  - `UpgradePanel` (3 Buttons)
  - `DayResultPanel` (anfangs inaktiv)
- `Main Camera` (fester isometrischer Winkel), `Directional Light`

### Prefabs
- **Customer.prefab**: Capsule + farbiges Material, `Customer.cs`, Collider
  (für Raycast), optional kleines Worldspace-Bedürfnis-Icon (Quad).
- **ProductionStation.prefab**: Cube, `ProductionStation.cs`, Collider,
  Fortschritts-Quad/Slider drüber.
- **CounterSlot.prefab**: leeres GO mit Marker-Gizmo (Warteposition).
- **MoneyPopup.prefab**: Worldspace-Canvas + TextMeshPro, kurze Aufwärts-Animation.

### Scripts
Siehe Struktur in §2. Zentrale Public-APIs:
- `Wallet`: `int Money`, `bool TrySpend(int)`, `void Add(int)`, Event `MoneyChanged`.
- `OrderService.Deliver(Customer c)`: prüft ReadyTray>0, zieht 1 ab, zahlt
  `price = product.SellPrice * EconomyModifiers.PriceMult`.
- `ProductionStation.Interact()`: startet `CookingJob` falls freie Kapazität.
- `GoalTracker`: hört `MoneyChanged`, vergleicht mit `GameConfig.DayGoal`.
- `DayTimer`: zählt `GameConfig.DayLengthSeconds` runter, feuert `DayEnded`.

---

## 4. Datenmodell

### ScriptableObjects (Tuning / Definitionen)
```csharp
// GameConfig.cs
[CreateAssetMenu(menuName="DoenerRush/GameConfig")]
public class GameConfig : ScriptableObject {
    public int   startMoney = 0;
    public int   dayGoal = 200;          // X
    public float dayLengthSeconds = 90;  // Y
}

// ProductDefinition.cs
[CreateAssetMenu(menuName="DoenerRush/Product")]
public class ProductDefinition : ScriptableObject {
    public string displayName = "Döner";
    public float  baseCookSeconds = 3f;
    public int    baseSellPrice = 10;
}

// UpgradeDefinition.cs
public enum UpgradeType { FasterCook, ExtraStation, HigherPrice }
[CreateAssetMenu(menuName="DoenerRush/Upgrade")]
public class UpgradeDefinition : ScriptableObject {
    public UpgradeType type;
    public string displayName;
    public int   baseCost = 50;
    public float costGrowth = 1.6f;      // Kosten = baseCost * growth^level
    public float effectPerLevel = 0.15f; // Bedeutung je Typ (s. EconomyModifiers)
    public int   maxLevel = 5;
}

// CustomerProfile.cs
[CreateAssetMenu(menuName="DoenerRush/CustomerProfile")]
public class CustomerProfile : ScriptableObject {
    public float patienceSeconds = 12f;
    public float moveSpeed = 2.5f;
    public Vector2 spawnIntervalRange = new(2f, 4f);
}
```

### Runtime-Modelle (reines C#)
```csharp
public enum CustomerState { Entering, Queuing, Ordering, WaitingForFood, Paying, Leaving }

public class Order {                       // eine Bestellung
    public ProductDefinition Product;
    public bool Fulfilled;
}

public class CookingJob {                  // ein laufender Koch-Vorgang
    public float Remaining;                // Sekunden bis fertig
    public bool  IsDone => Remaining <= 0f;
}

public struct UpgradeLevel {               // Laufzeit-Stand je Upgrade
    public UpgradeType Type;
    public int Level;
}
```

### EconomyModifiers (aggregierte Effekte aus Upgrades)
```csharp
public class EconomyModifiers {
    public float CookTimeMult = 1f;   // FasterCook senkt → min clamp 0.3
    public int   StationCapacity = 1; // ExtraStation erhöht parallele Koch-Jobs
    public float PriceMult = 1f;      // HigherPrice erhöht
}
```
`UpgradeService` berechnet diese aus den gekauften `UpgradeLevel`s neu, wenn ein
Kauf passiert, und feuert `ModifiersChanged`.

### Events (GameEvents.cs)
`MoneyChanged{int total,int delta}` · `CookStarted{}` · `DoenerReady{int trayCount}` ·
`OrderDelivered{int price}` · `CustomerLeftAngry{}` · `UpgradePurchased{UpgradeType,int level}` ·
`DayTick{float remaining}` · `DayEnded{bool won,int finalMoney}`

---

## 5. MVP-Scope & Out-of-Scope

### In Scope
- 1 Szene, 1 Produkt (Döner), 1 Produktionsstation (Kapazität via Upgrade erweiterbar).
- Kunden spawnen in Intervallen, laufen zu freiem Tresen-Slot, bestellen, warten.
- Tap auf Station → kocht (Timer). Tap auf wartenden Kunden mit fertigem Döner → Übergabe + Bezahlung.
- Geduld-Timer: läuft Geduld ab → Kunde geht wütend (verlorener Umsatz, kein Crash).
- 3 Upgrades: Faster Cook, Extra Station, Higher Price (mit Kostenkurve & MaxLevel).
- Tagesziel: erreiche `dayGoal` Geld vor Ablauf von `dayLengthSeconds` → Win-Panel; sonst Lose-Panel.
- „Nochmal"-Button (re-init der Szene/States, KEIN Persistenz-Save).
- Maus- UND Touch-Input über denselben Raycast.
- Placeholder-Primitives + Materialfarben, MoneyPopup.
- Edit-Mode-Tests für Wallet, UpgradeService, GoalTracker, CookingJob.

### Out of Scope (explizit NICHT)
- Kein Savegame / keine Persistenz.
- Kein In-App-Shop, keine Ads, keine Analytics.
- Kein Multiplayer / Netzwerk.
- Keine NavMesh-KI (einfaches Lerp/MoveTowards reicht), keine Pathfinding-Hindernisse.
- Keine echten 3D-Assets, Animationen, Sound (optional späterer Polish).
- Keine mehreren Produkte/Stationstypen, keine Mitarbeiter, keine Story.
- Keine Konzern-/Sim-Systeme aus dem großen Port.

---

## 6. Akzeptanzkriterien

Funktional
- [ ] Beim Start zeigt das HUD Geld = `startMoney`, Restzeit = `dayLengthSeconds`, Ziel = `dayGoal`.
- [ ] Kunden spawnen periodisch und besetzen freie Tresen-Slots; bei vollem Tresen wird nicht über die Slot-Zahl hinaus „eingecheckt".
- [ ] Tap auf die Station startet genau dann einen Koch-Job, wenn freie Kapazität (`StationCapacity`) vorhanden ist; Fortschritt ist sichtbar.
- [ ] Nach `baseCookSeconds * CookTimeMult` erhöht sich der ReadyTray um 1.
- [ ] Tap auf einen wartenden Kunden mit ReadyTray > 0 übergibt, zahlt `baseSellPrice * PriceMult`, Kunde verlässt den Laden, Geld steigt sichtbar (MoneyPopup).
- [ ] Läuft die Geduld eines Kunden ab, verlässt er den Laden ohne Zahlung; kein Fehler/Crash.
- [ ] Upgrade-Kauf zieht Kosten ab (nur wenn leistbar), erhöht Level, verändert den jeweiligen Effekt messbar; Kosten steigen gemäß `costGrowth`; `maxLevel` wird respektiert.
- [ ] Erreicht das Geld `dayGoal` vor Zeitablauf → DayResultPanel „Gewonnen".
- [ ] Zeit läuft ab ohne Ziel → DayResultPanel „Verloren". „Nochmal" startet sauber neu.

Technisch / Qualität
- [ ] Keine direkten Cross-Layer-Referenzen außer über EventBus/Config (UI referenziert keine Customer/Station-Instanzen).
- [ ] Alle Tuning-Werte stammen aus ScriptableObjects/`GameConfig` — keine hartkodierten Zahlen in der Logik.
- [ ] Edit-Mode-Tests grün: Wallet (spend/add/clamp), UpgradeService (Kostenkurve, Effekt, maxLevel), GoalTracker (Schwelle), CookingJob (Timer).
- [ ] Läuft im Editor (Play) auf Desktop; Input funktioniert mit Maus. Touch-Pfad ist implementiert (Raycast identisch).
- [ ] Keine Console-Errors im normalen Spielverlauf; keine Event-Leaks (Unsubscribe in OnDisable).
- [ ] 60 FPS im Editor mit ~10 gleichzeitigen Kunden (Primitives, keine teuren Effekte).

---

## 7. Implementierungsreihenfolge (Schritt-für-Schritt für Codex)

> Jeder Schritt endet **lauffähig/testbar**. Nicht den nächsten beginnen, bevor
> der aktuelle im Editor verifiziert ist.

1. **Projekt-Setup**
   - `Assets/_Arcade/` + `DoenerRush.asmdef` (Namespace `DoenerRush`).
   - Leere Szene `DoenerRush.unity`, Kamera auf festen iso-Winkel, Licht, `Floor`.
   - Verify: Szene startet, leerer Boden sichtbar.

2. **Core-Gerüst**
   - `EventBus`, `GameEvents` (Event-Structs), `GameState`, `ServiceRegistry`, `GameBootstrap`.
   - Verify: Bootstrap loggt „services ready", keine Errors.

3. **Config-SOs + Assets**
   - `GameConfig`, `ProductDefinition`, `UpgradeDefinition`, `CustomerProfile` + je 1 Asset in `Config/`.
   - Verify: Assets im Inspector editierbar.

4. **Economy-Kern (testfähig, ohne Szene)**
   - `Wallet`, `EconomyModifiers`, `UpgradeService`.
   - Edit-Mode-Tests: WalletTests, UpgradeServiceTests.
   - Verify: `dotnet`/Unity-Testrunner grün.

5. **HUD (nur Anzeige)**
   - `HudController` zeigt Geld/Timer/Ziel via EventBus.
   - `DayTimer` + `GoalTracker` (Tagesziel-Logik) + GoalTrackerTests/CookingJobTests.
   - Verify: Timer zählt runter, DayEnded feuert; Test grün.

6. **Produktion**
   - `ProductionStation` (IInteractable), `CookingJob`, `ReadyTray`.
   - Temporär per Tastendruck/Debug-Button kochen, um Tray-Anstieg zu sehen.
   - Verify: Kochen erzeugt nach Zeit einen fertigen Döner (HUD/Tray-Anzeige).

7. **Input + Interaktion**
   - `PointerInput` (Maus+Touch → Raycast → `IInteractable.Interact()`).
   - Station per Tap kochen lassen.
   - Verify: Klick auf Station startet Koch-Job.

8. **Kunden**
   - `CustomerSpawner`, `Customer` (FSM), `QueueManager`, `CounterSlot`s.
   - Bewegung per `MoveTowards`. Kunde belegt Slot, geht in `WaitingForFood`.
   - Verify: Kunden spawnen, reihen sich ein, Geduld-Timer sichtbar.

9. **Übergabe & Bezahlung**
   - `OrderService.Deliver`: Tap auf Kunde mit Tray>0 → zahlen, Kunde `Leaving`, MoneyPopup.
   - Geduld-Ablauf → `CustomerLeftAngry`.
   - Verify: Voller Loop Kunde→Döner→Geld funktioniert; Geld steigt.

10. **Upgrades-UI**
    - `UpgradePanel` mit 3 Buttons; Kauf über `UpgradeService`; `EconomyModifiers` wirken
      (Kochzeit/Kapazität/Preis ändern sich spürbar).
    - Verify: Kauf zieht Geld, Effekt messbar, Kosten steigen, maxLevel greift.

11. **Tagesende & Restart**
    - `DayResultPanel` (Win/Lose), „Nochmal" reinitialisiert States/Szene.
    - Verify: Ziel erreicht → Win; Zeit aus → Lose; Neustart sauber.

12. **Polish-Pass (klein)**
    - MoneyPopup-Animation, Station-Fortschrittsbalken, einfache Farb-Hervorhebung
      bei wartenden/ungeduldigen Kunden. Mobile-Check (Touch, Canvas-Skalierung).
    - Verify: Akzeptanzkriterien §6 vollständig abgehakt.

---

## Risiken / Hinweise
- **Input doppelt (UI vs. World):** Pointer-Raycast muss UI-Klicks ignorieren
  (`EventSystem.current.IsPointerOverGameObject()`), sonst löst ein Button-Tap
  auch einen World-Raycast aus.
- **Event-Leaks:** konsequent in `OnDisable` abmelden.
- **Balance:** `dayGoal`/`dayLengthSeconds`/Preise früh über `GameConfig` justieren —
  nicht im Code. Startwerte oben sind Anhaltspunkte, kein finales Tuning.
- **Skalierung später:** Mehr Produkte/Stationen sind additiv möglich, weil
  Produkte/Upgrades schon datengetrieben (SO) sind.
