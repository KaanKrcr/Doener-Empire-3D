# Map Design Spec — Premium Iso City (Mockup-Look)

Referenz für KI-Coding-Assistenten (Aider) zur Ableitung exakter
Flutter/CustomPainter-Implementierungen. Alle Maße beziehen sich auf einen
**Design-Frame 1080 × 1620 px (2:3)**. In Flutter: `scale = screenWidth / 1080`.

Starter-Implementierung: [`lib/ui/widgets/iso_city_map_painter.dart`](../lib/ui/widgets/iso_city_map_painter.dart)

## Status — Umsetzung (Stand 2026-06-14)

**Gewählter Weg: Hybrid (Sprite + Flutter-Overlays), KEIN Engine-Wechsel.**
Begründung in [`MAP_ENGINE_ENTSCHEIDUNG.md`](MAP_ENGINE_ENTSCHEIDUNG.md).

- **Feld/Nachbarn:** `IsoMapPainter` (Vektor, dunkle Nacht-Blöcke) — Fallback.
- **Aktive/eigene Filiale:** vorgerendertes Sprite `assets/iso/building_owned.png`
  (freier Standort: `building_empty.png`) via `IsoCityMapCanvas`, Halo per
  `ShaderMask` (radialer Alpha-Cut) entfernt.
- **UI-Overlays** (Label-Bubble, Header, Pins, Detail-Panel, Donut, Sparklines)
  = reine Flutter-Widgets. Voller Referenz-Screen: `lib/ui/widgets/hybrid_shop_screen.dart`.
- **Integriert in** `city_map_view.dart`; Sprite & Painter teilen die statische
  Projektion `IsoMapPainter.originFor()/projectTile()`; Tap via `toScene()`.
- **Headless-Render-Helfer** (PNG nach `build/`): `test/iso_canvas_test.dart`,
  `test/hybrid_screen_test.dart` u. a.

**Offene Punkte:** (1) Sprites brauchen echten transparenten Hintergrund
(Alpha-Cutout) statt eingebackenem Halo — aktuell per ShaderMask kaschiert.
(2) `InteractiveViewer(constrained:false)` zeigt anfangs die obere-linke Ecke
→ initiales Transform auf eigene/zentrale Filiale setzen.

> **Wichtig — Palette weicht vom aktuellen Theme ab.**
> Das Mockup ist ein *kühler* Premium-Dark-Look mit Neon-Orange. Das aktuelle
> `AppColors` in `lib/core/theme.dart` ist *warm braun-orange*. Diese Spec
> beschreibt den Mockup-Look. Mapping-Tabelle in §8. Nicht blind `AppColors`
> überschreiben — erst entscheiden, ob die Karte auf kühl umgestellt wird.

---

## 0. Master-Palette (Mockup)

