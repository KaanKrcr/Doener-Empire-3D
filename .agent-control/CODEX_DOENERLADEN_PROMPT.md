# Codex: Dönerladen-Gebäude im IsoTilemapPainter + Upgrade-Visuals

## Ziel

Die Iso-Tilemap zeigt aktuell nur einfarbige Rauten als Gebäude. Das soll sich ändern: Jedes Gebäude soll wie ein **echter Dönerladen** aussehen, mit Erkennungsmerkmalen. Und: Gekaufte Läden durchlaufen 3 visuelle Upgrade-Stufen, die man im Shop freischaltet.

## Referenz

Das Bild `docs/assets/doener_empire_3d_mobile_concept.png` zeigt den Ziel-Look:
- Flache, breite Gebäude (keine Hochhäuser)
- Markise, Schaufenster, Neon-Schild
- Außengastronomie / Tische draußen
- Warme, einladende Optik

## Bestehende Architektur (nutzen!)

- `lib/ui/widgets/building_styles.dart` → `MapPalette` (Farben), `BuildingStyle` (Stufen worn/normal/premium)
- `lib/ui/widgets/iso_tilemap_painter.dart` → existierender Painter, muss erweitert werden
- `lib/ui/widgets/iso_tilemap.dart` → IsoGrid-Koordinaten
- `lib/data/berlin_scene.dart` → Szenen-Daten

## Was zu tun ist

### 1. Upgrade-Level in IsoTile einbauen

In `lib/ui/widgets/iso_tilemap_painter.dart`, erweitere das `IsoTile`-Model:

```dart
enum BuildingUpgrade { none, basic, normal, premium }

class IsoTile {
  // ... bestehende Felder
  final BuildingUpgrade upgradeLevel;  // NEU
  final bool hasAwning;                 // NEU
  final bool hasOutdoorSeating;         // NEU  
  final bool hasNeonSign;               // NEU
}
```

### 2. Painter: 3 Upgrade-Stufen zeichnen

Ersetze die einfarbigen Gebäude-Rauten durch detaillierte CustomPainter-Zeichnungen:

**Stufe basic (gerade gekauft):**
- Kleiner, flacher Baukörper (1 Stock)
- Eine Tür + 1 Fenster
- Einfaches rechteckiges Schild "DÖNER" in Schreibschrift-ähnlichem Stil (einfach Text)
- Farbe: `MapPalette.bgCard` (dunkelbraun)
- Akzent: `MapPalette.accent` (orange) dünne Linie

**Stufe normal (einige Upgrades):**
- Größerer Baukörper
- Markise über der Tür (gewölbtes Dreieck in `MapPalette.accent`)
- 2 Fenster + 1 Tür
- Neon-Röhren-Schild (leuchtendes Orange mit Glow)
- Außengastro: 2-3 kleine runde Tische + Stühle (kleine Kreise)
- Farbe: etwas helleres Braun

**Stufe premium (volle Upgrades):**
- 2 Stockwerke
- Große Neon-Fassade (halbe Gebäudebreite, 2-3px dicker Glow)
- Außengastro mit 4 Tischen + Sonnenschirm
- Markise + 3 Fenster
- Zusätzliche Dekoration: Pflanzen (kleine grüne Kreise)

**Konkurrenz (competitor):**
- Nur Stufe basic/normal, aber in `MapPalette.danger`-Tönen (rot)
- Kein Neon, gedämpft

**Freie Standorte (empty):**
- Leeres Grundstück mit Baugerüst (einige Linien/Gitter)
- "ZU VERKAUFEN"-Schild (klein)

### 3. Szenen-Daten aktualisieren

In `lib/data/berlin_scene.dart`:
- Hero-Tile bekommt `BuildingUpgrade.basic` als Start (wird später aus Shop-State gelesen)
- Competitor-Tiles bekommen `BuildingUpgrade.none` (Standard-Konkurrenzoptik)
- Empty-Tiles bleiben wie sie sind

### 4. Später: Upgrade aus Shop-State lesen

(Nur vorbereiten, nicht implementieren) — der Code soll so geschrieben sein, dass man später einfach `tile.upgradeLevel = shopUpgradeLevel(shops[0])` setzen kann.

## Validierung

```bash
flutter analyze   # 0 issues
flutter test      # 131 Tests (4 neue für die Upgrade-Stufen)
```

## Dateien, die du bearbeiten darfst:

- `lib/ui/widgets/iso_tilemap_painter.dart`
- `lib/data/berlin_scene.dart`
- `test/iso_tilemap_test.dart` (neue Render-Tests für jede Stufe)

## Dateien, die du NICHT ändern sollst:

- `lib/ui/screens/city_map_screen.dart`
- `lib/ui/widgets/building_styles.dart`
- `lib/core/*`
- `lib/models/*`
- `lib/providers/*`
- Alle anderen Dateien außer den 3 oben genannten
