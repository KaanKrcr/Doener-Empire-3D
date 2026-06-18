# Map Design Spec — Premium Iso City

Diese Spezifikation beschreibt das verbindliche Zielbild der City Map: eine
zusammenhängende, zoombare Iso-Stadt in Flutter. Die bestehende
Wirtschaftssimulation bleibt unverändert; neu aufgebaut wird die visuelle
Karten- und Interaktionsebene.

Visuelle Referenzen:

- `docs/assets/doener_empire_3d_mobile_concept.png`
- `docs/assets/doener_empire_mobile_premium_ui.png`
- `docs/UI_STYLE_GUIDE.md`

Alle Referenzmaße beziehen sich auf einen Design-Frame von **1080 × 1620 px
(2:3)**. Flutter-Komponenten müssen responsiv umgesetzt werden; Pixelwerte aus
diesem Dokument sind Richtwerte, keine festen Gerätemaße.

## Status — 2026-06-18

### Produktiver Stand

Der aktive `CityMapScreen` besteht derzeit aus drei getrennten Ebenen:

1. `map_deutschland.dart` — Deutschlandkarte für die Stadtauswahl
2. `map_city_overview.dart` — Stadtteile als organische Polygone
3. `map_street_view.dart` und `street_building_painter.dart` — separater
   2.5D-Straßenzug mit sechs prozedural gezeichneten Gebäuden

Dieser Aufbau bleibt während der Migration funktionsfähig, ist aber nicht mehr
das Zielbild.

### Vorhandene Prototypen und Assets

- `hybrid_shop_screen.dart` demonstriert den Premium-Screen mit
  Sprite-Hintergrund, Flutter-Overlays und echten Spieldaten.
- `hybrid_map_preview.dart` ist die isolierte visuelle Referenz für
  Hero-Gebäude, Label-Bubble, Konkurrenz-Pins und Status-Header.
- `assets/iso/building_owned.png` und
  `assets/iso/building_empty.png` sind vorhanden.
- Die Sprites sind noch **nicht** in die produktive City Map integriert.
- Die vorhandenen PNGs besitzen einen eingebackenen Hintergrund/Halo und
  müssen langfristig durch Exporte mit echtem Alpha ersetzt werden.

### Verbindliche Entscheidung

Die drei Kartenebenen werden langfristig durch **eine zusammenhängende
Flutter-native Iso-Tilemap pro Stadt** ersetzt.

- Die Deutschlandkarte bleibt als separate Expansions- und Stadtauswahl-UI.
- `CityMapScreen` zeigt nach Auswahl einer Stadt direkt deren zoombare
  Iso-Karte.
- Standorte, eigene Filialen, Konkurrenz, Landmarken und Dekoration verwenden
  dasselbe Tile-Koordinatensystem.
- Header, Markerlabels, Kartensteuerung und Bottom Sheet bleiben normale
  Flutter-Widgets.
- Es gibt keinen Unity-/Godot-Wechsel.
- Flame gehört nicht zum MVP und wird erst bei vielen gleichzeitig animierten
  Kunden oder Fahrzeugen neu bewertet.

Die Art-Pipeline und Exportkonventionen stehen in
[`BLENDER_SPRITE_PIPELINE.md`](BLENDER_SPRITE_PIPELINE.md).

---

## 0. Verbindliche warme Kartenpalette

`MapPalette` in `lib/ui/widgets/building_styles.dart` ist die Quelle der
Wahrheit. Die frühere kühle, blaugraue Mockup-Palette ist verworfen.

```dart
// HINTERGRUND / PANELS
const bgDeep   = Color(0xFF0A0806);
const bgBase   = Color(0xFF130E0A);
const bgPanel  = Color(0xFF1A130E);
const bgCard   = Color(0xFF221810);

// MARKE / STATUS
const accent   = Color(0xFFF07010);
const gold     = Color(0xFFD46816);
const danger   = Color(0xFFE74C3C);
const success  = Color(0xFF7BC950);

// TEXT
const textMain  = Color(0xFFFFFAE6);
const textMuted = Color(0xFFC4B5A0);
const textDim   = Color(0xFF8C7B6C);

// STRASSE / KANTEN
const asphalt  = Color(0xFF0C0905);
const sidewalk = Color(0xFF221810);
const marking  = Color(0xFF3D3028);
const border   = Color(0xFF3A2C20);
```