```dart
// HINTERGRUND / BÜHNE
const cBgDeepest  = Color(0xFF07080A); // App-Rand, ganz außen
const cBgBase     = Color(0xFF0C0E11); // Karten-Grundfläche (Nacht-Asphalt)
const cBgPanel    = Color(0xFF121418); // Floating-Panel unten, Header-Pill
const cBgPanelHi  = Color(0xFF1A1D22); // Stat-Kacheln

// GEBÄUDE
const cRoofDark   = Color(0xFF15171B);
const cRoofMid    = Color(0xFF1E2127);
const cRoofLight  = Color(0xFF262A31); // beleuchtete Dachkante
const cWallLight  = Color(0xFF1C1F25); // linke (beleuchtete) Fassade
const cWallDark   = Color(0xFF101216); // rechte (Schatten-)Fassade
const cWinOff     = Color(0xFF0E1014); // dunkles Fenster
const cWinWarm    = Color(0xFFE8A24B); // warm beleuchtet
const cWinCool    = Color(0xFF4A6B8A); // kühl beleuchtet

// STRASSEN / BODEN
const cRoad       = Color(0xFF0A0B0D);
const cRoadEdge   = Color(0xFF16181C); // Bordstein
const cRoadLine   = Color(0xFF3A3D44); // Fahrbahnmarkierung
const cSidewalk   = Color(0xFF1A1C20);
const cGrass      = Color(0xFF14201A);
const cTree       = Color(0xFF1B2D22);
const cWater      = Color(0xFF0B1620);
const cWaterGlow  = Color(0xFF13283A);

// MARKE / NEON-AKZENT
const cAccent       = Color(0xFFF5A623); // Primär-Orange (Button, Glow-Kern)
const cAccentBright = Color(0xFFFFB845); // Glow-Höhepunkt
const cAccentDeep   = Color(0xFFE07B1A); // Verlauf-Ende
const cAccentGlow   = Color(0x66F5A623); // mit Alpha für Schein/Blur

// TEXT
const cTextTitle  = Color(0xFFF3E9D6); // Creme (Logo, Überschriften)
const cTextWhite  = Color(0xFFF5F3EF); // große Werte/Zahlen
const cTextMuted  = Color(0xFF8A8E96); // Labels
const cTextDim    = Color(0xFF5C606A); // inaktiv

// STERNE / RATING
const cStarGold   = Color(0xFFF5A623);
const cStarEmpty  = Color(0xFF3A3D44);

// KONKURRENZ
const cCompMid    = Color(0xFF5A6470); // mittleres Rating (kühl grau)
const cCompBad    = Color(0xFF2E4A3A); // schlechtes Rating (grünlich gedämpft)
const cCompChip   = Color(0xFF1C1F25);

// MISC
const cMoneyGreen = Color(0xFF7FB069);
const cNotifyDot  = Color(0xFFF5662E);
const cDivider    = Color(0xFF22252B);
```

---

## 1. Gebäude

**Isometrie:** dimetrische 2.5D-Projektion, **2:1** (horizontal:vertikal),
Winkel ≈ 26,57°. Jede Grundfläche ist eine Raute (Diamond), Höhe per Extrusion
nach oben. Kachel: `TILE_W = 64`, `TILE_H = 32`.

