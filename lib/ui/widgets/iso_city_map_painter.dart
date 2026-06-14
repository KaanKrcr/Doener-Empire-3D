import 'dart:math' as math;

import 'package:flutter/material.dart';

/// Datenmodell: Ein isometrisches Gebäude auf der Karte.
class IsoBuilding {
  final int tx;        // Grid-Spalte (0-5)
  final int ty;        // Grid-Zeile (0-5)
  final int floors;    // Stockwerke (3-7)
  final int seed;      // Deterministischer Zufall für Fenster
  final bool hero;     // Hervorgehobenes Gebäude (Gold-Glow)

  const IsoBuilding({
    required this.tx,
    required this.ty,
    this.floors = 4,
    this.seed = 0,
    this.hero = false,
  });
}

/// Professioneller Iso-CityMap-Painter nach MAP_DESIGN_SPEC.md.
class IsoMapPainter extends CustomPainter {
  final List<IsoBuilding> buildings;

  // Iso-Geometrie
  static const double _tileW = 140;
  static const double _tileH = 80;
  static const double _sceneW = 1400;
  static const double _sceneH = 1060;
  static const double _originX = _sceneW / 2;
  static const double _originY = 180;

  IsoMapPainter({required this.buildings});

  Offset _iso(int col, int row) => Offset(
        _originX + (col - row) * (_tileW / 2),
        _originY + (col + row) * (_tileH / 2),
      );

  @override
  void paint(Canvas canvas, Size size) {
    _drawBackground(canvas);
    _drawGrid(canvas);
    _drawBuildings(canvas);
  }

  // ── 1. Hintergrund ──────────────────────────────────────────────────────
  void _drawBackground(Canvas canvas) {
    final bgRect = Offset.zero & const Size(_sceneW, _sceneH);
    final gradient = Paint()
      ..shader = const LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [Color(0xFF1A1A2E), Color(0xFF16213E)],
      ).createShader(bgRect);
    canvas.drawRect(bgRect, gradient);

