# Codex Prompt: Döner Empire City Map – Gap-Assessment

## Aufgabe

Analysiere das Referenzbild `docs/assets/doener_empire_3d_mobile_concept.png` und die existierende Flutter-Implementierung. Erstelle ein detailliertes Gap-Assessment: Was fehlt, um die City Map **optisch exakt** auf Mockup-Niveau zu bringen.

## Referenzbild

Öffne `docs/assets/doener_empire_3d_mobile_concept.png` (und optional `docs/assets/doener_empire_mobile_premium_ui.png`).

Extrahiere daraus:
- Komplette Farbpalette (HEX-Werte für Himmel, Gebäude, Straßen, UI-Elemente, Akzente)
- UI-Layout: Header-Höhe, Panel-Positionen, Button-Größen, Abstände
- Gebäude-Stil: Dachform, Fassadenstruktur, Fenster-Raster, Material-Look
- Effekte: Neon-Glow, Schatten, Fenster-Beleuchtung, Straßen-Reflexionen
- Typografie: Schriftgrößen, -gewichte, -farben

## Existierende Implementierung (prüfen)

### Haupt-Screen
- `lib/ui/screens/city_map_screen.dart`

### Karten-Widgets (3 Ebenen)
- `lib/ui/widgets/map_deutschland.dart` – Deutschlandkarte
- `lib/ui/widgets/map_city_overview.dart` – Stadtplan (organic layout)
- `lib/ui/widgets/map_street_view.dart` – Straßenzug (2.5D)
- `lib/ui/widgets/street_building_painter.dart` – Gebäude-Painter

### Aktuelle Design-Dokumente
- `docs/MAP_DESIGN_SPEC.md` – enthält die komplette Master-Palette und Spec
- `docs/MAP_ENGINE_ENTSCHEIDUNG.md` – Engine-Entscheidung (Flutter + Sprites)
- `docs/UI_STYLE_GUIDE.md` – UI-Style-Guide

### Assets
- `assets/iso/building_owned.png` – eigenes Gebäude-Sprite (mit Halo-Problem)
- `assets/iso/building_empty.png` – freier Standort-Sprite

## Deliverables

### 1. Gap-Assessment (detailliert)

Pro Karten-Ebene eine Tabelle:

| Bereich | Mockup | Aktuell | Gap | Aufwand |
|---|---|---|---|---|
| Gebäude-Fassade | ... | ... | ... | ... |
| Fenster-Look | ... | ... | ... | ... |
| Neon-Effekte | ... | ... | ... | ... |
| Schatten | ... | ... | ... | ... |
| UI-Panels | ... | ... | ... | ... |
| usw. | ... | ... | ... | ... |

### 2. Flutter-Package-Recherche

Recherchiere ob es Flutter-Packages gibt, die helfen:
- `rive` – Animation/Vector
- `flame` + `bonfire` – Tilemaps/Game
- `shader` / `FragmentProgram` – benutzerdefinierte Shader
- `backdrop_filter` / `ImageFilter.blur` – Glow-Effekte
- `vector_graphics` – optimierte Vektordarstellung
- **Gibt es ein Package, das isometrische 2.5D-Städte rendern kann?**
- **Gibt es eine Flutter-Bridge zu einer 3D-Engine für Teil-Rendering?**

### 3. Verbesserungsvorschläge (priorisiert)

Sortiert nach Impact/Aufwand:
- **P0** – Schnelle CustomPainter-Optimierungen (Fenster, Schatten, Farben)
- **P1** – Sprite-basierte Verbesserungen (neue Gebäude-PNGs aus Blender)
- **P2** – UI-Feintuning (Pixel-perfect nach Mockup)
- **P3** – Alternative: Rive/Flame/andere Packages
- **P4** – Grundsatzentscheidung: Engine-Wechsel nötig?

### 4. Blender-Workflow (falls nötig)

Wenn Flutter allein nicht reicht, beschreibe den minimalen Blender-Pipeline:
- Kamerasetup (orthographisch, Iso-Winkel 26,57°, 2:1)
- HDRI-Beleuchtung
- PBR-Materialien
- Export als PNG mit Alpha (transparenter Hintergrund)
- Welche Gebäude-Varianten werden benötigt (owned, premium, competitor, empty, filler)?

### 5. Code-Verbesserungen (optional)

Wenn du konkrete Code-Änderungen siehst, die den Look signifikant verbessern, schreib sie direkt als Vorschlag oder Diff.

## Validierung

Nach Änderungen immer:
- `flutter analyze` – 0 issues
- `flutter test` – alle grün (aktuell >120 Tests)
- Optional: `flutter build apk --debug`
