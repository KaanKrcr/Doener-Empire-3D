import 'package:flutter/material.dart';
import 'dart:math' as math;

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Coffee Inc 2-inspirierte 2.5D Stadtkarte mit CustomPainter.
/// Ersetzt die alte card-basierte Version.
/// API bleibt unverändert (city/locations/shops/selected/onSelect).
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
    return LayoutBuilder(
      builder: (context, constraints) {
        // Calculate optimal canvas size for the map
        const canvasWidth = 1200.0;
        const canvasHeight = 900.0;
        
        return Stack(
          children: [
            // Layer 1: Interactive Map with Pan/Zoom
            InteractiveViewer(
              minScale: 0.5,
              maxScale: 3.0,
              boundaryMargin: const EdgeInsets.all(100),
              child: SizedBox(
                width: canvasWidth,
                height: canvasHeight,
                child: CustomPaint(
                  painter: _CityMapPainter(
                    city: city,
                    locations: locations,
                    shops: shops,
                  ),
                  child: GestureDetector(
                    behavior: HitTestBehavior.translucent,
                    onTapUp: (details) => _handleTap(details.localPosition, context),
                    child: SizedBox(width: canvasWidth, height: canvasHeight),
                  ),
                ),
              ),
            ),
            // Layer 2: Floating Header (nicht gescrollt)
            Positioned(
              top: 0,
              left: 0,
              right: 0,
              child: _FloatingHeader(city: city),
            ),
            // Layer 3: Bottom Bar (nicht gescrollt)
            Positioned(
              bottom: 0,
              left: 0,
              right: 0,
              child: _BottomBar(
                selected: selected,
                onSelect: onSelect,
              ),
            ),
          ],
        );
      },
    );
  }

  void _handleTap(Offset position, BuildContext context) {
    // Grid-Größe berechnen
    const gridSize = 6;
    const cellWidth = 1200.0 / gridSize;
    const cellHeight = 900.0 / gridSize;
    
    for (final location in locations) {
      // Position aus dem Model holen
      final x = location.mapPosition.dx * cellWidth;
      final y = location.mapPosition.dy * cellHeight;
      
      // Prüfen ob Tap im Bereich des Standorts ist
      final rect = Rect.fromCenter(
        center: Offset(x + cellWidth / 2, y + cellHeight / 2),
        width: cellWidth * 0.8,
        height: cellHeight * 0.8,
      );
      
      if (rect.contains(position)) {
        onSelect(location);
        return;
      }
    }
  }
}

// ── City Map Painter (2.5D Isometrisch) ───────────────────────────────
class _CityMapPainter extends CustomPainter {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;

  // Farben aus dem Prompt
  static const waterColor = Color(0xFF0077BE);
  static const streetColor = Color(0xFF999999);
  static const streetLineColor = Color(0xFFFFFFFF);
  static const sidewalkColor = Color(0xFFCCCCCC);
  static const buildingWallLeft = Color(0xFFB8956A);
  static const buildingWallRight = Color(0xFFA08050);
  static const buildingRoof = Color(0xFFC4A882);
  static const windowColor = Color(0xFF87CEEB);
  static const windowLitColor = Color(0xFFFFE4B5);
  static const greenColor = Color(0xFF4CAF50);

  static const gridN = 6;
  static const cellWidth = 1200.0 / gridN;
  static const cellHeight = 900.0 / gridN;

  _CityMapPainter({
    required this.city,
    required this.locations,
    required this.shops,
  });

  @override
  void paint(Canvas canvas, Size size) {
    _drawBackground(canvas, size);
    _drawStreets(canvas, size);
    _drawBuildings(canvas, size);
  }

  void _drawBackground(Canvas canvas, Size size) {
    // Wasser-Hintergrund
    final waterPaint = Paint()..color = waterColor;
    canvas.drawRect(Rect.fromLTWH(0, 0, size.width, size.height), waterPaint);

    // Grünflächen in bestimmten Zellen
    final greenPaint = Paint()..color = greenColor;
    for (int i = 0; i < gridN; i++) {
      for (int j = 0; j < gridN; j++) {
        // Zufällige Grünflächen (deterministisch basierend auf Position)
        if ((i + j * 3) % 7 == 0) {
          final rect = Rect.fromLTWH(
            i * cellWidth + 10,
            j * cellHeight + 10,
            cellWidth - 20,
            cellHeight - 20,
          );
          canvas.drawRRect(
            RRect.fromRectAndRadius(rect, const Radius.circular(8)),
            greenPaint,
          );
        }
      }
    }
  }