### Nutzungsregeln

- Orange markiert primäre Aktionen, Auswahl und eigene Filialen.
- Grün markiert freie Standorte oder positive Performance.
- Rot markiert Konkurrenz, Risiko und Verlust.
- Große Kartenflächen bleiben dunkel und warm.
- Creme ist die primäre Textfarbe; reines Weiß nur ausnahmsweise.
- Pro Ansicht sind höchstens zwei kräftige Akzentfarben gleichzeitig dominant.

---

## 1. Iso-Projektion und Tile-System

Die Stadt nutzt eine dimetrische **2:1-Projektion** mit einem sichtbaren Winkel
von ungefähr **26,57°**.

Referenzgröße eines Tiles:

```dart
const tileWidth = 64.0;
const tileHeight = 32.0;

Offset projectTile(IsoTileCoord tile, Offset origin) {
  return Offset(
    origin.dx + (tile.x - tile.y) * tileWidth / 2,
    origin.dy + (tile.x + tile.y) * tileHeight / 2 - tile.elevation,
  );
}
```

Alle Objekte werden über ihren Boden-/Fußpunkt verankert. Die visuelle Höhe
eines Sprites verändert nicht seine logische Tile-Position.

### Vorgesehene Kartenmodelle

```dart
class IsoTileCoord {
  final int x;
  final int y;
  final double elevation;
}

class IsoMapEntity {
  final String id;
  final IsoEntityKind kind;
  final IsoTileCoord tile;
  final String assetKey;
  final IsoFootprint footprint;
  final Offset anchor;
}

class CityIsoLayout {
  final int width;
  final int height;
  final List<IsoGroundTile> groundTiles;
  final List<IsoMapEntity> entities;
  final Map<String, IsoTileCoord> locationTiles;
}
```

Die konkrete Dart-Implementierung darf immutable Records oder Klassen
verwenden, muss aber diese Verantwortlichkeiten erhalten.

### Bezug zu bestehenden Daten

- `CityMapLocation` bleibt Adapter für Zielgruppe, Traffic, Miete, Risiko und
  Empfehlung.
- `mapPosition` wird während der Migration beibehalten und um eine feste
  Tile-Koordinate ergänzt oder anschließend dadurch ersetzt.
- `Shop.locationName` und `Competitor.locationName` bleiben die Verbindung zur
  Wirtschaftssimulation.
- Kartenlayouts sind Präsentationsdaten und dürfen keine parallele
  Wirtschaftssimulation einführen.

---

## 2. Stadtstruktur und Rendering-Layer

Die Karte besteht aus einer statischen Stadtkomposition mit klaren
Entscheidungspunkten, nicht aus einer frei editierbaren City-Builder-Simulation.

### Layer-Reihenfolge

1. Boden, Straßen, Gehwege, Wasser und Grünflächen
2. Gebäude, Landmarken, Bäume, Fahrzeuge und Dekoration
3. Auswahlglow, Kundenströme, Warnungen und Einflusszonen
4. Marker und Labels als Flutter-Overlay
5. Header, Kartensteuerung und Bottom Sheet

### Sortierung

Sichtbare Weltobjekte werden stabil sortiert nach:

1. `tile.x + tile.y`
2. `tile.elevation`
3. stabiler `entity.id`

Bei mehrteiligen Footprints ist der vorderste Bodenpunkt des Objekts für die
Sortierung maßgeblich. Damit überdecken vordere Gebäude zuverlässig hintere
Objekte, ohne von ihrer Sprite-Höhe abhängig zu sein.

### Stadtinhalt

Eine MVP-Stadt enthält:

- 6–10 spielbare Standorte
- eigene Filialen und freie Grundstücke
- sichtbare Konkurrenzfilialen
- 1–3 Landmarken wie Bahnhof, Universität oder Innenstadtplatz
- 8–12 Füllgebäude-Varianten
- Straßen, Gehwege, Bäume, Wasser oder Parkflächen
- sparsame dekorative Fahrzeuge und Personen

