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

Flutter `toJson()` schreibt diese Felder, C#-DTO ignoriert sie:
- `history`, `missions`, `stocks`, `facilities`
- `hrManager`, `hrStrategy`, `hrCandidates`
- `globalPrices`, `cityPrices`
- `activeCityCampaigns`, `activeGlobalCampaigns`
- `completedChapterIds`, `activeComboIds`, `productQuality`

Codex schließt diese Lücken im Zuge der jeweiligen Engine-Ports (M4–M6).

### Nächste Schritte (Verantwortlichkeiten klar)
- **Owner (du):** Android Build Support per Hub-GUI nachinstallieren.
  Dann meldest du dich → ich erzeuge `Packages/manifest.json` für URP
  und triggere Unity batchmode für initiale `ProjectSettings/`.
- **Codex:** `CompetitorEngine`-Port als nächste Engine (M4a). Spec
  liegt in `lib/services/competitor_engine.dart`, Testerwartungen aus
  Flutter-Test-Suite spiegeln.
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
