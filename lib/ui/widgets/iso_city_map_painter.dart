import 'dart:math' as math;

import 'package:flutter/material.dart';

/// Premium-Iso-Stadtkarte im Stil des Döner-Empire-Mockups (Nacht-City mit
/// Orange-Neon-Hero). Eigenständiger Startpunkt — bewusst NICHT an die echten
/// City-/Shop-Daten gebunden, damit nichts am bestehenden [CityMapView]
/// kaputtgeht. Farben sind hier lokal definiert ([MapColors]), weil die
/// Mockup-Palette kühler ist als das warme `AppColors`-Set in theme.dart.
/// Bei Übernahme ins finale Theme können sie nach AppColors wandern.
///
/// Spec: docs/MAP_DESIGN_SPEC.md
///
/// Schnelltest:
/// ```dart
/// Scaffold(body: IsoCityMapDemo())
/// ```

/// Dimetrische 2:1-Iso-Kachel.
const double kTileW = 64;
const double kTileH = 32;

/// Höhe einer Etage in Pixeln (Wand-Extrusion).
const double kFloorH = 18;

/// Kühle Mockup-Palette (siehe Spec §0). Lokal, um theme.dart nicht zu berühren.
class MapColors {
  const MapColors._();

  static const bgDeepest = Color(0xFF07080A);
  static const bgBase = Color(0xFF0C0E11);

  static const roofMid = Color(0xFF1E2127);
  static const roofLight = Color(0xFF262A31);
  static const wallLight = Color(0xFF1C1F25);
  static const wallDark = Color(0xFF101216);

  static const winOff = Color(0xFF0E1014);
  static const winWarm = Color(0xFFE8A24B);
  static const winCool = Color(0xFF4A6B8A);

  static const road = Color(0xFF0A0B0D);
  static const roadLine = Color(0xFF3A3D44);

  static const accent = Color(0xFFF5A623);
  static const accentGlow = Color(0x66F5A623);
}

/// Ein Gebäude auf dem Iso-Raster. Footprint = 1 Kachel.
class IsoBuilding {
  /// Kachel-Koordinaten (Spalte/Zeile im Raster).
  final int tx;
  final int ty;

  /// Höhe in Etagen → Pixelhöhe = floors * kFloorH.
  final int floors;

  /// Seed für die deterministische Fensterverteilung.
  final int seed;

  /// Eigene aktive Filiale → Neon-Outline + Boden-Glow + warme Fenster.
  final bool hero;

  const IsoBuilding({
    required this.tx,
    required this.ty,
    required this.floors,
    this.seed = 0,
    this.hero = false,
  });

  int get depth => tx + ty;
  double get heightPx => floors * kFloorH;
}

class IsoMapPainter extends CustomPainter {
  final List<IsoBuilding> buildings;

  /// Manueller Pan-Offset (optional, für späteres Scrollen/Zoomen).
  final Offset pan;

  IsoMapPainter({required this.buildings, this.pan = Offset.zero});

  @override
  void paint(Canvas canvas, Size size) {
    _paintBackground(canvas, size);

    // Raster mittig platzieren: Schwerpunkt der Kacheln auf ~45 % Höhe.
    final origin = _computeOrigin(size) + pan;

    // Hero-Boden-Glow ZUERST (liegt unter den Gebäuden).
    for (final b in buildings.where((b) => b.hero)) {
      _paintGroundGlow(canvas, _worldToScreen(b, origin));
    }

    _paintRoads(canvas, origin);

    // Painter's Algorithm: hinten (kleiner depth) zuerst.
    final sorted = [...buildings]..sort((a, b) => a.depth.compareTo(b.depth));
    for (final b in sorted) {
      _paintBuilding(canvas, b, origin);
    }

    // Neon-Outline der Hero-Gebäude on top.
    for (final b in sorted.where((b) => b.hero)) {
      _paintHeroOutline(canvas, b, origin);
    }

    _paintVignette(canvas, size);
  }

  // ── Boden & Atmosphäre ────────────────────────────────────────────────────

  void _paintBackground(Canvas canvas, Size size) {
    final paint = Paint()
      ..shader = const LinearGradient(
        colors: [MapColors.bgBase, MapColors.bgDeepest],
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
      ).createShader(Offset.zero & size);
    canvas.drawRect(Offset.zero & size, paint);
  }

  void _paintVignette(Canvas canvas, Size size) {
    final rect = Offset.zero & size;
    final paint = Paint()
      ..shader = RadialGradient(
        center: const Alignment(0, -0.1),
        radius: 0.9,
        colors: const [Color(0x00000000), Color(0xB3070809)],
        stops: const [0.55, 1.0],
      ).createShader(rect);
    canvas.drawRect(rect, paint);
  }

