# README_STATUS - Doener Empire

## Aktueller Stand
- Projektstatus: in aktiver Entwicklung, Working Tree sauber.
- Fokus der letzten Iterationen: Flutter-CI/Smoke-Tests, 2.5D-City-Map-Richtung
  und Unity-Vertical-Slice-Reviews ueber Agent-Control.
- Aktueller Branch: `main`
- Aktueller Commit: `e904f1e` (`Accept RestaurantDetail price controller review`)
- Remote: `origin/main`, synchron zum lokalen `main`
- Review Queue: `Status: empty`

## Letzte Änderungen
- Flutter-Fokus bestaetigt: Unity-Voll-Port bleibt Referenz, Premium-Optik wird
  als isometrische 2.5D-City-Map direkt in Flutter weiterverfolgt.
- Flutter CI Gate auf `main`: `flutter analyze` + `flutter test`.
- Smoke-Tests fuer Haupttabs und Detail-Screens ergaenzt.
- Unity-Agent-Control weitergefuehrt: RestaurantDetail-Preisfluss fuer Commit
  `f51589c` reviewed und akzeptiert.
- n8n/OpenClaw-Dispatch wieder lauffaehig; Router-Fix fuer Review-vs-Codex-
  Zuordnung im lokalen OpenClaw-Workspace dokumentiert.

## Offene Todos / Bugs
- Release-Signing steht weiter auf Debug-Konfiguration (vor Public Release umstellen).
- Kotlin-Plugin-Migrationswarnung aktuell noch bei `audioplayers_android`
  upstream-blockiert; `shared_preferences_android`/`url_launcher_android` sind
  bereits migriert.
- Längere manuelle Runs für Cash-/Tageslogik weiter nötig (insb. Delivery + Auto-Hire in großen Spielständen).
- Save-Kompatibilität bleibt kritisch bei Legacy-Saves: Delivery-/History-Felder und alte Shop-/Upgrade-Pfade weiter gegentesten.
- Naechstes konkretes Agent-Queue-Item muss durch Claude/Kaan definiert werden;
  Codex soll bei leerer Queue nicht pauschal weiterarbeiten.

## Ergebnis Flutter Analyze / Test
- Ausgeführt am: 2026-06-11 06:07 +02:00
- `flutter analyze`: **erfolgreich**, keine Findings.
- `flutter test`: **erfolgreich**, 97 Tests bestanden.

## Ergebnis Unity Logic Tests
- Ausgefuehrt am: 2026-06-11 07:39 +02:00
- `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`:
  **erfolgreich**, 507 Tests bestanden, 0 Fehler.

## Nächster sinnvoller Schritt
1. Claude/Kaan definiert das naechste konkrete Review-/Implementierungsitem.
2. Empfohlene Produktspur: Flutter City-Map als primaeren Ingame-Flow staerken
   (Bottom-Sheet-Entscheidungen, Konkurrenzmarker, Profit-/Risiko-Prognose).
3. Empfohlene Release-Spur: Signing klaeren und internen APK-Test mit
   `docs/MANUAL_TESTPLAN_INTERNAL.md` fahren.
