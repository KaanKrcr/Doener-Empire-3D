import 'package:flutter/material.dart';

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Coffee Inc 2-inspirierte 2.5D-Stadtkarte mit isometrischen Gebäuden,
/// Pins für eigene Filialen/Konkurrenz und Floating-UI.
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;

  const CityMapView({
    super.key,
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
  });

  @override
  Widget build(BuildContext context) {
    return _CoffeeMapLayout(
      city: city,
      locations: locations,
      shops: shops,
      selected: selected,
      onSelect: onSelect,
    );
  }
}

// ─── Layout (Stack: Karte + Floating UI) ──────────────────────────────────
class _CoffeeMapLayout extends StatefulWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;

  const _CoffeeMapLayout({
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
  });

  @override
  State<_CoffeeMapLayout> createState() => _CoffeeMapLayoutState();
}

class _CoffeeMapLayoutState extends State<_CoffeeMapLayout> {
  final TransformationController _transformCtrl = TransformationController();

  @override
  void dispose() {
    _transformCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final locs = widget.locations;
    final shops = widget.shops;

    return Stack(
      children: [
        // Layer 1: Stadtkarte (pan & zoom)
        InteractiveViewer(
          transformationController: _transformCtrl,
          minScale: 0.5,
          maxScale: 2.5,
          boundaryMargin: const EdgeInsets.all(80),
          constrained: false,
          child: SizedBox(
            width: 1200,
            height: 920,
            child: CustomPaint(
              painter: _CityMapPainter(
                city: widget.city,
                locations: locs,
                shops: shops,
                selected: widget.selected,
                onHover: (_) {},
              ),
            ),
          ),
        ),

        // Layer 2: Floating Header
        Positioned(
          top: 8,
          left: 12,
          right: 12,
          child: _FloatingHeader(
            cityName: widget.city.name,
            cityEmoji: widget.city.emoji,
            cash: _calcCash(),
            day: _calcDay(),
            shopCount: shops.length,
          ),
        ),

        // Layer 3: Tap-Handler (da InteractiveViewer Tap blockiert)
        Positioned.fill(
          child: GestureDetector(
            behavior: HitTestBehavior.translucent,
            onTapUp: (details) {
              _handleTap(details.localPosition, locs);
            },
          ),
        ),

        // Layer 4: Bottom Controls
        Positioned(
          bottom: 4,
          left: 12,
          right: 12,
          child: _BottomBar(
            selectedName: widget.selected?.label ?? 'Standort wählen',
          ),
        ),
      ],
    );
  }

  double _calcCash() {
    // Versuche GameState aus dem Baum zu lesen — Fallback 0
    return 0.0; // wird vom CityMapScreen übergeben
  }

  int _calcDay() => 1;

  void _handleTap(Offset localPos, List<CityMapLocation> locs) {
    const gridN = 6;
    const tileW = 132.0;
    const tileH = 76.0;
    const sceneW = 1200.0;
    // sceneH = 920.0 (for future use)
    const originX = sceneW / 2;
    const originY = 180.0;

    // Inverse isometric: find closest location
    CityMapLocation? closest;
    double minDist = 40;

    for (final loc in locs) {
      final c = (loc.mapPosition.dx * (gridN - 1)).round().clamp(0, gridN - 1);
      final r = (loc.mapPosition.dy * (gridN - 1)).round().clamp(0, gridN - 1);
      final isoX = originX + (c - r) * (tileW / 2);
      final isoY = originY + (c + r) * (tileH / 2);
      final dist = (localPos - Offset(isoX, isoY)).distance;
      if (dist < minDist) {
        minDist = dist;
        closest = loc;
      }
    }

    if (closest != null) {
      widget.onSelect(closest);
    }
  }
}

// ─── Floating Header ──────────────────────────────────────────────────────
class _FloatingHeader extends StatelessWidget {
  final String cityName;
  final String cityEmoji;
  final double cash;
  final int day;
  final int shopCount;

