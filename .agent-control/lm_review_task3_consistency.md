# Konsistenz-Prüfung

### ✅ Matches

| **Design-Spec** | **Code** |
|-----------------|----------|
| Verbindliche warme Kartenpalette | `MapPalette` in `lib/ui/widgets/building_styles.dart` |
| Farben wie `bgDeep`, `accent`, `gold`, `danger`, `success`, `textMain`, `textMuted`, `textDim`, `asphalt`, `sidewalk`, `marking`, `border` | Konstanten in `MapPalette` |
| Nutzungsregeln für Farben | Implementiert in der Palette |
| Iso-Projektion und Tile-System | `tileWidth = 64.0`, `tileHeight = 32.0` |
| `IsoTileCoord`, `IsoMapEntity`, `CityIsoLayout` | Klassen in Dart-Code |
| Stadtstruktur und Rendering-Layer | Layer-Reihenfolge und Sortierung in Code |
| Eigene Filiale, Konkurrenz, Freier Standort, Füllgebäude | Implementiert in `BuildingStyle` |
| Karteninteraktion und Kamera | Verwendung von `InteractiveViewer` mit `TransformationController` |
| Floating Header | Implementiert in UI |
| Kartensteuerung | Zentrieren-Button, Layer-Button |
| Kontext-Bottom-Sheet | Implementiert in UI |

### ⚠️ Abweichungen

| **Design-Spec** | **Code** |
|-----------------|----------|
| `CityMapScreen` besteht aus drei getrennten Ebenen: Deutschlandkarte, Stadtteile als organische Polygone, separater 2.5D-Straßenzug | Code zeigt eine 3-Ebenen-City-Map mit Deutschlandkarte, Vogelperspektive und 2.5D-Straßenzug |
| `CityMapScreen` zeigt nach Auswahl einer Stadt direkt deren zoombare Iso-Karte | Code zeigt eine 3-Ebenen-City-Map, aber die Ebenen sind nicht zusammenhängend |
| Standorte, eigene Filialen, Konkurrenz, Landmarken und Dekoration verwenden dasselbe Tile-Koordinatensystem | Code verwendet verschiedene Modelle wie `CityMapLocation`, `ShopModel`, `CompetitorModel` |
| Header, Markerlabels, Kartensteuerung und Bottom Sheet bleiben normale Flutter-Widgets | Implementiert in UI |
| Es gibt keinen Unity-/Godot-Wechsel | Code verwendet Flutter |
| Flame gehört nicht zum MVP und wird erst bei vielen gleichzeitig animierten Kunden oder Fahrzeugen neu bewertet | Nicht erwähnt im Code |

### ❌ Fehlend

| **Design-Spec** | **Code** |
|-----------------|----------|
| `hybrid_shop_screen.dart` demonstriert den Premium-Screen mit Sprite-Hintergrund, Flutter-Overlays und echten Spieldaten | Nicht implementiert |
| `hybrid_map_preview.dart` ist die isolierte visuelle Referenz für Hero-Gebäude, Label-Bubble, Konkurrenz-Pins und Status-Header | Nicht implementiert |
| `assets/iso/building_owned.png` und `assets/iso/building_empty.png` sind vorhanden | Nicht in Code erwähnt |
| Die Sprites sind noch nicht in die produktive City Map integriert | Nicht implementiert |
| Die vorhandenen PNGs besitzen einen eingebackenen Hintergrund/Halo und müssen langfristig durch Exporte mit echtem Alpha ersetzt werden | Nicht implementiert |

### Doku Aktualität

Die Dokumentation scheint aktuell zu sein, da sie viele Details zur Implementierung der City Map enthält. Es gibt jedoch einige Abweichungen zwischen der Spezifikation und dem tatsächlichen Code, was darauf hindeutet, dass die Implementierung noch nicht vollständig oder korrekt ist.