  void _paintGroundGlow(Canvas canvas, Offset center) {
    final paint = Paint()
      ..shader = RadialGradient(
        colors: const [Color(0x66F5A623), Color(0x00F5A623)],
      ).createShader(Rect.fromCircle(center: center, radius: 120))
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 16);
    canvas.drawCircle(center, 120, paint);
  }

  /// Zwei kreuzende Iso-Straßen als Orientierung (Starter — echte Straßen =
  /// leere Kachelreihen im Raster, siehe Spec §2).
  void _paintRoads(Canvas canvas, Offset origin) {
    final road = Paint()
      ..color = MapColors.road
      ..strokeWidth = kTileH * 0.9
      ..strokeCap = StrokeCap.round
      ..style = PaintingStyle.stroke;
    final line = Paint()
      ..color = MapColors.roadLine
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    Offset iso(double tx, double ty) => Offset(
          origin.dx + (tx - ty) * kTileW / 2,
          origin.dy + (tx + ty) * kTileH / 2,
        );

    for (final spec in [
      [iso(-1, 3), iso(7, 3)], // horizontale Iso-Achse
      [iso(3, -1), iso(3, 7)], // vertikale Iso-Achse
    ]) {
      canvas.drawLine(spec[0], spec[1], road);
      _dashedLine(canvas, spec[0], spec[1], line, dash: 12, gap: 16);
    }
  }

  void _dashedLine(Canvas canvas, Offset a, Offset b, Paint paint,
      {required double dash, required double gap}) {
    final total = (b - a).distance;
    final dir = (b - a) / total;
    double t = 0;
    while (t < total) {
      final start = a + dir * t;
      final end = a + dir * math.min(t + dash, total);
      canvas.drawLine(start, end, paint);
      t += dash + gap;
    }
  }

  // ── Gebäude ───────────────────────────────────────────────────────────────

  Offset _worldToScreen(IsoBuilding b, Offset origin) => Offset(
        origin.dx + (b.tx - b.ty) * kTileW / 2,
        origin.dy + (b.tx + b.ty) * kTileH / 2,
      );

  void _paintBuilding(Canvas canvas, IsoBuilding b, Offset origin) {
    final base = _worldToScreen(b, origin);
    final h = b.heightPx;

    // Diamant-Eckpunkte der Grundfläche.
    final n = base + Offset(0, -kTileH / 2); // Nord (hinten)
    final e = base + Offset(kTileW / 2, 0); // Ost (rechts)
    final s = base + Offset(0, kTileH / 2); // Süd (vorne)
    final w = base + Offset(-kTileW / 2, 0); // West (links)
    final up = Offset(0, -h);

    // Schlagschatten am Boden (nach unten-rechts versetzt).
    final shadow = Path()
      ..addPolygon([n, e, s, w], true);
    canvas.drawPath(
      shadow.shift(const Offset(6, 8)),
      Paint()
        ..color = const Color(0x66000000)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 10),
    );

    // Sichtbare Fassaden (links beleuchtet, rechts im Schatten).
    final leftWall = Path()..addPolygon([w, s, s + up, w + up], true);
    final rightWall = Path()..addPolygon([s, e, e + up, s + up], true);
    canvas.drawPath(leftWall, Paint()..color = MapColors.wallLight);
    canvas.drawPath(rightWall, Paint()..color = MapColors.wallDark);

    // Fenster.
    _paintWindows(canvas, w + up, s + up, h, b, MapColors.wallLight);
    _paintWindows(canvas, s + up, e + up, h, b, MapColors.wallDark);

    // Dach.
    final roof = Path()..addPolygon([n + up, e + up, s + up, w + up], true);
    canvas.drawPath(roof, Paint()..color = MapColors.roofMid);
    canvas.drawPath(
      roof,
      Paint()
        ..color = MapColors.roofLight
        ..strokeWidth = 1
        ..style = PaintingStyle.stroke,
    );
  }

  /// Fenstergitter auf einer Fassade. [topA]→[topB] ist die obere Kante,
  /// [h] die Wandhöhe nach unten.
  void _paintWindows(
      Canvas canvas, Offset topA, Offset topB, double h, IsoBuilding b, Color wall) {
    final rng = math.Random(b.seed ^ (topA.dx * 7).round());
    final edge = topB - topA;
    final cols = (edge.distance / 14).floor().clamp(1, 5);
    final rows = b.floors.clamp(1, 7);
    final down = Offset(0, h); // von Oberkante nach unten

    Offset p(double u, double v) => topA + edge * u + down * v;

    for (var c = 0; c < cols; c++) {
      for (var r = 0; r < rows; r++) {
        final u0 = (c + 0.22) / cols, u1 = (c + 0.78) / cols;
        final v0 = (r + 0.25) / rows, v1 = (r + 0.78) / rows;
        final quad = Path()
          ..addPolygon([p(u0, v0), p(u1, v0), p(u1, v1), p(u0, v1)], true);

        final roll = rng.nextDouble();
        final Color color;
        if (b.hero) {
          color = roll < 0.7 ? MapColors.winWarm : MapColors.winOff;
        } else if (roll < 0.70) {
          color = MapColors.winOff;
        } else if (roll < 0.95) {
          color = MapColors.winWarm;
        } else {
          color = MapColors.winCool;
        }
        canvas.drawPath(quad, Paint()..color = color);

        if (color == MapColors.winWarm) {
          canvas.drawPath(
            quad,
            Paint()
              ..color = MapColors.winWarm.withAlpha(120)
              ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 2),
          );
        }
      }
    }
  }

  void _paintHeroOutline(Canvas canvas, IsoBuilding b, Offset origin) {
    final base = _worldToScreen(b, origin);
    final h = b.heightPx;
    final up = Offset(0, -h);
    final n = base + Offset(0, -kTileH / 2) + up;
    final e = base + Offset(kTileW / 2, 0) + up;
    final s = base + Offset(0, kTileH / 2) + up;
    final w = base + Offset(-kTileW / 2, 0) + up;
    final sBase = base + Offset(0, kTileH / 2);
    final eBase = base + Offset(kTileW / 2, 0);
    final wBase = base + Offset(-kTileW / 2, 0);

    // Outline: Dachkante + senkrechte Vorderkanten.
    final outline = Path()
      ..addPolygon([n, e, s, w], true)
      ..moveTo(w.dx, w.dy)
      ..lineTo(wBase.dx, wBase.dy)
      ..moveTo(s.dx, s.dy)
      ..lineTo(sBase.dx, sBase.dy)
      ..moveTo(e.dx, e.dy)
      ..lineTo(eBase.dx, eBase.dy);

    // Glow-Pass.
    canvas.drawPath(
      outline,
      Paint()
        ..color = MapColors.accentGlow
        ..style = PaintingStyle.stroke
        ..strokeWidth = 8
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 12),
    );
    // Scharfe Kernlinie.
    canvas.drawPath(
      outline,
      Paint()
        ..color = MapColors.accent
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3
        ..strokeJoin = StrokeJoin.round,
    );
  }

  // ── Layout-Helfer ───────────────────────────────────────────────────────

  /// Verschiebt das Raster so, dass sein Mittelpunkt bei ~(50 %, 45 %) liegt.
  Offset _computeOrigin(Size size) {
    if (buildings.isEmpty) return Offset(size.width / 2, size.height * 0.45);
    final cx = buildings.map((b) => b.tx).reduce((a, b) => a + b) / buildings.length;
    final cy = buildings.map((b) => b.ty).reduce((a, b) => a + b) / buildings.length;
    final centerScreen = Offset(
      (cx - cy) * kTileW / 2,
      (cx + cy) * kTileH / 2,
    );
    return Offset(size.width / 2, size.height * 0.45) - centerScreen;
  }

  @override
  bool shouldRepaint(covariant IsoMapPainter old) =>
      old.buildings != buildings || old.pan != pan;
}

/// Lauffähige Demo: 7×7-Block-Stadt mit einer Hero-Filiale in der Mitte.
class IsoCityMapDemo extends StatelessWidget {
  const IsoCityMapDemo({super.key});

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: MapColors.bgDeepest,
      child: CustomPaint(
        painter: IsoMapPainter(buildings: _demoCity()),
        size: Size.infinite,
      ),
    );
  }

  static List<IsoBuilding> _demoCity() {
    final rng = math.Random(42);
    final list = <IsoBuilding>[];
    for (var x = 0; x < 7; x++) {
      for (var y = 0; y < 7; y++) {
        if (x == 3 || y == 3) continue; // Straßenreihen freilassen
        list.add(IsoBuilding(
          tx: x,
          ty: y,
          floors: 2 + rng.nextInt(7),
          seed: x * 31 + y,
        ));
      }
    }
    // Hero-Filiale prominent neben der Kreuzung.
    list.add(const IsoBuilding(tx: 2, ty: 2, floors: 4, seed: 99, hero: true));
    return list;
  }
}
