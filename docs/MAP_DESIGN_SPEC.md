# MAP DESIGN SPEC — Doener Empire 3D Mobile

> Abgeleitet aus Coffee Inc 2 + Premium Dark Mode (Kaan)
> Basis-Auflösung: 1080 × 1620 px (Frame). Painter-Leinwand: 1400 × 1060.

---

## 1. Hintergrund / Himmel

- **Himmelsverlauf**: Oben `#1A1A2E` → unten `#16213E`  
- **Horizont-Linie**: Y ≈ 380, weicher Übergang zur Stadt  
- **Boden/Block-Grund**: `#2D2D44` (dunkles Indigo), leichte Vignette an den Rändern

---

## 2. Iso-Straßen (6×6 Grid)

- **Straßenbelag**: `#555566`  
- **Mittelmarkierung**: `#888899`, gestrichelt (Strich 12 px, Lücke 8 px)  
- **Gehweg / Curb**: `#666677` beidseitig, 6 px breit  
- **Grid-Raster**: 6×6 (6 Zellen pro Achse). Iso-Tile: 140 × 80 px  
- **Schnittpunkte** der Straßen bilden die **Standort-Parzellen**

---

## 3. Gebäude (Isometrische Blöcke)

Jeder Standort = ein Gebäude. Höhe 3–7 Stockwerke, variiert nach Index.

### 3.1 Dach

- **Flachdach** mit leichter Überkantung (4 px überhang)
- Farbe: `#C4A882`
- Obere Kante: heller (`#D4B892`)

### 3.2 Fassade links

- `#B8956A`
- `#A08050` im Schatten (20 % dunkler)

### 3.3 Fassade rechts

- `#A08050`
- `#907040` im Schatten

### 3.4 Fenster

- Farben zufällig per `seed` (deterministisch):
  - Beleuchtet (warm): `#FFE4B5`
  - Beleuchtet (kühl): `#87CEEB`
  - Aus (dunkel): `#3D3D5C`
- Größe: 8 × 6 px, 2 pro Stockwerk pro Seite
- Abstand horizontal: 12 px, vertikal: 10 px

### 3.5 Eingangstür

- Rechte Seite, unteres Stockwerk
- Farbe: `#5D4037`
- Größe: 12 × 16 px

### 3.6 Schattenwurf

- Jedes Gebäude wirft einen Schatten nach rechts-unten
- 50 % opacity von `#000000`
- Offset: +10 px X, +6 px Y
- `MaskFilter.blur(BlurStyle.normal, 4.0)`

### 3.7 Hero-Gebäude

- Das Gebäude mit `hero: true` bekommt:
  - **Boden-Glow**: Radialer Verlauf von `#D46816` (20 %) zu transparent, Radius 60 px
  - **Neon-Outline**: `#D46816`, 2 px, nach `MaskFilter.blur(BlurStyle.normal, 6.0)` gezeichnet (Glow-Pass)

---

## 4. Pins & Marker

### 4.1 Eigene Filiale (Gold-Pin)

- Kreis: `#D46816` (gold), Radius 16 px
- Schatten: `#000000` 40 %, Offset 0/4, blur 6
- Icon: Döner-Emoji 🥙, Schrift 14 px, zentriert
- Bei `selected`: **Glow** (gold, blur 10 px, alpha 50)

### 4.2 Verfügbar (Lease-Pin)

- Kreis: `#3D2E22`, Radius 14 px
- Kleines gold- Fähnchen (`#D46816`) oben rechts am Pin
- Senkrechter Strich nach unten: `#FFFFFF` 70 %, 1.5 px

### 4.3 Konkurrenz (eigenes Shop-Modell)

- (Später) Roter Pin `#E74C3C` mit Konkurrenz-Icon

---

## 5. UI-Overlays (Stack über der Map)

### 5.1 Floating Header (oben, halbtransparent)

- Container: `#1A1A1A` @ 80 % opacity, border-radius 16 px, border `#3A2C20`
- Inhalt (Row):
  - 📍 Stadt-Emoji (24 px)
  - Stadtname (Baloo2, 18 px, gold `#D46816`)
  - Filialanzahl (12 px, `#C4B5A0`)
  - Spacer
  - 💰 Cash (14 px Emoji + Wert gold, fett)
  - 📅 Day (14 px Emoji + Wert)

### 5.2 Tag beenden Button (oben rechts)

- `ElevatedButton.icon` 38 px hoch, gold, abgerundet 20 px
- Icon: `Icons.nightlight_round` (16 px)
- Label: "Tag beenden" (12 px)

### 5.3 Location Panel (Bottom Overlay)

- Container: `#1A1A1A` @ 80 % opacity, top-radius 20 px
- Drag-Handle: 36 × 4 px, `#C4B5A0` @ 50 %, centered
- Content: Standortname, Miete/Kaution, Konkurrenz, CTA-Button gold

---

## 6. Interaktion

- **Pan**: 1 Finger ziehen (InteractiveViewer)
- **Zoom**: 2 Finger, min 0.5×, max 2.5×
- **Tap**: Auf Gebäude/Standort → `onSelect(location)`
- **Doppeltipp**: Zentriert auf ausgewählten Standort

---

## 7. Flutter-Architektur

```
lib/ui/widgets/
├── city_map_view.dart          ← Bestehend, bleibt als Einstiegspunkt
└── iso_city_map_painter.dart   ← Neuer CustomPainter (diese Spec)
```

- `city_map_view.dart` importiert `iso_city_map_painter.dart` und verwendet `IsoMapPainter`
- `IsoMapPainter` ersetzt `_CityMapPainter` (aktuell in city_map_view.dart)

---

## 8. Mapping-Tabelle: Standort → Iso-Koordinaten

| Standort-ID | Grid Col | Grid Row | Gebäude-Höhe (Floors) | Hero |
|-------------|---------|---------|----------------------|------|
| marktplatz  | 1       | 1       | 4                    | true |
| hauptstrasse| 4       | 1       | 5                    | false |
| bahnhofsnähe | 1      | 4       | 6                    | false |
| randlage    | 4       | 4       | 3                    | false |
| fussgaengerzone | 2   | 2       | 5                    | false |
| einkaufszentrum | 3  | 2       | 7                    | false |
| bahnhof     | 2       | 3       | 6                    | false |
| wohnviertel | 3       | 3       | 3                    | false |
| innenstadt-premium | 2 | 1      | 7                    | pop |
| shoppingcenter | 3   | 1       | 6                    | false |
| uni-viertel | 1       | 2       | 5                    | false |
| stadtrand   | 4       | 3       | 3                    | false |
