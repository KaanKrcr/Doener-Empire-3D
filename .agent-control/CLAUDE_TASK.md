# CLAUDE TASK: Gebäude-Detail + Stadtplan-Design verbessern (mit Referenzbild)

## Referenzbild
**Pfad:** `docs/assets/doener_empire_3d_mobile_concept.png`

Öffne diese Datei mit `/open` oder `file`-Tool. Das Bild ist die **Art-Direction-Vorlage** für ALLE optischen Änderungen.

## Kritikpunkte (Kaan, getestet)

1. **Stadtplan (Vogelperspektive) ist zu leer und billig**  
   Sieht aus wie eine 4×3-Tabelle mit Strassenlinien. Kein Stadt-Gefühl.

2. **Gebäude im 2.5D-Straßenzug sind undetailliert und billig**  
   Qualität entspricht nicht dem Design-Stil aus dem Referenzbild.

3. **Deutschland-Karte: Grenze nicht exakt**  
   Sollte erkennbar Deutschland sein.

## Was zu tun ist

### 1. Gebäude im Straßenzug (street_building_painter.dart)
**Baue die Gebäude basierend auf dem Referenzbild um:**

Dem Referenzbild nachempfundene Gebäude-Elemente:
- **Fassaden:** Dunkle Anthrazit-Töne (`#0E1014` bis `#1C1F25`), mit **vertikalem Farbverlauf** (oben heller, unten dunkler)
- **Dächer:** Flachdach mit Klimaanlagen/Lüftungskästen als kleine Quader in `#15171B`
- **Dachkante:** 1px Highlight `#262A31`
- **Fenster:** Deterministiches Raster (Seed pro Gebäude)
  - ~70% aus (`#0E1014`)
  - ~25% warm (`#E8A24B`) mit **Glow** (blur 2-3px, `#44E8A24B`)
  - ~5% kühl/blau (`#4A6B8A`)
  - Erdgeschoss: grössere Türen/Fenster (12×16px statt 8×12px)
  - Fensterrahmen 1px (`#22252B`), sichtbar bei warmen Fenstern
- **Schatten:** Iso-Raute am Boden, `#66000000`, blur 10px, nach unten-rechts versetzt
- **Fassaden-Shading:** linke Seite `#1C1F25`, rechte Seite `#101216`
- **Neon-Outline für Hero:** `#F5A623`, 3px, plus Blur-Glow 18px
- **Boden-Glow:** Radialgradient `#F5A623@40%` → transparent, Radius 120px
- **Konkurrenz-Filialen:** Kein Orange, gedämpfte Farbtöne (`#5A6470` / `#2E4A3A`), kleiner Teardrop-Pin, kein Glow

### 2. Stadtplan (map_city_overview.dart) — Komplett überarbeiten
**Nicht mehr 4×3-Tabelle. Stattdessen:**

- **Organische Stadt-Struktur**: Kein Grid! Verwende Pixel-Koordinaten für jedes Gebiet
- **Standort-Flächen** als unregelmässige Polygone/Pfade statt Rechtecke
- **Strassen** die zwischen den Gebieten verlaufen:
  - Asphalt `#0A0B0D`
  - Fahrbahnmarkierung `#3A3D44`, gestrichelt (Strich 12px, Lücke 16px)
  - Bordstein `#16181C`, 1px Highlight
  - Gehweg `#1A1C20`
- **Wasserfläche** (unten links oder rechts): `#0B1620` + horizontale Reflexstreifen `#13283A`
- **Grünflächen** (Park): `#14201A`, Bäume als kleine grüne Kreise
- **Bezirksnamen** in der Mitte jeder Fläche (cremeweiss, Bold, 18px)
- **Infos unter dem Namen:** Kurzbeschreibung (sandfarben, 11px)
- **Marker** auf den Flächen:
  - Orange 🥙 für eigene Filialen
  - Rot 👤 für Konkurrenz  
  - Grün 📋 für freie Standorte
- **Hintergrund:** Sanfter Verlauf `#0C0E11` → `#07080A`

### 3. Deutschlandkarte (map_deutschland.dart) — Grenze verbessern
Nutze diese approximierten Koordinaten für Deutschlands Umriss (vereinfacht, skalierbar auf Display):

```
/*
 * Deutschland-Umriss (stark vereinfachte Polygon-Punkte)
 * Skaliert auf Display-Grösse.
 * Quelle: Ungefähre Kontur, 0.0-1.0 normalisiert
 *
 * Beispiel-Punkte (tx, ty rel. zu Bildmitte):
 * Norden:    (0.5, 0.05)  — Kiel/Flensburg
 * Nordost:   (0.85, 0.1)  — Usedom
 * Osten:     (0.9, 0.35)  — Görlitz
 * Südost:    (0.8, 0.6)   — Passau
 * Süden:     (0.55, 0.75) — Oberstdorf
 * Südwest:   (0.3, 0.65)  — Saarbrücken
 * Westen:    (0.2, 0.35)  — Aachen
 * Nordwest:  (0.25, 0.1)  — Borkum/Nordsee
 */
```

Zeichne die Grenze als `Path` mit:
- Füllung `#121418` (leicht transparent, mit Border `#22252B` 2px)
- Städte als Punkte (grün `#7BC950` für verfügbar, grau `#5C606A` für gesperrt)
- Stadt-Name rechts vom Punkt

## WICHTIG: Datei-Struktur
- **ALLE flachen Flat-2D-Dateien (`flat_*.dart`) bitte löschen** — die waren von einem vorherigen Versuch und werden nicht mehr gebraucht
- Nur folgende Dateien bearbeiten/erstellen:
  - `lib/ui/widgets/street_building_painter.dart` — Gebäude-Painter verbessern (Referenzbild!)
  - `lib/ui/widgets/map_city_overview.dart` — Stadtplan überarbeiten
  - `lib/ui/widgets/map_deutschland.dart` — Grenze verbessern

## Validierung
```bash
flutter analyze      # 0 issues
flutter test         # alle grün (aktuell 112)
flutter build apk --debug  # erfolgreich
```