Gebäude sind Hintergrund und Orientierung. Spielbare Standorte müssen trotz
der Detailmenge in weniger als drei Sekunden erkennbar bleiben.

---

## 3. Gebäude und Sprites

Die Zielqualität entsteht aus vorgerenderten Blender-Sprites, nicht aus
komplexeren Vektor-Paintern.

### Eigene Filiale

- größtes und kontrastreichstes Gebäude im aktuellen Kartenausschnitt
- warme Fenster und Leuchtreklame
- orange Kontur oder separater Auswahlglow
- Label-Bubble mit Name und Reputation
- Bodenlicht nur bei Auswahl oder aktiver Filiale

### Konkurrenz

- eigene Gebäude-Sprites statt bloßer Punkte
- rote Akzente, aber weniger Bloom als bei der Spielerfiliale
- gedämpfte Label-Chips mit Name und Rating
- Markt- oder Einflusszonen nur als optionaler Layer

### Freier Standort

- klar lesbares leeres Grundstück, Rohbau oder unbeleuchtetes Ladenlokal
- grüner Marker für Verfügbarkeit
- kein permanenter starker Glow

### Füllgebäude

- geringe Kontraste und wenig gesättigte Materialien
- keine dominanten Markenschilder
- Varianten bei Dach, Höhe und Fassade
- identischer Kamerawinkel, Fußpunkt und Beleuchtungsaufbau

Assetnamen und Blender-Exportregeln sind in
[`BLENDER_SPRITE_PIPELINE.md`](BLENDER_SPRITE_PIPELINE.md) verbindlich
definiert.

---

## 4. Karteninteraktion und Kamera

Das MVP verwendet Flutter `InteractiveViewer` mit einem
`TransformationController`.

### Kameraverhalten

- initial auf eine vorhandene eigene Filiale zentrieren
- ohne eigene Filiale auf den empfohlenen Startstandort zentrieren
- Pan und Pinch-Zoom unterstützen
- sinnvolle Mindest- und Maximalvergrößerung definieren
- Zentrieren-Button setzt auf das aktuell ausgewählte Objekt zurück
- Layer-Button öffnet Marktanteil, Konkurrenz und Nachfrage als exklusive
  Kartenmodi

Viewport-Koordinaten werden für Hit-Tests mit
`TransformationController.toScene()` in Szenenkoordinaten umgerechnet.

### Auswahl

- Tap auf Standort oder Gebäude setzt genau eine aktive Auswahl.
- Die Auswahl skaliert oder pulsiert kurz, ohne dauerhaft zu springen.
- Das Bottom Sheet wechselt per Crossfade auf die neuen Daten.
- Ein Tap auf freie Kartenfläche schließt Detailinformationen nicht
  automatisch, sofern das Bottom Sheet bereits eine relevante Entscheidung
  zeigt.
- Labels werden als Flutter-Widgets über projizierten Fußpunkten positioniert
  und bleiben bei Zoom lesbar; sie werden nicht in Sprites eingebrannt.

---

## 5. UI-Overlays

### Floating Header

- Kontostand und aktueller Tag sind immer sichtbar.
- Stadtname und Navigation zur Deutschlandkarte bleiben kompakt.
- Der Header darf die wichtigsten Standorte im oberen Kartenbereich nicht
  verdecken.

### Kartensteuerung

- Zentrieren
- Kartenlayer
- optional Prognose/Trend

Die Buttons liegen vertikal am rechten Rand und verwenden `bgPanel`,
`border` und `textMuted`.

### Kontext-Bottom-Sheet

Das Sheet ist der primäre Entscheidungskontext und zeigt abhängig von der
Auswahl:

- Name und Standorttyp
- eigene Filiale, Konkurrenz oder freier Standort
- Reputation beziehungsweise Standortattraktivität
- Foot Traffic
- Wochenmiete
- Marktanteil oder Konkurrenzdruck
- kurze operative Empfehlung
- eine primäre und höchstens eine sekundäre Aktion

Beispiele:

- `Optimieren`
- `Filiale eröffnen`
- `Marketing starten`
- `Personal erhöhen`