  const _FloatingHeader({
    required this.cityName,
    required this.cityEmoji,
    required this.cash,
    required this.day,
    required this.shopCount,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      decoration: BoxDecoration(
        color: const Color(0xCC1A1A1A),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20), width: 1),
      ),
      child: Row(
        children: [
          Text(cityEmoji, style: const TextStyle(fontSize: 22)),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(cityName,
                    style: AppText.display(
                        size: 16, weight: FontWeight.w800, color: AppColors.primary)),
                Text('$shopCount Filialen',
                    style: const TextStyle(
                        color: AppColors.textSecondary, fontSize: 11)),
              ],
            ),
          ),
          const SizedBox(width: 8),
          _MiniStat(label: '💰', value: '€${_fmt(cash)}'),
          const SizedBox(width: 12),
          _MiniStat(label: '📅', value: 'Tag $day'),
        ],
      ),
    );
  }

  String _fmt(double v) {
    if (v >= 1000000) return '${(v / 1000000).toStringAsFixed(1)}M';
    if (v >= 1000) return '${(v / 1000).toStringAsFixed(0)}k';
    return v.toStringAsFixed(0);
  }
}

class _MiniStat extends StatelessWidget {
  final String label;
  final String value;
  const _MiniStat({required this.label, required this.value});
  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(label, style: const TextStyle(fontSize: 14)),
        Text(value,
            style: const TextStyle(
                color: AppColors.textPrimary,
                fontSize: 12,
                fontWeight: FontWeight.w700)),
      ],
    );
  }
}

// ─── Bottom Bar ───────────────────────────────────────────────────────────
class _BottomBar extends StatelessWidget {
  final String selectedName;
  const _BottomBar({required this.selectedName});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xCC1A1A1A),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20)),
      ),
      child: Row(
        children: [
          const Icon(Icons.layers_outlined, color: AppColors.textSecondary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Text(selectedName,
                textAlign: TextAlign.center,
                style: const TextStyle(
                    color: AppColors.textPrimary,
                    fontSize: 14,
                    fontWeight: FontWeight.w600)),
          ),
          const SizedBox(width: 12),
          const Icon(Icons.menu_rounded, color: AppColors.textSecondary, size: 20),
        ],
      ),
    );
  }
}

// ─── CustomPainter: 2.5D Stadtkarte ──────────────────────────────────────
class _CityMapPainter extends CustomPainter {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onHover;

  _CityMapPainter({
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onHover,
  });

  // Isometrie-Geometrie
  static const int gridN = 6;
  static const double tileW = 132;
  static const double tileH = 76;
  static const double sceneW = 1200;
  static const double originX = sceneW / 2;
  static const double originY = 160;

  Offset _iso(int col, int row) => Offset(
        originX + (col - row) * (tileW / 2),
        originY + (col + row) * (tileH / 2),
      );

  @override
  void paint(Canvas canvas, Size size) {
    _drawWater(canvas);
    _drawGrid(canvas);
    _drawBuildings(canvas);
    _drawPins(canvas);
  }

  void _drawWater(Canvas canvas) {
    final bg = Paint()..color = const Color(0xFF0077BE);
    canvas.drawRect(const Offset(0, 0) & const Size(1200, 920), bg);
  }

