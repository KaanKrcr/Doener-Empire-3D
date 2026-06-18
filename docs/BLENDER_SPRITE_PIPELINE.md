# Blender Sprite Pipeline — Döner Empire 3D

Diese Pipeline erzeugt reproduzierbare isometrische PNG-Sprites für die
Flutter-City-Map. Blender ist ausschließlich ein Offline-Art-Werkzeug; zur
Laufzeit rendert Flutter eine 2D-Iso-Tilemap.

## 1. Ziel und feste Konventionen

Alle Sprites müssen dieselbe Projektion, Kamera, Beleuchtung und Verankerung
verwenden. Nur dann lassen sie sich ohne sichtbare Perspektivsprünge auf einer
gemeinsamen Karte kombinieren.

### Ausgabegrößen

| Assettyp | Ausgabe |
|---|---:|
| Füllgebäude, Bäume, kleine Dekoration | 512 × 512 px |
| Hero-Filiale, große Konkurrenzgebäude | 1024 × 1024 px |
| Landmarken | 1024 × 1024 px |

Render immer quadratisch. Das sichtbare Objekt darf den Rahmen nicht berühren
und benötigt ausreichend transparenten Rand für Schatten und Glow.

### Weltmaßstab

- Eine Blender-Einheit entspricht einem Meter.
- Ein Standardgrundstück ist beispielsweise 16 × 16 Meter.
- Der Objektursprung liegt auf Bodenhöhe im logischen Fußpunkt.
- Gebäude mit mehreren Tiles verwenden den vordersten mittleren Bodenpunkt als
  gemeinsamen Flutter-Anker.
- Alle Varianten einer Gebäudefamilie müssen denselben Footprint behalten.

## 2. Orthografisches Iso-Kamera-Setup

Die Karte nutzt eine 2:1-Dimetrie. Das projizierte Tile ist doppelt so breit wie
hoch; der sichtbare Achsenwinkel beträgt ungefähr **26,57°**.

### Empfohlenes Kamera-Preset

1. Kamera auf `ORTHO` stellen.
2. Kamera um die Welt-Z-Achse auf **45°** ausrichten.
3. Kamera so neigen, dass die Weltachsen im Bild ungefähr 26,57° zur
   Horizontalen bilden. Als reproduzierbarer Startwert:
   - Rotation X: **60°**
   - Rotation Z: **45°**
4. Kamera auf den Fußpunkt des Assets ausrichten.
5. `ortho_scale` pro Assetklasse fest definieren:
   - Standardgebäude: ein gemeinsamer Wert für alle 512er Exporte
   - Hero/Landmark: ein gemeinsamer Wert für alle 1024er Exporte
6. Niemals Perspektivkamera, Brennweitenänderung oder individuelle
   Kamerarotation pro Asset verwenden.

Blender-Rotationswerte können je nach Kamerarig und Euler-Reihenfolge anders
angezeigt werden. Entscheidend ist ein gesperrtes Master-Kameraobjekt und die
visuelle 2:1-Prüfung:

- Eine quadratische Bodenplatte muss als symmetrische Raute erscheinen.
- Rautenbreite zu Rautenhöhe muss 2:1 betragen.
- Beide horizontalen Weltachsen müssen im Bild denselben Winkel besitzen.

### Renderrahmen

- Der Fußpunkt liegt horizontal in der Bildmitte.
- Seine vertikale Position bleibt innerhalb einer Assetklasse identisch.
- Der unterste sichtbare Pixel ist nicht automatisch der Anker; Schatten dürfen
  unter den Fußpunkt reichen.
- Lege im Masterfile ein nicht renderndes Empty `SPRITE_ANCHOR` am Fußpunkt an.
- Lege ein zweites Empty `CAMERA_TARGET` an und richte die Kamera per
  `Track To` oder reproduzierbarer Rotation darauf aus.

## 3. Render Engine und Farbmanagement

Für schnelle Serienproduktion ist EEVEE die Standard-Engine. Cycles darf für
Final-Assets verwendet werden, wenn sämtliche Varianten mit identischen
Settings neu gerendert werden.

Empfohlen:

