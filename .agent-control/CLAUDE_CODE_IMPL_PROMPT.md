# Claude Code: City Map Implementation

Du bist ein Flutter-Experte. Arbeite exakt nach Spezifikation.

## Schritt 1: MapPalette wärmer machen

Lies `lib/ui/screens/city_map_screen.dart` – dort ist `MapPalette` definiert (oder finde die echte Palette in `lib/core/theme.dart`).

Die aktuelle Palette ist kühl (`#0C0E11`, `#07080A`, `#F5A623`).
Ziel: wärmere Version, angelehnt an das Mockup-Referenzbild.

Ändere:
- `bgBase` → wärmeres Schwarzbraun
- `bgPanel` → wärmeres Dunkelbraun
- `accent` → wärmeres Orange (`#D05000` → `#F07010`)
- Alle Panel/Tab-Farben entsprechend wärmer

**Wichtig:** Erstelle die Farben als neuen `MapPalette`-Block, ändere nicht `AppColors`. Die Map-Palette soll eigenständig bleiben, aber wärmer.

## Schritt 2: Hybrid-Prototyp an echte Daten anbinden

Lies:
- `lib/ui/widgets/hybrid_shop_screen.dart`
- `lib/ui/widgets/hybrid_map_preview.dart`
- `assets/iso/building_owned.png`
- `assets/iso/building_empty.png`
- `lib/models/game_state.dart`
- `lib/models/city_model.dart`

Der Hybrid-Prototyp ist aktuell ein isolierter Screen mit Dummy-Daten. Binde ihn an echte `GameState`-, `Shop`- und `CityMapLocation`-Daten:
1. Ersetze Dummy-Shops durch `game.shops`
2. Ersetze Dummy-Locations durch `LocationEngine.locationsFor(city)`
3. Ersetze Dummy-Cash durch `game.cash`
4. Füge Navigation ein: Tap auf Gebäude → `context.push('/shop/${shop.id}')`
5. Füge Tag-beenden-Button ein (wie in `city_map_screen.dart` existiert)
6. Füge `InteractiveViewer` für Pan/Zoom ein

**Nicht ändern:**
- `city_map_screen.dart` (das ist der aktive Screen)
- `game_provider.dart` (GameState-Logik)
- `street_building_painter.dart`

## Schritt 3: Validierung

Nach jedem Schritt:
```bash
flutter analyze
flutter test
```

Beide müssen grün sein. Keine Ausnahmen.
Keine ungenutzten Imports hinterlassen.
Keine TODOs/FIXMEs im Code.