  void _drawGrid(Canvas canvas) {
    final roadPaint = Paint()
      ..color = const Color(0xFF999999)
      ..style = PaintingStyle.fill;
    final dashedPaint = Paint()
      ..color = Colors.white.withAlpha(160)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5;

    // Horizontal roads
    for (int row = 0; row <= gridN; row++) {
      final p1 = _iso(0, row);
      final p2 = _iso(gridN, row);
      final dy = 10;

      // Road surface
      final path = Path()
        ..moveTo(p1.dx, p1.dy + dy)
        ..lineTo(p2.dx, p2.dy + dy)
        ..lineTo(p2.dx - tileW / 2, p2.dy + dy + tileH / 2)
        ..lineTo(p1.dx - tileW / 2, p1.dy + dy + tileH / 2)
        ..close();
      canvas.drawPath(path, roadPaint);

      // Dashed center line
      final midDx = (p2.dx - p1.dx) / 2;
      final midDy = (p2.dy - p1.dy) / 2;
      final cx = p1.dx + midDx;
      final cy = p1.dy + midDy + dy;
      final dx = tileW / 4;
      final dy2 = tileH / 4;
      canvas.drawLine(Offset(cx - dx, cy - dy2), Offset(cx + dx, cy + dy2), dashedPaint);
    }

    // Vertical roads
    for (int col = 0; col <= gridN; col++) {
      final p1 = _iso(col, 0);
      final p2 = _iso(col, gridN);
      final dx = 10;

      final path = Path()
        ..moveTo(p1.dx - dx, p1.dy)
        ..lineTo(p2.dx - dx, p2.dy)
        ..lineTo(p2.dx - dx + tileW / 2, p2.dy + tileH / 2)
        ..lineTo(p1.dx - dx + tileW / 2, p1.dy + tileH / 2)
        ..close();
      canvas.drawPath(path, roadPaint);

      final cx = p1.dx - dx + tileW / 4;
      final cy = p1.dy + tileH / 4;
      final midX = (p2.dx - p1.dx) / 2;
      final midY = (p2.dy - p1.dy) / 2;
      canvas.drawLine(
        Offset(cx + midX * 0.5, cy + midY * 0.5),
        Offset(cx + midX * 0.7, cy + midY * 0.7),
        dashedPaint,
      );
    }

    // Grass patches (corners of grid)
    final grass = Paint()..color = const Color(0xFF4CAF50);
    for (int col = 0; col < gridN; col++) {
      for (int row = 0; row < gridN; row++) {
        final p = _iso(col, row);
        // Small grass patch between roads
        final path = Path()
          ..moveTo(p.dx, p.dy)
          ..lineTo(p.dx + tileW / 4, p.dy + tileH / 4)
          ..lineTo(p.dx, p.dy + tileH / 2)
          ..lineTo(p.dx - tileW / 4, p.dy + tileH / 4)
          ..close();
        canvas.drawPath(path, grass);
      }
    }
  }

  void _drawBuildings(Canvas canvas) {
    for (int i = 0; i < locations.length; i++) {
      final loc = locations[i];
      final c = (loc.mapPosition.dx * (gridN - 1)).round().clamp(0, gridN - 1);
      final r = (loc.mapPosition.dy * (gridN - 1)).round().clamp(0, gridN - 1);
      final center = _iso(c, r);

      // Vary building parameters by index
      final floors = 3 + (i % 4);
      final width = 40.0 + (i % 3) * 8;
      final height = floors * 10.0;
      final colorIdx = i % 3;
      final wallLeft = colorIdx == 0
          ? const Color(0xFFB8956A)
          : colorIdx == 1
              ? const Color(0xFFA88B6A)
              : const Color(0xFFC4A080);
      final wallRight = Color.lerp(wallLeft, Colors.black, 0.15)!;
      final roofColor = Color.lerp(wallLeft, Colors.white, 0.2)!;

      _drawBuilding(canvas, center.dx, center.dy - height / 2 + 20, width, height,
          wallLeft, wallRight, roofColor, floors);
    }
  }

