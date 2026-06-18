# Local LM: Review + Spec-Check (Döner Empire)

## Aufgabe 1: Code-Review

Prüfe folgende Dateien auf Inkonsistenzen, veraltete Imports, ungenutzte Variablen oder Stil-Brüche:

- `lib/ui/screens/city_map_screen.dart`
- `lib/ui/widgets/map_city_overview.dart`
- `lib/ui/widgets/map_deutschland.dart`
- `lib/ui/widgets/map_street_view.dart`
- `lib/ui/widgets/street_building_painter.dart`
- `lib/ui/widgets/hybrid_shop_screen.dart`
- `lib/core/theme.dart` (besonders `MapPalette` vs `AppColors`)

Erstelle eine Liste:
1. Ungenutzte Imports
2. Duplizierte Konfigurationen (z.B. zwei Stellen, die ähnliche Farben definieren)
3. Veraltete Kommentare/Doku im Code
4. Fehlende const-Konstruktoren
5. Sonstige Auffälligkeiten

## Aufgabe 2: Spec-Extraktion aus Referenzbild

Analysiere `docs/assets/doener_empire_3d_mobile_concept.png` (wenn möglich) und extrahiere:

1. Farbpalette (TOP 20 HEX-Werte, geclustert nach Funktion)
2. UI-Layout (Abstände, Höhen, Breiten, Positionen relativ zum Bildschirm)
3. Schriftgrößen geschätzt
4. Ikonen-Beschreibung

Schreib die Extraktion als Markdown-Tabelle.

## Aufgabe 3: Konsistenz-Prüfung

Vergleiche `docs/MAP_DESIGN_SPEC.md` mit dem tatsächlichen Code:
- Welche in der Spec erwähnten Klassen/Funktionen/Dateien existieren nicht?
- Welche existieren, werden aber in der Spec nicht erwähnt?
- Gibt es Farb-Diskrepanzen zwischen Spec und Code?

Ergebnis als:
- ✅ Matches
- ⚠️ Abweichungen (mit Erklärung)
- ❌ Fehlende Dateien/Konzepte
