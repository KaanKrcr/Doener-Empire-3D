# Bekannte Risiken und Bugs (Interner Test-Release)

## Kritisch vor externem Release

- Release-Signing ist aktuell auf Debug-Key gestellt
  (`android/app/build.gradle.kts`, `buildTypes.release.signingConfig = debug`).
  Für Public Release muss ein echter Keystore mit sicherer Signierung verwendet werden.

## Technische Risiken

- Kotlin-Plugin-Migrationswarnung beim Android-Build (KGP → Built-in Kotlin):
  Stand 2026-06-10 nach Patch-Update **nur noch `audioplayers_android`**
  (6.7.1, neueste Version). `shared_preferences_android` (2.4.25) und
  `url_launcher_android` (6.3.32) sind durch das Update bereits migriert und
  raus aus der Warnung.
  - **Upstream-blockiert:** Es gibt noch keine `audioplayers`-Version mit
    Built-in-Kotlin-Support; die installierte ist bereits die neueste.
  - **Nicht build-blockierend** auf Flutter 3.44 (Debug-/Release-APK bauen
    erfolgreich). Künftige Flutter-Versionen könnten das erzwingen.
  - **To-do:** `flutter pub upgrade audioplayers` sobald der Maintainer eine
    migrierte Version veröffentlicht; dann verschwindet die Warnung ganz.
- ~~Build-Warnung zu erwarteten Cupertino-Fonts (nicht build-blockierend).~~
  Erledigt 2026-06-10: `cupertino_icons` als Dependency ergänzt (Flutter-Default);
  Font-Manifest-Warnung verschwindet, ungenutzte Glyphen werden tree-geshaked.

## Produkt-/UX-Risiken für Testphase

- ~~App-Label auf Android ist derzeit technisch (`doener_empire`) statt
  marketingfreundlich (`Döner Empire`).~~ Erledigt 2026-06-09: Label auf `Döner Empire` gesetzt.
- Keine Crash-Reproduktion in den aktuellen Smoke-Tests, aber Fokus für Tester:
  - Corporate-/Imperium-Tab mehrmals öffnen/schließen
  - Alt-Save laden und mehrere Tage fortsetzen
  - Auto-Hire + Lieferdienst über längere Spielstände beobachten

## Verifizierter Stand (2026-05-25)

- `flutter analyze`: erfolgreich, keine Issues
- `flutter test`: alle Tests erfolgreich
- `flutter build apk --debug`: erfolgreich
- `flutter build apk --release`: erfolgreich