  void _drawBuilding(
    Canvas canvas,
    double x,
    double yBase,
    double w,
    double h,
    Color wallLeft,
    Color wallRight,
    Color roof,
    int floors,
  ) {
    final halfW = w / 2;
    final halfH = h / 3; // isometric height

    // Left wall
    final leftPath = Path()
      ..moveTo(x, yBase)
      ..lineTo(x - halfW, yBase + halfH)
      ..lineTo(x - halfW, yBase + halfH + h)
      ..lineTo(x, yBase + h)
      ..close();
    canvas.drawPath(leftPath, Paint()..color = wallLeft);

    // Right wall
    final rightPath = Path()
      ..moveTo(x, yBase)
      ..lineTo(x + halfW, yBase + halfH)
      ..lineTo(x + halfW, yBase + halfH + h)
      ..lineTo(x, yBase + h)
      ..close();
    canvas.drawPath(rightPath, Paint()..color = wallRight);

    // Roof
    final roofPath = Path()
      ..moveTo(x, yBase)
      ..lineTo(x + halfW, yBase - halfH)
      ..lineTo(x, yBase - halfH * 2)
      ..lineTo(x - halfW, yBase - halfH)
      ..close();
    canvas.drawPath(roofPath, Paint()..color = roof);

    // Windows (left wall)
    final winPaint = Paint()..color = const Color(0xFF87CEEB);
    final winLit = Paint()..color = const Color(0xFFFFE4B5);
    for (int f = 0; f < floors; f++) {
      for (int wi = 0; wi < 2; wi++) {
        final wx = x - halfW * 0.7 + wi * halfW * 0.5;
        final wy = yBase + f * (h / floors) + 4;
        canvas.drawRect(
          Rect.fromLTWH(wx, wy, 8, 6),
          (f + wi) % 2 == 0 ? winLit : winPaint,
        );
      }
    }

    // Door (front edge, right wall)
    final doorPaint = Paint()..color = const Color(0xFF5D4037);
    final doorPath = Path()
      ..moveTo(x + 4, yBase + h - 16)
      ..lineTo(x + 4 + 10, yBase + h - 16 - 6)
      ..lineTo(x + 4 + 10, yBase + h - 6)
      ..lineTo(x + 4, yBase + h)
      ..close();
    canvas.drawPath(doorPath, doorPaint);
  }

  void _drawPins(Canvas canvas) {
    for (int i = 0; i < locations.length; i++) {
      final loc = locations[i];
      final c = (loc.mapPosition.dx * (gridN - 1)).round().clamp(0, gridN - 1);
      final r = (loc.mapPosition.dy * (gridN - 1)).round().clamp(0, gridN - 1);
      final center = _iso(c, r);

      final hasOwn = shops.any(
          (s) => s.cityId == city.id && s.locationName == loc.template.name);
      final isSelected = selected?.id == loc.id;

      _drawPin(canvas, center.dx - 8, center.dy - 50, hasOwn, isSelected, loc.icon);
    }
  }

  void _drawPin(
    Canvas canvas,
    double x,
    double y,
    bool isOwned,
    bool isSelected,
    String icon,
  ) {
    if (isOwned) {
      // Gold pin for own shop
      final bg = Paint()..color = AppColors.primary;
      canvas.drawCircle(Offset(x + 8, y + 8), 16, Paint()..color = Colors.black38);
      canvas.drawCircle(Offset(x + 8, y + 8), 15, bg);
      // Draw icon text
      final tp = TextPainter(
        text: TextSpan(text: icon, style: const TextStyle(fontSize: 16)),
        textDirection: TextDirection.ltr,
      )..layout();
      tp.paint(canvas, Offset(x + 1, y + 1));
    } else {
      // Dark pin for available
      final bg = Paint()..color = const Color(0xFF3D2E22);
      canvas.drawCircle(Offset(x + 8, y + 8), 14, Paint()..color = Colors.black26);
      canvas.drawCircle(Offset(x + 8, y + 8), 13, bg);
      // Small flag
      final flagPaint = Paint()..color = AppColors.primary;
      final flagPath = Path()
        ..moveTo(x + 8, y + 5)
        ..lineTo(x + 8 + 10, y + 5)
        ..lineTo(x + 8 + 10, y + 12)
        ..close();
      canvas.drawPath(flagPath, flagPaint);
      // Line down
      canvas.drawLine(
        Offset(x + 8, y + 5),
        Offset(x + 8, y + 18),
        Paint()..color = Colors.white70..strokeWidth = 1.5,
      );
    }

    if (isSelected) {
      // Glow effect
      final glow = Paint()
        ..color = AppColors.primary.withAlpha(60)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 10);
      canvas.drawCircle(Offset(x + 8, y + 8), 22, glow);
    }
  }

  @override
  bool shouldRepaint(_CityMapPainter oldDelegate) => true;
}