  void _drawStreets(Canvas canvas, Size size) {
    final streetPaint = Paint()
      ..color = streetColor
      ..strokeWidth = 24
      ..style = PaintingStyle.stroke;

    final linePaint = Paint()
      ..color = streetLineColor
      ..strokeWidth = 2
      ..style = PaintingStyle.stroke;

    // Vertikale Straßen
    for (int i = 1; i < gridN; i++) {
      final x = i * cellWidth;
      canvas.drawLine(
        Offset(x, 0),
        Offset(x, size.height),
        streetPaint,
      );
      // Gestrichelte Mittellinie
      _drawDashedLine(
        canvas,
        Offset(x, 0),
        Offset(x, size.height),
        linePaint,
        dashLength: 20,
        gapLength: 15,
      );
    }

    // Horizontale Straßen
    for (int j = 1; j < gridN; j++) {
      final y = j * cellHeight;
      canvas.drawLine(
        Offset(0, y),
        Offset(size.width, y),
        streetPaint,
      );
      // Gestrichelte Mittellinie
      _drawDashedLine(
        canvas,
        Offset(0, y),
        Offset(size.width, y),
        linePaint,
        dashLength: 20,
        gapLength: 15,
      );
    }

    // Gehwege an Kreuzungen
    final sidewalkPaint = Paint()..color = sidewalkColor;
    for (int i = 0; i < gridN; i++) {
      for (int j = 0; j < gridN; j++) {
        final rect = Rect.fromLTWH(
          i * cellWidth + 4,
          j * cellHeight + 4,
          cellWidth - 8,
          cellHeight - 8,
        );
        canvas.drawRRect(
          RRect.fromRectAndRadius(rect, const Radius.circular(4)),
          sidewalkPaint..style = PaintingStyle.stroke,
        );
      }
    }
  }

  void _drawDashedLine(Canvas canvas, Offset start, Offset end, Paint paint,
      {double dashLength = 10, double gapLength = 10}) {
    final dx = end.dx - start.dx;
    final dy = end.dy - start.dy;
    final length = math.sqrt(dx * dx + dy * dy);
    final unitDx = dx / length;
    final unitDy = dy / length;

    double distance = 0;
    while (distance < length) {
      final startPoint = Offset(
        start.dx + unitDx * distance,
        start.dy + unitDy * distance,
      );
      final endDistance = math.min(distance + dashLength, length);
      final endPoint = Offset(
        start.dx + unitDx * endDistance,
        start.dy + unitDy * endDistance,
      );
      canvas.drawLine(startPoint, endPoint, paint);
      distance += dashLength + gapLength;
    }
  }

  void _drawBuildings(Canvas canvas, Size size) {
    for (final location in locations) {
      final gridX = location.mapPosition.dx.toInt();
      final gridY = location.mapPosition.dy.toInt();

      // Prüfen ob Position gültig ist
      if (gridX < 0 || gridX >= gridN || gridY < 0 || gridY >= gridN) continue;

      // Isometrische Position berechnen
      final baseX = gridX * cellWidth + cellWidth / 2;
      final baseY = gridY * cellHeight + cellHeight / 2;

      // Building-Dimensionen (isometrisch)
      final bWidth = cellWidth * 0.7;
      final bHeight = cellHeight * 0.7;
      final bDepth = 40.0;
      final floors = _getFloorsForLocation(location);

      // Hat eigene Filiale?
      final hasOwned = shops.any(
          (s) => s.cityId == city.id && s.locationName == location.template.name);

      // 2.5D Isometrisches Gebäude zeichnen
      _drawIsometricBuilding(
        canvas,
        Offset(baseX, baseY - 20),
        bWidth,
        bHeight,
        bDepth,
        floors,
        hasOwned,
      );
    }
  }

  int _getFloorsForLocation(CityMapLocation location) {
    // Stockwerke basierend auf Standort-Typ
    switch (location.template.personality) {
      case LocationPersonality.business:
        return 5 + (location.mapPosition.dx.toInt() % 3);
      case LocationPersonality.university:
        return 4 + (location.mapPosition.dy.toInt() % 2);
      case LocationPersonality.touristic:
        return 6 + (location.mapPosition.dx.toInt() % 2);
      case LocationPersonality.residential:
        return 3 + (location.mapPosition.dy.toInt() % 4);
      case LocationPersonality.industrial:
        return 2;
      case LocationPersonality.suburban:
        return 2 + (location.mapPosition.dx.toInt() % 2);
    }
  }