**Aufbau (von unten gezeichnet):**
1. Boden-Schlagschatten (Iso-Raute, nach unten-rechts versetzt).
2. Linke Fassade `cWallLight` (#1C1F25) — Licht von oben-links.
3. Rechte Fassade `cWallDark` (#101216).
4. Dach-Raute `cRoofMid` (#1E2127), Kante `cRoofLight` (#262A31) 1 px.

**Dächer:** flach, kleine Aufbauten (Klima/Lüftung) als 6–12 px Mini-Quader in
`cRoofDark`. Dachkante 1 px Highlight (#2E323A).

**Fenster:** Raster auf jeder Fassade, Fenster **8 × 12 px**, Abstand 6 px.
Verteilung deterministisch (fester Seed je Gebäude):
~70 % `cWinOff` (#0E1014), ~25 % `cWinWarm` (#E8A24B), ~5 % `cWinCool` (#4A6B8A).
Warme Fenster mit kleinem Blur-Glow (radius 2–3 px).

**Türen:** Erdgeschoss-Fenster größer (12 × 16 px), oft warm, ebenerdig.

**Texturen:** keine Bitmaps — flache Flächen + 1-px-Kantenhighlights +
subtiler vertikaler Gradient (oben +8 % heller → unten dunkler).

---

## 2. Straßen / Stadtstruktur

- **Asphalt** `cRoad` (#0A0B0D), Iso-Bänder zwischen Blöcken, Breite 50–70 px.
- **Markierung:** gestrichelte Mittellinie `cRoadLine` (#3A3D44), Strich 12 px /
  Lücke 16 px, Breite 2 px, entlang Iso-Achse.
- **Bordstein/Gehweg:** `cRoadEdge` (#16181C), 6–10 px, 1 px Highlight (#22252B).
- **Gehwegplatten:** `cSidewalk` (#1A1C20).
- **Bäume:** Krone `cTree` (#1B2D22), Iso-Kugel Ø 14–20 px, Stamm #0E1410 (3 px),
  Schlagschatten. An Gehwegkanten, leicht zufällig.
- **Grünflächen:** `cGrass` (#14201A) flache Iso-Rauten.
- **Wasser (unten links):** `cWater` (#0B1620) + horizontale Reflexstreifen
  `cWaterGlow` (#13283A) @ 30–50 %, leicht versetzt; warme Lichter als blasse
  Orange-Punkte (#E8A24B @ 15 %).

---

## 3. Eigene aktive Filiale (Hero)

Kein klassischer Pin — **leuchtendes Gebäude-Highlight + Label-Bubble**:

- **Neon-Outline:** Strich `cAccent` (#F5A623), 3 px, darunter Blur-Glow
  (`cAccentGlow`, blur 18–24 px).
- **Boden-Lichtteppich:** radialer Gradient #F5A623 @ 40 % → transparent,
  Radius ~120 px, auf dem Gehweg.
- Leuchtreklame mit Döner-Icon in Orange-Glow; Fenster überwiegend warm.

**Label-Bubble (über dem Gebäude):**
- Pill, BG `cBgPanel` @ 92 %, Border 1,5 px `cAccent`, Radius 14 px.
- Padding 16 × 10 px, ~70 px über Gebäudespitze.
- Name `cTextTitle` 28 px bold; Zeile ★ (#F5A623, 18 px) + Wert (22 px).
- Tail: Dreieck 10 px nach unten. Shadow `0x80000000`, blur 20, y 8.

---

## 4. Konkurrenz

Teardrop-Pins, farblich gedämpft (kein Orange):

- **Pin:** Kreis Ø 18 px + Spitze, Höhe ~26 px. Innenpunkt #8A8E96 Ø 6 px.
  Farbe nach Rating: schlecht → `cCompBad` (#2E4A3A), mittel → `cCompMid`
  (#5A6470). Boden-Schatten (#000 @ 40 %, blur 6).
- **Chip:** BG `cCompChip` (#1C1F25) @ 85 %, Radius 8 px, Border 1 px `cDivider`,
  Padding 10 × 6 px. Name `cTextMuted` (#8A8E96) 20 px; ★ gedämpft (#9A8456).
  **Kein Glow**, kleiner + transparenter → tritt zurück.

Beispiele: „LEZZET DÖNER ★3.2“, „CITY KEBAP ★3.6“, „BERLIN DÖNER ★2.8“.

---

## 5. Schatten & Tiefe (2.5D)

- **Zeichenreihenfolge:** Painter's Algorithm, sortiert nach `tileX + tileY`
  (hinten → vorne).
- **Schlagschatten:** je Gebäude Iso-Raute am Boden, nach unten-rechts versetzt,
  `0x66000000`, blur 8–12 px.
- **Fassaden-Shading:** links heller, rechts dunkler, Dach am hellsten.
- **Globale Vignette:** RadialGradient, Mitte transparent → Rand `cBgDeepest`
  (#07080A) @ 70 %.
- **Atmosphäre:** entferntere (obere) Gebäude leicht entsättigt + 8 % aufgehellt.

---

## 6. UI-Overlays (als Flutter-Widgets, nicht im Painter)

### 6.1 Floating Header
- App-Bar (y 40–120): Logo-Icon (64 px Rundeck, BG #15171B, Orange-Glyph) +
  „Döner Empire“ (`cTextTitle`, Serif/Display ~52 px). Rechts 3 Icons je 36 px
  (#8A8E96); Glocke mit `cNotifyDot` (#F5662E) Badge Ø 10 px.
- Status-Pill (y 130–230): BG `cBgPanel`, Radius 20, Border 1 `cDivider`,
  Padding 28. Links 💵 (#7FB069) + „€ …“ (#F5F3EF 40 px) / „KONTOSTAND“
  (#8A8E96 20 px, spacing 1,5). Trennlinie #22252B. Rechts 📅 + „Tag 47“/Wochentag.

### 6.2 Map-Steuerung (rechts vertikal)
- 3 Quadrat-Buttons je 56 px, Radius 14, BG `cBgPanel` @ 80 %, Border 1 `cDivider`,
  Gap 12, rechter Rand 24, vertikal mittig. Icons: Zentrieren, Layer, Trend.

### 6.3 Bottom-Sheet (Filial-Detail)
- Rundeck Radius 28, BG `cBgPanel`. Drag-Handle 40 × 5 (#3A3D44, Radius 3).
- Kopf: Name (#F3E9D6, Serif 46 px) + ✏️ (#5C606A 22 px); 📍 Adresse (#8A8E96
  22 px). Rechts „AKTIVE FILIALE ●“ (#F5A623) + 4½ Sterne (32 px) + „4.6“ (40 px)
  / „REPUTATION“.
- Stat-Grid (4×): Kachel BG `cBgPanelHi` (#1A1D22), Radius 16, Padding 20, Gap 12.
  - MARKTANTEIL: Donut — Track #2A2E35, Arc `cAccent`, Stroke 14, Mitte „34%“
    (#F5F3EF 36 px), Subtext „STADT: 12%“.
  - Übrige: Icon (#5C606A) → Wert (#F5F3EF 38 px) → Einheit (#8A8E96) →
    Mini-Sparkline (Polyline 2 px `cAccent`, steigend, ohne Achsen).
- Actions: „⤴ OPTIMIEREN“ Gradient #F5A623→#E07B1A, Radius 16, Höhe 88, Text
  #1A1209 bold 30 px, Glow-Shadow (`cAccentGlow` blur 24 y6). „⊕ FILIALE ÖFFNEN“
  Outline, Border 1,5 `cDivider`, Text #F3E9D6.

### 6.4 Bottom-Nav
- 5 Items (ÜBERSICHT/FILIALEN/MANAGER/FORSCHUNG/SHOP), Icon 28 + Label 16.
  Aktiv `cAccent` (#F5A623), inaktiv `cTextDim` (#5C606A).

---

## 7. Flutter-Architektur

```
IsoMapPainter extends CustomPainter      // Boden, Straßen, Wasser, Gebäude, Schatten
worldToScreen(tx, ty):
  sx = origin.dx + (tx - ty) * TILE_W/2
  sy = origin.dy + (tx + ty) * TILE_H/2     // TILE_W=64, TILE_H=32
buildings.sort((a,b) => (a.tx+a.ty).compareTo(b.tx+b.ty));
Fenster: Random(building.seed) → Glow nur für warme Fenster.
Pins/Labels: Overlay-Layer (Stack über CustomPaint), KEINE Zoom-Skalierung.
Hero-Glow: 2. Pass mit MaskFilter.blur(BlurStyle.normal, 12) in cAccentGlow.
Vignette + Bottom-Sheet als Widgets, nicht im Painter.
```

---

## 8. Mapping: Mockup-Palette ↔ bestehendes `AppColors`

| Zweck            | Mockup       | Aktuelles AppColors      | Hinweis                       |
|------------------|--------------|--------------------------|-------------------------------|
| Akzent/Marke     | #F5A623      | `primary` #E85D2F        | Mockup ist gelber, weniger rot|
| Akzent dunkel    | #E07B1A      | `primaryDark` #C44820    |                               |
| BG Basis         | #0C0E11      | `bg` #14100E             | Mockup kühl, Theme warm       |
| Panel            | #121418      | `bgCard` #1F1813         | "                             |
| Panel hell       | #1A1D22      | `bgCardHover` #2A1F18    | "                             |
| Text Titel       | #F3E9D6      | `textPrimary` #FAF4E8    | nah dran                      |
| Text muted       | #8A8E96      | `textMuted` #7A6A5C      | Mockup grau, Theme sandbraun  |
| Divider/Border   | #22252B      | `border` #2F2419         | kühl vs. braun                |
| Geld-Grün        | #7FB069      | `accent` #7BC950         | nah dran                      |
| Stern-Gold       | #F5A623      | `gold` #FFC93C           |                               |

**Entscheidung offen:** Entweder (a) Karte auf kühle Mockup-Palette umstellen
(neue `MapColors`-Gruppe), oder (b) Mockup-Töne an das warme `AppColors`
angleichen. Der Starter-Painter nutzt (a) lokal — siehe `iso_city_map_painter.dart`.
