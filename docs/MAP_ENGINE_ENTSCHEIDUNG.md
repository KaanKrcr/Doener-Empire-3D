# Engine-Entscheidung — Wie erreichen wir den Mockup-Look?

Stand 2026-06-14. Kontext: Premium-Mockup (3D-Render-/KI-Bild-Qualität) für die
Döner-Empire-Stadtkarte. Frage: Welche Engine eignet sich?

## Kernaussage

**Die Engine ist nicht der Hebel — die Art-Pipeline ist es.** Die Mockup-Optik
entsteht aus *Assets* (3D-Modelle + gebackenes Licht), nicht aus Render-Code.
Ein reiner Vektor-`CustomPainter` erreicht diesen Look nie.

## Optionen

| Pfad | Look-Quelle | Flutter-only? | Aufwand |
|---|---|---|---|
| **A) Flutter + vorgerenderte Sprites** | Blender/KI → Iso-PNGs, in Flutter komponiert | ✅ | Mittel |
| B) Unity (Echtzeit-3D) | 3D-Modelle + Lighting | ❌ Engine-Wechsel | Hoch |
| C) Godot | wie Unity, FOSS | ❌ Engine-Wechsel | Hoch |

## Entscheidung: Pfad A (gewählt)

Begründung:
- Management-Sim mit **statischer Iso-Karte** — keine frei drehbare 3D-Kamera
  nötig. Genau dafür sind vorgerenderte Iso-Sprites gemacht.
- Respektiert die Vorgabe **„nur noch Flutter, kein Unity"** (2026-06-14).
- Kleinere App, kein 3D-Mobile-Tuning, kein Engine-Wechsel.

## Umsetzung (siehe MAP_DESIGN_SPEC.md → Status)

- Sprites `assets/iso/building_owned.png` / `building_empty.png` (bereits in
  Mockup-Qualität vorhanden) über dunklem Vektor-Feld (`IsoMapPainter`).
- UI (Label-Bubbles, Header, Detail-Panel, Pins) = Flutter-Widgets darüber.
- Halo der Sprites per `ShaderMask` (radialer Alpha-Cut) entfernt; langfristig
  Sprites mit echtem transparentem Hintergrund liefern.

## Wann doch eine 3D-Engine?

Nur falls echte Echtzeit-3D-Features gewollt sind: freie Kamera/Rotation,
dynamische Tageszeit/Schatten, 3D-Animationen. Dann Unity (oder Godot) — aber
das eröffnet die bewusst geschlossene „nur Flutter"-Entscheidung neu.