  void _drawIsometricBuilding(
    Canvas canvas,
    Offset base,
    double width,
    double height,
    double depth,
    int floors,
    bool hasOwned,
  ) {
    final floorHeight = 12.0;
    final buildingHeight = floors * floorHeight;

    // Linke Wand (dunkler)
    final wallLeftPath = Path()
      ..moveTo(base.dx - width / 2, base.dy)
      ..lineTo(base.dx - width / 2, base.dy - buildingHeight)
      ..lineTo(base.dx, base.dy - buildingHeight - depth)
      ..lineTo(base.dx, base.dy - depth)
      ..close();
    canvas.drawPath(
      wallLeftPath,
      Paint()..color = buildingWallLeft,
    );

    // Rechte Wand (dunkler)
    final wallRightPath = Path()
      ..moveTo(base.dx, base.dy - depth)
      ..lineTo(base.dx, base.dy - buildingHeight - depth)
      ..lineTo(base.dx + width / 2, base.dy - buildingHeight)
      ..lineTo(base.dx + width / 2, base.dy)
      ..close();
    canvas.drawPath(
      wallRightPath,
      Paint()..color = buildingWallRight,
    );

    // Dach
    final roofPath = Path()
      ..moveTo(base.dx - width / 2, base.dy - buildingHeight)
      ..lineTo(base.dx, base.dy - buildingHeight - depth)
      ..lineTo(base.dx + width / 2, base.dy - buildingHeight)
      ..lineTo(base.dx, base.dy - buildingHeight + depth)
      ..close();
    canvas.drawPath(
      roofPath,
      Paint()..color = buildingRoof,
    );

    // Fenster zeichnen
    for (int floor = 0; floor < floors; floor++) {
      final floorY = base.dy - (floor + 1) * floorHeight - 10;
      // Linke Fenster
      for (int w = 0; w < 2; w++) {
        final windowX = base.dx - width / 2 + 15 + w * 25;
        canvas.drawRect(
          Rect.fromCenter(
            center: Offset(windowX + 5, floorY),
            width: 12,
            height: 10,
          ),
          Paint()..color = windowColor,
        );
      }
      // Rechte Fenster
      for (int w = 0; w < 2; w++) {
        final windowX = base.dx + 10 + w * 25;
        canvas.drawRect(
          Rect.fromCenter(
            center: Offset(windowX, floorY),
            width: 12,
            height: 10,
          ),
          Paint()..color = windowColor,
        );
      }
    }

    // Eingang
    canvas.drawRect(
      Rect.fromCenter(
        center: Offset(base.dx, base.dy - 8),
        width: 20,
        height: 16,
      ),
      Paint()..color = const Color(0xFF5D4037),
    );

    // Eigene Filiale: Goldener Rand
    if (hasOwned) {
      final borderPaint = Paint()
        ..color = const Color(0xFFD46816)
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3;
      canvas.drawPath(wallLeftPath, borderPaint);
      canvas.drawPath(wallRightPath, borderPaint);
      canvas.drawPath(roofPath, borderPaint);
    }
  }

  @override
  bool shouldRepaint(covariant _CityMapPainter oldDelegate) {
    return oldDelegate.city != city ||
        oldDelegate.locations != locations ||
        oldDelegate.shops != shops;
  }
}

// ── Floating Header ─────────────────────────────────────────────────
class _FloatingHeader extends StatelessWidget {
  final CityData city;

  const _FloatingHeader({required this.city});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xFF1A1A1A).withAlpha(204),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          Text(city.emoji, style: const TextStyle(fontSize: 24)),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  city.name,
                  style: AppText.display(
                    size: 18,
                    color: AppColors.primary,
                  ),
                ),
                Text(
                  city.tier.label,
                  style: const TextStyle(
                    color: AppColors.textSecondary,
                    fontSize: 11,
                  ),
                ),
              ],
            ),
          ),
          // Cash Anzeige (würde normalerweise aus GameState kommen)
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: AppColors.success.withAlpha(40),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text('💰', style: TextStyle(fontSize: 14)),
                const SizedBox(width: 6),
                Text(
                  '€${city.rentBase * 10}',
                  style: const TextStyle(
                    color: AppColors.success,
                    fontSize: 14,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ── Bottom Bar ─────────────────────────────────────────────────────────
class _BottomBar extends StatelessWidget {
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;

  const _BottomBar({
    required this.selected,
    required this.onSelect,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xFF1A1A1A).withAlpha(204),
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          if (selected != null) ...[
            Text(selected!.icon, style: const TextStyle(fontSize: 24)),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    selected!.template.name,
                    style: AppText.display(
                      size: 16,
                      color: AppColors.primary,
                    ),
                  ),
                  Text(
                    '★ ${selected!.attractivenessScore(selected!.template.personality == LocationPersonality.business ? CityData(id: '', name: '', tier: CityTier.klein, footTrafficBase: 1000, rentBase: 500) : CityData(id: '', name: '', tier: CityTier.klein, footTrafficBase: 1000, rentBase: 500)).round()}',
                    style: const TextStyle(
                      color: AppColors.gold,
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
          ] else ...[
            const Text(
              '📍 Standort wählen',
              style: TextStyle(
                color: AppColors.textSecondary,
                fontSize: 14,
              ),
            ),
            const Spacer(),
          ],
          // Menü Button
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: AppColors.bgCard,
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(
              Icons.menu,
              color: AppColors.textSecondary,
              size: 20,
            ),
          ),
        ],
      ),
    );
  }
}
