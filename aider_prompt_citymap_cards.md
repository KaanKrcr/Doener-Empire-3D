# Prompt: CityMapView durch Premium-Karten ersetzen

## Ziel
Die isometrische 2.5D-CityMapView (CustomPainter, ~800 Zeilen) wird durch eine elegante, card-basierte Standortliste ersetzt. Premium Dark Mode, Gold-Akzente.

## Änderung in `lib/ui/widgets/city_map_view.dart`

### Neue API gleich lassen
Die öffentliche API bleibt identisch:
```dart
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;
```

### Komplett neues Layout (kein CustomPainter, kein InteractiveViewer)
Die gesamte Render-Logik (Iso-Grid, _IsoTile, _IsoBuilding, Schatten, Sprites, etc.) wird entfernt.

Die neue `CityMapView` zeigt die Standorte als vertikale Liste von Premium-Cards:

### Jede Location-Card:
```
┌──────────────────────────────────┐
│  Standort-Name        ★ Traffic   │  ← Header in Gold (#D46816)
│  ───────────────────────────────  │
│  Miete: 1.200 €/Wo  │  Kaution    │  ← Key-Value Paare
│  Konkurrenz: Mittel   │  Nachfrage │
│  [Icon: Standort-Typ]             │
│  ───────────────────────────────  │
│           [FILIALE ERÖFFNEN]      │  ← Gold-CTA wenn frei
│           [VERWALTEN]             │  ← Gold-Outline wenn own
│           [GESPERRT] █████████    │  ← Grau/blockiert
└──────────────────────────────────┘
```

### Design-Spezifikationen
- **Karten-Container**: Hintergrund #3D2E22, 16px Border-Radius, Border 1px #3A2C20
- **Linker Rand**: Gold-Akzent-Linie (3px breit, #D46816, links via Container decoration)
- **Header**: Standort-Name in Baloo2, 16px, gold (#D46816). Traffic rechts in warmweiß (#FFFAE6)
- **Info-Zeilen**: Zweispaltig (Label in #C4B5A0 Sand, Wert in #FFFAE6 warmweiß)
- **CTA Button**: Gold (#D46816), abgerundet 12px, "Filiale eröffnen" wenn frei, "Verwalten" wenn bereit besessen, disabled wenn gesperrt
- **Status-Badge**: Wenn bereits eigene Filiale: kleiner grüner Badge "EIGEN" oben rechts
- **Abstand**: Cards 10px vertikal, 8px horizontal Margin

### Ausgewählter Standort
- Selected-Card bekommt einen leuchtenden Gold-Rand (2px #D46816) und leichten Gold-Glow (BoxShadow)
- Animation: Fade-In beim Wechsel

### Interaction
- Antippen einer Karte → `onSelect(location)` wird gecallt
- Die CityMap übergibt weiterhin `selected` und `onSelect` wie gehabt

### Zu entfernender Code
- Alle Iso-Geometrie-Konstanten (_gridN, _tileW, _tileH, _sceneW, _sceneH, _originX, _originY)
- Alle Iso-Funktionen (_iso, _tileFor, _Tile)
- Alle Iso-Widgets (_IsoTile, _IsoBuilding, _IsoShadow, ...)
- Sprite-Logik (Image loading, _sprites)
- Alle CustomPainter (_CityMapPainter)
- InteractiveViewer, GestureDetector für Pan/Zoom
- `import 'dart:math' as math;` (wenn nicht mehr anderswo genutzt)
- `import 'dart:ui' as ui;` (wenn nicht mehr anderswo genutzt)
- `import 'package:flutter/services.dart' show rootBundle;`

### Wichtig
- `flutter analyze` muss sauber sein
- `flutter test` muss 100/100 bestehen
- CityMapView nimmt weiterhin `CityMapLocation? selected` und feuert `onSelect`
- Keine Logik-Änderungen außerhalb dieser Datei
- Der Consumer `city_map_screen.dart` bleibt unverändert
