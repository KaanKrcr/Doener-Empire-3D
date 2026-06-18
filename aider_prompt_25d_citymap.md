# Prompt: Coffee Inc 2-inspirierte 2.5D City Map (CustomPainter)

## Ziel
Erstelle eine neue, vollständige 2.5D-Stadtkarte im Stil von Coffee Inc 2 für `lib/ui/widgets/city_map_view.dart`. Die aktuelle card-basierte Version wird komplett ersetzt.

## Design-Referenz (Coffee Inc 2)
- Isometrische Stadtdarstellung (2.5D) mit Gebäuden, Straßen und Wasser
- Gebäude haben sichtbare Seitenwände, Fenster und Dachdetails (CustomPainter)
- Eigene Filialen: gold/amber (#D46816) Pin mit Döner-Icon
- Konkurrenz: grau/rot Pin mit anderem Icon
- Verfügbare Standorte: dunkler Pin mit "FOR LEASE"-Fähnchen
- Straßen: grau mit weißen gestrichelten Linien, Grid-basiert
- Floating Header: Cash-Anzeige + Stadtname + Datum
- Pan/Zoom via Gestures

## API unverändert
```dart
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;
```

## Layout-Struktur

### 1. Haupt-Container
- `Stack` als Root
- Layer 1: `InteractiveViewer` mit der Stadtkarte (Pan + Zoom)
- Layer 2: Floating Header oben (halbtransparent)
- Layer 3: Bottom Controls (Standort-Name, Buttons)

### 2. Stadtkarte (InteractiveViewer mit CustomPainter)
- Leinwandgröße: 1200x900 (größer als Bildschirm für Zoom)
- Hintergrund-Wasser: cyan/blau (#0077BE)
- Grid aus Straßen: graue Linien (#888) mit weißen gestrichelten Mittellinien
- 6x6 Block-Raster (wie alte _gridN = 6)
- Jeder Block = eine Bauparzelle für mögliche Standorte

### 3. Gebäude (CustomPainter)
- Jeder Standort bekommt ein Gebäude an seiner Grid-Position
- Gebäude sind isometrische Blöcke mit:
  - Seitenwänden (dunklerer Farbton)
  - Dachfläche (hellerer Farbton)
  - Fensterreihen (kleine Rechtecke)
  - Eingangstür (unten)
- Höhe variiert je nach Standort-Typ (3-7 Stockwerke)
- Farbe: warmes Beige/Braun (#C4A882 bis #8B7355)

### 4. Pins / Marker
- **Eigene Filiale**: Goldener Kreis (#D46816) mit weißem Döner-Icon (⊡ oder 🥙), plus Schatten
- **Konkurrenz**: Roter Kreis (#E74C3C) mit weißem Icon
- **Verfügbar**: Dunkler Kreis (#3D2E22) mit kleinem "LEASE"-Fähnchen in Gold
- **Ausgewählt**: Pin pulsiert (Animation) und hat einen Glow-Effekt

### 5. Floating Header (oben)
- Halbtransparenter dunkler Container (#1A1A1ACC)
- links: Stadtname (Baloo2, gold)
- rechts: Cash + aktueller Tag
- Padding, abgerundete Ecken

### 6. Bottom Controls (unten)
- Halbtransparenter Streifen
- Zentral: Standortname (wenn ausgewählt) oder "Standort wählen"
- Links: Layer-Button (optional)
- Rechts: Menü-Button

## Interaktion
- Pan: Mit einem Finger ziehen
- Zoom: Mit zwei Fingern
- Tap auf Location: `onSelect(location)` callen
- Doppeltipp: Zentriert auf ausgewählte Location

## Farbpalette für die Karte
- Wasser: #0077BE
- Straßen: #999999 mit #FFFFFF Mittellinie
- Gehwege: #CCCCCC
- Gebäudewand links: #B8956A
- Gebäudewand rechts: #A08050
- Gebäudedach: #C4A882
- Fenster: #87CEEB + #FFE4B5 (beleuchtet)
- Grünflächen: #4CAF50 (#2E7D32)

## Code-Struktur
Die Datei soll enthalten:
1. `CityMapView` - StatelessWidget mit Stack + InteractiveViewer
2. `_CityMapPainter` - CustomPainter (komplette isometrische Map)
3. `_MarkerPainter` - CustomPainter für Pins/Overlays
4. `_FloatingHeader` - Widget für den Floating Header
5. `_BottomBar` - Widget für Bottom Controls

## Wichtig
- Alle Location-Daten kommen aus den bestehenden Modellen
- `flutter analyze` muss sauber sein
- `flutter test` muss 100/100 (oder 98/100 durch Layout-Overflow exkludiert)
- KEINE neuen Abhängigkeiten einführen
- KEINE Logik außerhalb dieser Datei ändern
- Die bestehenden CityMapModel-Logik bleibt unangetastet (footTrafficFor, weeklyRentFor, etc.)
