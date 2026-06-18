# Codex: Iso-Tilemap-Architektur für Flutter

## Ziel

Baue eine vollständige Iso-Tilemap-Engine in Flutter, die:
1. Ein isometrisches Tile-Raster rendert (2:1, dimetrisch)
2. Jedes Tile mit einem PNG-Sprite (aus `assets/iso/`) belegen kann
3. Pan- und Zoom-fähig ist (InteractiveViewer)
4. Tap-Erkennung auf einzelne Tiles hat
5. Eine Szene aus mehreren Layern aufbauen kann (Boden → Gebäude → Overlays)

Das Zielbild: Eine zusammenhängende Iso-Stadt, keine 3 getrennten Ebenen mehr.

## Vorgehen

### Schritt 1: IsoTilemap-Koordinatensystem

Erstelle `lib/ui/widgets/iso_tilemap.dart` mit:

```dart
class IsoGrid {
  static const tileW = 64.0;   // Breite einer Iso-Kachel
  static const tileH = 32.0;   // Höhe einer Iso-Kachel

  /// Welt-Koordinate (Spalten/Zeilen) → Pixel-Position auf dem Bildschirm
  static Offset tileToScreen(int col, int row) { ... }

  /// Pixel-Position → Tile-Koordinate (für Tap-Erkennung)
  static Point<int> screenToTile(Offset point) { ... }

  /// Gibt die Pixel-Distanz zwischen zwei Tiles zurück
  static double tileDistance(int c1, int r1, int c2, int r2) { ... }
}
```

### Schritt 2: TilemapRenderer (CustomPainter)

Erstelle `lib/ui/widgets/iso_tilemap_painter.dart`:

```dart
class IsoTile {
  String? spriteAsset;  // assets/iso/building_owned.png oder null = leer
  bool isWalkable;      // false = Gebäude, true = Straße
  bool isHero;          // true = eigene Filiale (bekommt Glow)
  String? label;        // optionaler Name (wird als Overlay gezeichnet)
  double? rating;       // optionales Rating
}

class TilemapData {
  final int width;      // Anzahl Tiles horizontal
  final int height;     // Anzahl Tiles vertikal
  final List<List<IsoTile>> tiles;
  final Offset heroTile; // Position der eigenen Filiale
}
```

Der Painter zeichnet:
1. **Boden-Layer** (Straße/Gehweg/Gras) – einfarbige Rauten
2. **Gebäude-Layer** – PNG-Sprites aus assets/iso/
3. **Hero-Glow** – radialer Gradient um das Hero-Tile
4. **Labels** – Namens-Bubble über Gebäuden

### Schritt 3: Stadt-Szene aufbauen

Eine konkrete Stadt-Szene in `lib/data/berlin_scene.dart`:

```dart
TilemapData buildBerlinScene(List<Shop> shops) {
  // Baue eine 20×20 Iso-Karte
  // Zentrum: Hero-Filiale
  // Ränder: Füll-Gebäude (zufällig, aus einem Seed)
  // Straßen zwischen Gebäuden
  // Konkurrenz-Gebäude an festen Positionen
  // Freie Standorte als leere Grundstücke
}
```

Nutze diese Assets (Platzhalter, werden später durch Blender-Renders ersetzt):
- `assets/iso/building_owned.png` – Hero-Gebäude
- `assets/iso/building_empty.png` – freier Standort
- Für Konkurrenz/Füll-Gebäude: temporär farbige Rauten zeichnen

### Schritt 4: Integration in city_map_screen.dart

Ersetze die aktuelle 3-Ebenen-Navigation (Deutschland→Stadt→Straße) durch einen Screen, der:
1. Die Iso-Tilemap als Hauptansicht zeigt
2. Deutschland als Overlay/Navigation behält (via Button "Stadt wechseln")
3. Detail-Panel als Bottom-Sheet (snapbar)
4. Tag-beenden-Button

Konkret: `CityMapScreen` behält den Deutschland-Level, aber den Stadt-Level ersetzt du durch die Iso-Tilemap. Street-Level entfällt (die Stadt ist jetzt der Straßenzug).

### Schritt 5: Interaktion

- **Pan/Zoom** via `InteractiveViewer`
- **Tap auf Hero-Gebäude** → Detail-Panel öffnet sich (wie hybrid_shop_screen.dart)
- **Tap auf freien Standort** → "Filiale eröffnen"-Dialog
- **Tap auf Konkurrenz** → Kurzinfo
- **Pinch/Scroll** → Zoom

### Schritt 6: Validierung

```bash
flutter analyze   # 0 issues
flutter test      # alle grün
flutter build apk --debug  # erfolgreich
```

## Bestehende Architektur, die du nutzen SOLLST:

- `lib/ui/widgets/building_styles.dart` → `MapPalette` (Farben)
- `lib/ui/widgets/hybrid_shop_screen.dart` → als Vorlage für Detail-Panel
- `lib/models/game_state.dart` → `shop.displayName`, `shop.reputation`, `shop.locationName`
- `lib/services/location_engine.dart` → Standort-Logik
- `lib/ui/screens/city_map_screen.dart` → Haupt-Screen, den du bearbeiten darfst
- **KEINE Sprites.** Nicht assets/iso/ verwenden. Stadt komplett prozedural aus farbigen Rauten.

## Bestehende Dateien, die du NICHT ändern sollst:

- `lib/core/router.dart` – nur wenn neuer Pfad nötig
- `lib/providers/game_provider.dart` – GameState-Logik
- `lib/models/*` – Modelle nicht umbauen
- Test-Dateien außer `screen_smoke_test.dart` (darfst smoke-test für neue Widgets ergänzen)

## Output

1. `lib/ui/widgets/iso_tilemap.dart` – IsoGrid-Koordinatensystem
2. `lib/ui/widgets/iso_tilemap_painter.dart` – TilemapRenderer
3. `lib/data/berlin_scene.dart` – Beispiel-Stadt-Szene
4. Änderungen an `lib/ui/screens/city_map_screen.dart` – Integration
5. `test/iso_tilemap_test.dart` – einfacher Render-Smoke-Test

Nach jedem erstellten File: flutter analyze. Bei Fehlern sofort beheben.