- Color Management: **AgX**
- Look: `Medium High Contrast` oder ein projektweit gesperrtes eigenes Preset
- Exposure: zentral im Masterfile, nicht pro Asset
- transparente Filmfläche aktivieren
- Ambient Occlusion und Contact Shadows verwenden
- Schatten weich halten, aber den Bodenkontakt klar lesbar lassen
- keine Depth-of-Field-Unschärfe auf Einzelassets
- Motion Blur deaktivieren

Texturen und Farben müssen im finalen Flutter-Hintergrund geprüft werden.
Ein Sprite, das auf dem Blender-Schachbrett gut aussieht, kann auf
`MapPalette.bgBase` zu dunkel oder zu kontrastarm wirken.

## 4. HDRI- und Licht-Preset

Das HDRI liefert nur Umgebung und Reflexionen. Die visuelle Richtung kommt von
kontrollierten Lampen.

### World/HDRI

Node-Aufbau:

```text
Environment Texture (HDRI)
→ Background
→ World Output
```

- neutrales oder leicht kühles Nacht-/Dämmerungs-HDRI verwenden
- Strength niedrig halten, typischer Startbereich `0.15–0.35`
- HDRI-Rotation im Masterfile sperren
- dieselbe HDRI-Datei und Rotation für alle Assets nutzen
- HDRI niemals als sichtbaren Hintergrund exportieren

### Key Light

- großes Area Light von oben links/vorne
- warmes Licht, etwa 2800–3600 K
- formt Fassade, Markisen und Eingänge
- weiche Schatten durch ausreichend große Light-Size

### Fill/Rim Light

- schwächeres Area Light von rechts/hinten
- neutral bis kühl, etwa 5500–7500 K
- trennt dunkle Dach- und Fassadenkanten vom Hintergrund
- darf die warme Markenbeleuchtung nicht überstrahlen

### Praktische Lichter

- Fenster und Innenräume über Emission plus diskrete Area/Point Lights
- Neon und Schilder über Emission
- zusätzliche Lichter nur in Collections, die mit der jeweiligen
  Assetvariante aktiviert werden

Alle Lichtobjekte liegen in einer gesperrten Collection `LIGHTING_MASTER`.

## 5. PBR-Materialien

Materialien basieren auf `Principled BSDF`. Texturen verwenden nach Möglichkeit
Base Color, Roughness und Normal; Metallic wird nur für tatsächliche Metalle
verwendet.

### Fassade

- Metallic: `0.0`
- Roughness: `0.55–0.85`
- subtile Roughness- und Normalvariation
- keine fotorealistischen Hochfrequenztexturen, die bei 512 px flimmern
- Kanten mit kleinen Bevels versehen, damit sie Licht fangen

Varianten:

- Putz: hohe Roughness, schwache Normalstruktur
- Ziegel: sichtbares, aber größenrichtiges Muster
- Beton: geringe Farbvariation, mittlere bis hohe Roughness
- Glasfront: separate Materialzone, nicht als lackierte Fassade simulieren

### Dach

- dunkler als die beleuchtete Fassade
- Roughness `0.45–0.75`
- Metall nur bei Blechdächern
- Dachaufbauten und Lüftungskästen über echte Silhouetten statt aufgemalter
  Details

### Fenster und Glas

- Principled BSDF mit niedriger Roughness und kontrollierter Transmission
- beleuchtete Fenster erhalten zusätzlich Emission
- Innenraumtiefe über einfache Geometrie oder Parallax-Flächen erzeugen
- nicht jedes Fenster beleuchten; deterministische Muster pro Variante nutzen

Empfohlene Emissionsstärke für Fenster als Startwert: `1.5–4.0`. Der finale
Wert hängt von Exposure und Render Engine ab.

### Neon und Markenschilder

- Emission Color aus der warmen Kartenpalette, primär `#F07010`
- Emission Strength als Startwert `5–20`
- sichtbaren Leuchtkörper und Glow getrennt beurteilen
- Bloom/Fog Glow sparsam einsetzen
- Glow darf nicht bis an den Bildrand reichen
- der Footprint und die Alpha-Silhouette müssen trotz Glow klar bleiben

## 6. Schatten, Alpha und Compositing

### Transparenter Export

In den Render Properties:

- Film → `Transparent` aktivieren
- File Format: `PNG`
- Color: `RGBA`
- Color Depth: `8` für reguläre Runtime-Assets
- Compression: projektweit einheitlich

Der World-/HDRI-Hintergrund beleuchtet weiterhin die Szene, wird aber nicht in
die Ausgabe gerendert.