    // Horizon fade
    final horizon = Paint()
      ..shader = LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [Colors.transparent, const Color(0xFF2D2D44)],
      ).createShader(Rect.fromLTWH(0, 300, _sceneW, 200));
    canvas.drawRect(const Offset(0, 300) & const Size(_sceneW, 200), horizon);
  }

  // ── 2. Iso-Straßen (6×6 Grid) ───────────────────────────────────────────
  void _drawGrid(Canvas canvas) {
    final road = Paint()..color = const Color(0xFF555566);
    final curb = Paint()..color = const Color(0xFF666677);
    final dashed = Paint()
      ..color = const Color(0xFF888899)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;

    // Horizontal roads (row → row+1 connections)
    for (int r = 0; r <= 6; r++) {
      final p0 = _iso(0, r);
      final p6 = _iso(6, r);
      final dy = 10.0;

      // Road body
      final body = Path()
        ..moveTo(p0.dx, p0.dy + dy)
        ..lineTo(p6.dx, p6.dy + dy)
        ..lineTo(p6.dx - _tileW / 2, p6.dy + dy + _tileH / 2)
        ..lineTo(p0.dx - _tileW / 2, p0.dy + dy + _tileH / 2)
        ..close();
      canvas.drawPath(body, road);

      // Curb left
      final curbL = Path()
        ..moveTo(p0.dx, p0.dy + dy)
        ..lineTo(p6.dx, p6.dy + dy)
        ..lineTo(p6.dx - 4, p6.dy + dy - 2)
        ..lineTo(p0.dx - 4, p0.dy + dy - 2)
        ..close();
      canvas.drawPath(curbL, curb);

      // Dashed center
      final mid = Offset(
        (p0.dx + p6.dx) / 2 - _tileW / 4,
        (p0.dy + p6.dy) / 2 + dy + _tileH / 4,
      );
      canvas.drawLine(
        Offset(mid.dx - 10, mid.dy - 6),
        Offset(mid.dx + 10, mid.dy + 6),
        dashed,
      );
    }

    // Vertical roads
    for (int c = 0; c <= 6; c++) {
      final p0 = _iso(c, 0);
      final p6 = _iso(c, 6);
      final dx = 10.0;

      final body = Path()
        ..moveTo(p0.dx - dx, p0.dy)
        ..lineTo(p6.dx - dx, p6.dy)
        ..lineTo(p6.dx - dx + _tileW / 2, p6.dy + _tileH / 2)
        ..lineTo(p0.dx - dx + _tileW / 2, p0.dy + _tileH / 2)
        ..close();
      canvas.drawPath(body, road);

      final mid = Offset(
        (p0.dx + p6.dx) / 2 - dx + _tileW / 4,
        (p0.dy + p6.dy) / 2 + _tileH / 4,
      );
      canvas.drawLine(
        Offset(mid.dx - 6, mid.dy - 10),
        Offset(mid.dx + 6, mid.dy + 10),
        dashed,
      );
    }

    // Grass patches (Tile-Mitten)
    final grass = Paint()..color = const Color(0xFF2D5A27);
    for (int c = 0; c < 6; c++) {
      for (int r = 0; r < 6; r++) {
        final p = _iso(c, r);
        final g = Path()
          ..moveTo(p.dx, p.dy + 2)
          ..lineTo(p.dx + _tileW / 4 - 4, p.dy + _tileH / 4)
          ..lineTo(p.dx, p.dy + _tileH / 2 - 2)
          ..lineTo(p.dx - _tileW / 4 + 4, p.dy + _tileH / 4)
          ..close();
        canvas.drawPath(g, grass);
      }
    }
  }

  // ── 3. Gebäude ──────────────────────────────────────────────────────────
  void _drawBuildings(Canvas canvas) {
    // Depth sort: draw back-to-front (row + col)
    final sorted = List<IsoBuilding>.from(buildings)
      ..sort((a, b) => (a.tx + a.ty).compareTo(b.tx + b.ty));

    for (final b in sorted) {
      final center = _iso(b.tx, b.ty);
      _drawSingleBuilding(canvas, center.dx, center.dy, b);
    }
  }

  void _drawSingleBuilding(Canvas canvas, double x, double y, IsoBuilding b) {
    final rng = math.Random(b.seed);
    final w = 48.0;
    final floors = b.floors;
    final h = floors * 12.0;

    // Shadow
    if (b.hero) {
      final glow = Paint()
        ..shader = RadialGradient(
          colors: [
            const Color(0xFFD46816).withAlpha(50),
            Colors.transparent,
          ],
        ).createShader(Rect.fromCircle(center: Offset(x, y + h + 10), radius: 60));
      canvas.drawCircle(Offset(x, y + h + 10), 60, glow);
    } else {
      final shadow = Paint()
        ..color = Colors.black.withAlpha(40)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 4);
      final sPath = Path()
        ..moveTo(x + 8, y + h + 4)
        ..lineTo(x + w / 2 + 18, y + h + 4 + w / 4 + 2)
        ..lineTo(x + w / 2 + 10, y + h + 4 + w / 4 + 2 + h - 10)
        ..lineTo(x + 8, y + h + h - 8)
        ..close();
      canvas.drawPath(sPath, shadow);
    }

    // Neon outline for hero
    if (b.hero) {
      final neon = Paint()
        ..color = const Color(0xFFD46816)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);
      final nPath = Path()
        ..moveTo(x, y)
        ..lineTo(x + w / 2, y - w / 4)
        ..lineTo(x + w / 2, y - w / 4 + h)
        ..lineTo(x, y + h)
        ..close();
      canvas.drawPath(nPath, neon);
    }

    // Walls
    final halfW = w / 2;
    final halfH = w / 4; // isometric depth

    // Left wall
    final leftShade = Paint()
      ..color = const Color(0xFFB8956A);
    final leftPath = Path()
      ..moveTo(x, y)
      ..lineTo(x - halfW, y + halfH)
      ..lineTo(x - halfW, y + halfH + h)
      ..lineTo(x, y + h)
      ..close();
    canvas.drawPath(leftPath, leftShade);

    // Right wall (darker)
    final rightShade = Paint()
      ..color = const Color(0xFFA08050);
    final rightPath = Path()
      ..moveTo(x, y)
      ..lineTo(x + halfW, y + halfH)
      ..lineTo(x + halfW, y + halfH + h)
      ..lineTo(x, y + h)
      ..close();
    canvas.drawPath(rightPath, rightShade);

    // Roof
    final roof = Paint()..color = const Color(0xFFD4B892);
    final roofPath = Path()
      ..moveTo(x, y)
      ..lineTo(x + halfW, y - halfH)
      ..lineTo(x, y - halfH * 2)
      ..lineTo(x - halfW, y - halfH)
      ..close();
    canvas.drawPath(roofPath, roof);

    // Roof overhang edge
    final roofEdge = Paint()
      ..color = const Color(0xFFC4A882)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5;
    canvas.drawPath(roofPath, roofEdge);

    // Windows (left wall)
    const warm = Color(0xFFFFE4B5);
    const cool = Color(0xFF87CEEB);
    const darkWin = Color(0xFF3D3D5C);
    final winSize = const Size(8, 6);

    for (int f = 0; f < floors; f++) {
      for (int wi = 0; wi < 2; wi++) {
        final winX = x - halfW * 0.75 + wi * halfW * 0.5;
        final winY = y + f * (h / floors) + 6;
        final winColor = [warm, cool, darkWin][rng.nextInt(3)];
        canvas.drawRect(
          Rect.fromLTWH(winX, winY, winSize.width, winSize.height),
          Paint()..color = winColor,
        );
      }
    }

    // Door (right wall, ground floor)
    final door = Paint()..color = const Color(0xFF5D4037);
    final doorPath = Path()
      ..moveTo(x + 6, y + h - 14)
      ..lineTo(x + 6 + 12, y + h - 14 - 6)
      ..lineTo(x + 6 + 12, y + h - 6)
      ..lineTo(x + 6, y + h - 2)
      ..close();
    canvas.drawPath(doorPath, door);
  }

  @override
  bool shouldRepaint(covariant IsoMapPainter oldDelegate) => true;
}

