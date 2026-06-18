# Codex: Architektur + Doku (Döner Empire City Map)

## Aufgabe 1: MAP_DESIGN_SPEC.md bereinigen

Lies `docs/MAP_DESIGN_SPEC.md` und aktualisiere:

1. Ersetze alle Verweise auf nicht mehr existierende Dateien:
   - `city_map_view.dart` → existiert nicht mehr, wurde durch `map_city_overview.dart`, `map_deutschland.dart`, `map_street_view.dart` ersetzt
   - `iso_city_map_canvas.dart` → existiert nicht mehr
2. Aktualisiere den Status-Block (datiert auf 2026-06-14, heute ist 2026-06-18)
3. Setze die kühle Mockup-Palette entweder auf den aktuellen `MapPalette`-Stand zurück oder markiere sie als veraltet
4. Füge einen klaren Verweis auf den existierenden Hybrid-Prototypen `hybrid_shop_screen.dart` und `hybrid_map_preview.dart` ein

## Aufgabe 2: Architektur-Entscheidung

Das Spiel hat aktuell 3 Kartenebenen:
1. **Deutschlandkarte** (map_deutschland.dart)
2. **Stadtplan** (map_city_overview.dart) – Bezirke als Polygone
3. **Straßenzug** (map_street_view.dart + street_building_painter.dart) – 6 Häuser, 2.5D

Das Mockup zeigt eine **zusammenhängende Iso-Stadt** auf einem Screen.

Analysiere beide Ansätze und beantworte:

**Variante A: Eine Iso-Stadt (wie Mockup)**
- Vorteile: Look & Feel, weniger Navigation, eine zusammenhängende Spielwelt
- Nachteile: Grundsatzumbau, Deutschlandkarte wird reine Expansions-UI, größere Karte = mehr Assets

**Variante B: Drill-Down behalten + jede Ebene aufwerten**
- Vorteile: Weniger Umbau, skalierbar (viele Städte möglich), schon implementiert
- Nachteile: Fragmentiert, niemals "eine Stadt auf einen Blick"

Gib eine fundierte Empfehlung mit Begründung.

## Aufgabe 3: Blender-Workflow-Dokumentation

Schreib eine ausführliche Anleitung `docs/BLENDER_SPRITE_PIPELINE.md`:

1. Blender-Setup: orthografische Iso-Kamera (26,57°, 2:1), Auflösung 512×512 oder 1024×1024
2. HDRI-Beleuchtung warm/kühl
3. Material-Setup: Fassade, Dach, Fenster (Emission), Neon (Emission)
4. Gebäude-Varianten: owned (Hero), owned_premium, competitor, empty, filler_01..08
5. Export als PNG mit Alpha (transparentem Film/Background)
6. Batch-Render per Blender-Python-Script
7. Flutter-Integration: Sprites in `assets/iso/`, Benennungskonvention, Sortierung per Fußpunkt

Nutze Web-Recherche für aktuelle Blender-Render-Empfehlungen.