Der Map-Bereich bleibt auch bei geöffnetem Sheet sichtbar und visuell dominant.

---

## 6. Effekte und Bewegung

### Statische Tiefe

- gebackene Schatten und Ambient Occlusion in den Sprites
- warmer Fenster- und Neonanteil
- globale, sehr leichte Vignette
- Boden-Glow als Flutter-Effekt, nicht als permanenter Sprite-Halo
- keine deckenden rechteckigen Hintergründe in Sprite-Dateien

### MVP-Motion

- sanfter Karten-Einstieg
- kurzer Scale-/Glow-Impuls bei Auswahl
- Bottom-Sheet-Crossfade
- Umsatz- oder Warnungs-Popup nach Tagesabschluss

### Später

- kleine Kundenpunkte auf festgelegten Wegen
- Konkurrenz-Ping
- Marktanteils- und Nachfragezonen
- wenige dekorative Fahrzeuge

Flame wird erst geprüft, wenn die Anzahl gleichzeitig animierter Weltobjekte
mit Flutter-Animationen nicht mehr zuverlässig performant ist.

---

## 7. Zielarchitektur in Flutter

```text
CityMapScreen
├── CityIsoMap
│   ├── InteractiveViewer
│   │   └── IsoScene
│   │       ├── GroundLayer
│   │       ├── SortedEntityLayer
│   │       └── WorldEffectLayer
│   └── ScreenSpaceLabelLayer
├── MapHeader
├── MapControls
└── LocationBottomSheet
```

### Verantwortlichkeiten

- `CityMapScreen`: GameState anbinden, Stadt wechseln, Auswahl und Aktionen
  koordinieren.
- `CityIsoLayout`: statische Geometrie und Platzierung einer Stadt.
- `CityIsoMap`: Projektion, Kamera, Hit-Tests und Layerkomposition.
- `IsoMapEntity`: renderbares Weltobjekt ohne Wirtschaftsdaten.
- Overlay-Widgets: dynamische Zahlen, Labels und Aktionen.
- `LocationEngine`: bestehende Standortanalyse und Prognosen weiterverwenden.

Die Route `/city-map/:cityId` bleibt stabil.

---

## 8. Migrationsfolge

1. Tile-Datenmodelle und ein statisches Layout für eine Pilotstadt ergänzen.
2. Iso-Canvas parallel zu `MapCityOverview` renderbar machen.
3. `CityMapLocation`, eigene Shops und Auswahl an feste Tile-Koordinaten
   anbinden.
4. Header, Label-Bubble und Bottom Sheet aus den Hybrid-Prototypen
   datengetrieben übernehmen.
5. Konkurrenzgebäude und freie Grundstücke integrieren.
6. Funktionale und visuelle Parität für Öffnen, Verwalten, Anpassen und
   Übernehmen herstellen.
7. `MapCityOverview` und `MapStreetView` aus dem produktiven Pfad entfernen.
8. `_MapLevel` in `CityMapScreen` auflösen. Die Deutschlandkarte wird über eine
   eigene Expansions-/Stadtauswahlaktion geöffnet.
9. Alte Painter und Drill-down-Komponenten erst nach grünen Tests und
   visueller Abnahme löschen.

Während der Migration dürfen bestehende Simulation, Saves und
Standortbezeichnungen nicht verändert werden.

---

## 9. Abnahmekriterien

- Eine Stadt ist auf einem zusammenhängenden Iso-Screen erkennbar.
- Eigene Filiale, freie Standorte und Konkurrenz sind ohne Drill-down
  auswählbar.
- Kamera startet auf einem relevanten Standort und kann zuverlässig
  zentriert werden.
- Labels bleiben bei allen erlaubten Zoomstufen lesbar.
- Bottom Sheet zeigt höchstens fünf entscheidende Kennzahlen.
- Primäraktion ist eindeutig.
- Sprites besitzen echtes Alpha und teilen Kamera, Licht und Fußpunkt.
- Route, Shop-Aktionen und Wirtschaftssimulation bleiben kompatibel.
- `flutter analyze` und `flutter test` bleiben grün.
