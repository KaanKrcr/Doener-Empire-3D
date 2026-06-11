# README_STATUS - Doener Empire

## Aktueller Stand
- Projektstatus: Flutter-first, aktive Entwicklung.
- Unity ist als Produkt- und Build-Spur beendet und aus dem Repo entfernt.
- Aktueller Branch: `main`
- Remote: `origin/main`
- Review Queue: `Status: empty`

## Letzte Aenderungen
- Premium-Console-Ansatz aus dem Unity-Slice in die Flutter-Shop-Detail-Ansicht portiert.
- Flutter-Shop-Detail startet jetzt mit einem staerkeren Command-Header:
  Cash, Tagesprofit, Kunden, aktive Kampagnen, Ruf und Team sind direkt sichtbar.
- Unity-Projekt, Unity-Logic-Tests, Unity-GitHub-Actions und Unity-Dokumente entfernt,
  damit Android-Pakete kuenftig ueber Flutter gebaut werden.
- Agent-Control auf Flutter-only umgestellt.

## Offene Todos / Bugs
- Release-Signing steht weiter auf Debug-/Standardkonfiguration und muss vor einem
  oeffentlichen Release sauber eingerichtet werden.
- Kotlin-Plugin-Migrationswarnung aktuell noch bei `audioplayers_android`
  upstream-blockiert.
- Laengere manuelle Runs fuer Cash-/Tageslogik weiter noetig, besonders Delivery,
  Auto-Hire und grosse Spielstaende.
- Save-Kompatibilitaet bleibt kritisch bei Legacy-Saves.

## Ergebnis Flutter Analyze / Test
- Ausgefuehrt am: 2026-06-11 +02:00
- `flutter analyze`: erfolgreich, keine Findings.
- `flutter test`: erfolgreich, 98 Tests bestanden.
- `flutter build apk --release`: erfolgreich,
  `build/app/outputs/flutter-apk/app-release.apk`.

## Naechster sinnvoller Schritt
1. Flutter-APK an Tester geben.
2. City Map weiter zur primaeren Spieloberflaeche ausbauen.
3. Shop-Detail weiter visuell veredeln und mit echten Entscheidungshilfen staerken.
