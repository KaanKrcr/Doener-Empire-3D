# Iso-City-Map Sprites

Lege hier isometrische Gebäude-Sprites (PNG, transparenter Hintergrund) ab.
Sobald eine Datei mit passendem Namen existiert, nutzt die City-Map sie
automatisch statt des prozeduralen Vektor-Gebäudes (Foto-Look). Fehlt sie,
greift der Vektor-Fallback — die App funktioniert in jedem Fall.

## Erwartete Dateien (Namen exakt)

| Datei | Bedeutung |
|---|---|
| `building_owned.png` | Eigene Filiale / Hero-Döner-Restaurant (mit Neon/Schild, Nacht-Look) |
| `building_empty.png` | Freier, baubarer Standort (z. B. leeres Grundstück / Rohbau) |

(Weitere Slots — Konkurrenz, Dekor, Bäume — können in
`lib/ui/widgets/city_map_view.dart` → `IsoArt.manifest` ergänzt werden.)

## Vorgaben für die Sprites

- **Perspektive:** isometrisch (~2:1 Diamant-Grundfläche), Lichteinfall von
  rechts oben — passend zur Boden-Geometrie der Map.
- **Bodenkontakt:** unten-mittig (das Sprite wird breitenfüllend skaliert und
  mit dem Fußpunkt auf der Tile-Raute verankert; Feinanker via
  `_SpriteBuildingPainter` justierbar).
- **Auflösung:** ~512×512 px reicht für Tablet; transparenter Rand ringsum.
- **Stil:** dunkler, warmer Nacht-Look (siehe Mockup) für ein stimmiges Bild.

## Bezugsquellen

- **Kenney.nl** (CC0, kostenlos): „Isometric City" / „City Kit" Packs →
  passende Gebäude exportieren/umbenennen.
- Eigene/kommissionierte oder KI-gerenderte Iso-Sprites (am ehesten
  Mockup-Qualität, dafür Aufwand/Kosten).

## Nach dem Ablegen

1. PNGs hier ablegen (Namen exakt wie oben).
2. `flutter pub get`
3. App neu starten (neue Assets werden nicht per Hot-Reload gebündelt).
4. Map zeigt die Sprites; Vektor bleibt Fallback für fehlende Slots.