### Bodenschatten

Ein Sprite benötigt einen lesbaren Bodenkontakt, ohne ein sichtbares
rechteckiges Bodenbild zu exportieren.

Empfohlene Methode:

- transparente Shadow-Catcher-/Ground-Geometrie innerhalb des Asset-Footprints
  verwenden oder
- Schatten als separaten Renderpass exportieren und kontrolliert in die
  Sprite-RGBA-Ausgabe komponieren.

Der Schatten darf den logischen Footprint moderat überschreiten, aber keine
fremden Tiles vollständig abdunkeln.

### Glow

Für Bloom den Compositor-Node `Glare` im Modus `Fog Glow` verwenden. Der
Effekt bleibt Teil des RGBA-Sprites, muss aber auf vollständig transparenten
Pixeln sauber auslaufen.

Prüfung nach jedem Export:

- Alpha an allen vier Bildecken ist 0.
- Kein brauner, schwarzer oder HDRI-farbener Rechteckhintergrund.
- Keine harte Alpha-Kante um Bloom oder Schatten.
- Der Fußpunkt hat zwischen Varianten dieselben Pixelkoordinaten.

## 7. Collections und Assetvarianten

Die Blender-Datei verwendet eine gemeinsame Struktur:

```text
CAMERA_RIG
LIGHTING_MASTER
GROUND_HELPERS
ASSET_building_owned
ASSET_building_owned_premium
ASSET_building_competitor_01
ASSET_building_empty
ASSET_building_filler_01
...
```

Nur Collections mit dem Präfix `ASSET_` werden vom Batch-Script exportiert.
Gemeinsame Collections bleiben immer aktiv.

### Verbindliche Runtime-Namen

```text
building_owned.png
building_owned_premium.png
building_competitor_01.png
building_competitor_02.png
building_competitor_03.png
building_empty.png
building_filler_01.png
building_filler_02.png
...
building_filler_08.png

landmark_station.png
landmark_university.png
landmark_downtown.png

tree_01.png
tree_02.png
vehicle_car_01.png
vehicle_van_01.png
decor_bench_01.png
decor_lamp_01.png
```

Weitere Varianten folgen demselben Schema:

```text
<category>_<role-or-name>_<two-digit-variant>.png
```

Dateinamen sind kleingeschrieben, ASCII-kompatibel und verwenden nur
Unterstriche.

## 8. Batch-Render mit Blender-Python

Das Batch-Script wird zusammen mit dem Master-`.blend` versioniert. Es rendert
jede `ASSET_`-Collection isoliert und bricht bei inkonsistentem Setup ab.

### Erforderliche Prüfungen

Vor dem ersten Render:

- aktive Kamera existiert und ist orthografisch
- `SPRITE_ANCHOR` existiert
- Ausgabeauflösung ist 512 oder 1024
- Film ist transparent
- Dateiformat ist PNG/RGBA
- jede Asset-Collection enthält renderbare Objekte
- jedes Asset besitzt einen gültigen Footprint beziehungsweise Anchor
- Zieldateinamen sind eindeutig

### Beispielstruktur

```python
from pathlib import Path
import bpy

OUTPUT_DIR = Path(bpy.path.abspath("//../assets/iso"))
ALWAYS_VISIBLE = {"CAMERA_RIG", "LIGHTING_MASTER", "GROUND_HELPERS"}


def asset_collections():
    return sorted(
        (c for c in bpy.data.collections if c.name.startswith("ASSET_")),
        key=lambda c: c.name,
    )


def set_collection_visibility(active):
    for collection in bpy.data.collections:
        enabled = collection.name in ALWAYS_VISIBLE or collection == active
        collection.hide_render = not enabled


def validate_scene():
    scene = bpy.context.scene
    camera = scene.camera
    if camera is None or camera.type != "ORTHO":
        raise RuntimeError("Active orthographic camera is required")
    if bpy.data.objects.get("SPRITE_ANCHOR") is None:
        raise RuntimeError("SPRITE_ANCHOR is missing")

    scene.render.film_transparent = True
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.image_settings.color_depth = "8"


def output_name(collection):
    return collection.name.removeprefix("ASSET_").lower() + ".png"


def render_all():
    validate_scene()
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    collections = asset_collections()
    if not collections:
        raise RuntimeError("No ASSET_ collections found")

    for collection in collections:
        set_collection_visibility(collection)
        bpy.context.scene.render.filepath = str(
            OUTPUT_DIR / output_name(collection)
        )
        bpy.ops.render.render(write_still=True)


render_all()
```