/// Test-Demo (einfach per Scaffold(body: IsoCityMapDemo()) nutzbar).
class IsoCityMapDemo extends StatelessWidget {
  const IsoCityMapDemo({super.key});

  @override
  Widget build(BuildContext context) {
    final buildings = [
      const IsoBuilding(tx: 1, ty: 1, floors: 4, seed: 42, hero: true),
      const IsoBuilding(tx: 4, ty: 1, floors: 5, seed: 17),
      const IsoBuilding(tx: 1, ty: 4, floors: 6, seed: 33),
      const IsoBuilding(tx: 4, ty: 4, floors: 3, seed: 55),
      const IsoBuilding(tx: 2, ty: 2, floors: 5, seed: 91),
      const IsoBuilding(tx: 3, ty: 2, floors: 7, seed: 12),
      const IsoBuilding(tx: 2, ty: 3, floors: 6, seed: 78),
      const IsoBuilding(tx: 3, ty: 3, floors: 3, seed: 44),
      const IsoBuilding(tx: 2, ty: 1, floors: 7, seed: 99, hero: true),
      const IsoBuilding(tx: 3, ty: 1, floors: 6, seed: 21),
      const IsoBuilding(tx: 1, ty: 2, floors: 5, seed: 66),
      const IsoBuilding(tx: 4, ty: 3, floors: 3, seed: 88),
    ];

    return Scaffold(
      backgroundColor: const Color(0xFF1A1A2E),
      body: Center(
        child: InteractiveViewer(
          minScale: 0.5,
          maxScale: 2.5,
          boundaryMargin: const EdgeInsets.all(80),
          constrained: false,
          child: SizedBox(
            width: 1400,
            height: 1060,
            child: CustomPaint(
              painter: IsoMapPainter(buildings: buildings),
            ),
          ),
        ),
      ),
    );
  }
}