Das Produktionsscript muss zusätzlich variantenspezifische Auflösung,
Footprint-Metadaten und Pixelanker prüfen. Es soll bei einem Fehler mit
ungleich null enden, statt unvollständige Assets still zu überspringen.

### Aufruf

```powershell
blender --background path\to\doener_iso_assets.blend `
  --python path\to\render_iso_assets.py
```

In CI oder automatisierten Art-Builds muss eine feste Blender-Version verwendet
werden, da Render Engine, AgX und Compositor zwischen Versionen abweichen
können.

## 9. Flutter-Integration

### Ablage

Finale Runtime-Assets liegen unter:

```text
assets/iso/
```

Das Verzeichnis ist bereits in `pubspec.yaml` registriert:

```yaml
flutter:
  assets:
    - assets/iso/
```

Nach neuen Assets ist ein kompletter Neustart beziehungsweise neuer Build
notwendig; Hot Reload bündelt neue Dateien nicht zuverlässig nach.

### Fußpunkt und Projektion

- Standardanker: unten mittig relativ zum logischen Footprint
- Flutter positioniert das Sprite anhand des projizierten Tile-Fußpunkts
- Pixelanker werden pro Assetklasse zentral definiert, nicht an jeder
  Verwendungsstelle
- Sprite-Größe und Karten-Zoom dürfen die Weltkoordinate nicht verändern

### Renderreihenfolge

Entities werden stabil sortiert nach:

```text
tileX + tileY
→ elevation
→ entityId
```

Bei mehrteiligen Footprints zählt der vorderste Bodenpunkt.

### Flutter-Overlays

Folgende Inhalte werden nie in Blender eingebrannt:

- Filialname
- Reputation und Sterne
- Preise, Miete, Traffic und Marktanteil
- Auswahlstatus
- Konkurrenz- oder Risikohinweise
- Buttons und Kartensteuerung

Flutter projiziert den Tile-Fußpunkt und positioniert darüber Label-Bubbles,
Pins und Auswahlglows. Dadurch bleiben Texte lokalisiert, dynamisch und bei
Zoom lesbar.

### Asset-Fallback

Während der Migration darf ein einfacher Vektor- oder Placeholder-Fallback
verwendet werden, wenn ein Asset fehlt. Fehlende produktive Assetkeys müssen in
Debug/Test sichtbar gemeldet werden; Release-Builds dürfen nicht still das
falsche Gebäude anzeigen.

## 10. Qualitätskontrolle

Jedes neue Sprite wird vor Integration geprüft:

- korrekte 2:1-Projektion
- identische Licht- und Kamerarichtung
- transparentes PNG ohne Rechteck-Halo
- korrekter Fußpunkt
- ausreichender Rand für Schatten und Glow
- lesbar auf `MapPalette.bgBase`
- keine abgeschnittene Geometrie
- keine eingebrannten dynamischen Texte
- Dateiname entspricht der Konvention
- 512er und 1024er Assets wirken bei ihrer vorgesehenen Zoomstufe scharf

Zusätzlich sollte ein automatischer Kontaktbogen alle Assets auf dem echten
Kartenhintergrund rendern. Perspektiv-, Maßstabs- und Farbausreißer werden dort
schneller sichtbar als in Einzeldateien.

## Offizielle Blender-Referenzen

- [World Environment](https://docs.blender.org/manual/en/latest/render/lights/world.html)
- [Environment Texture](https://docs.blender.org/manual/en/latest/render/shader_nodes/textures/environment.html)
- [Principled BSDF](https://docs.blender.org/manual/en/latest/render/shader_nodes/shader/principled.html)
- [Emission Shader](https://docs.blender.org/manual/en/latest/render/shader_nodes/shader/emission.html)
- [Glare Node](https://docs.blender.org/manual/en/latest/compositing/types/filter/glare.html)
- [Command Line Arguments](https://docs.blender.org/manual/en/latest/advanced/command_line/arguments.html)
- [Blender Python API](https://docs.blender.org/api/current/bpy.app.html)
