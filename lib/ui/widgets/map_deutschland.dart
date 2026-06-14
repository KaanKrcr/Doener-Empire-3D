import 'package:flutter/material.dart';

import '../../core/constants.dart';
import '../../models/city_model.dart';
import 'building_styles.dart';

/// Ungefähre, stilisierte Position jeder Stadt auf dem Deutschland-Umriss
/// (normalisiert 0..1; x = West→Ost, y = Nord→Süd).
const Map<String, Offset> kCityMapPositions = {
  'hamburg': Offset(0.44, 0.17),
  'berlin': Offset(0.66, 0.26),
  'muenster': Offset(0.24, 0.35),
  'braunschweig': Offset(0.52, 0.31),
  'goettingen': Offset(0.43, 0.39),
  'duesseldorf': Offset(0.19, 0.42),
  'koeln': Offset(0.20, 0.46),
  'frankfurt': Offset(0.33, 0.53),
  'fulda': Offset(0.40, 0.49),
  'bayreuth': Offset(0.58, 0.56),
  'stuttgart': Offset(0.33, 0.72),
  'augsburg': Offset(0.52, 0.80),
  'muenchen': Offset(0.56, 0.84),
};

/// Ebene 1: Deutschlandkarte mit Städte-Auswahl.
class MapDeutschland extends StatelessWidget {
  final Set<String> unlockedCityIds;
  final Set<String> cityIdsWithShops;
  final void Function(String cityId) onSelectCity;

  const MapDeutschland({
    super.key,
    required this.unlockedCityIds,
    required this.cityIdsWithShops,
    required this.onSelectCity,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      color: MapPalette.bgBase,
      padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
      child: Column(
        children: [
          const _Hint(),
          const SizedBox(height: 8),
          Expanded(
            child: LayoutBuilder(
              builder: (context, c) {
                const aspect = 0.80; // Deutschland ist höher als breit
                var mapW = c.maxWidth;
                var mapH = mapW / aspect;
                if (mapH > c.maxHeight) {
                  mapH = c.maxHeight;
                  mapW = mapH * aspect;
                }
                final ox = (c.maxWidth - mapW) / 2;
                final oy = (c.maxHeight - mapH) / 2;

                return Stack(
                  children: [
                    Positioned(
                      left: ox,
                      top: oy,
                      width: mapW,
                      height: mapH,
                      child: CustomPaint(painter: _GermanyPainter()),
                    ),
                    for (final city in kAllCities)
                      _buildDot(city, ox, oy, mapW, mapH),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDot(
      CityData city, double ox, double oy, double mapW, double mapH) {
    final pos = kCityMapPositions[city.id] ?? const Offset(0.5, 0.5);
    final unlocked = unlockedCityIds.contains(city.id);
    final hasShop = cityIdsWithShops.contains(city.id);
    const dotSize = 16.0;
    final left = ox + pos.dx * mapW - dotSize / 2;
    final top = oy + pos.dy * mapH - dotSize / 2;

    return Positioned(
      left: left - 30,
      top: top - 4,
      width: 60 + dotSize,
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: unlocked ? () => onSelectCity(city.id) : null,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: dotSize,
              height: dotSize,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: unlocked ? MapPalette.success : MapPalette.textDim,
                border: Border.all(
                  color: hasShop ? MapPalette.accent : MapPalette.bgDeep,
                  width: hasShop ? 2.5 : 1.5,
                ),
                boxShadow: unlocked
                    ? [
                        BoxShadow(
                          color: MapPalette.success.withAlpha(120),
                          blurRadius: 8,
                          spreadRadius: 1,
                        )
                      ]
                    : null,
              ),
            ),
            const SizedBox(height: 2),
            Text(
              city.name,
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 10,
                fontWeight: FontWeight.w600,
                color: unlocked ? MapPalette.textMain : MapPalette.textDim,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Hint extends StatelessWidget {
  const _Hint();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: MapPalette.bgPanel,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: MapPalette.border),
      ),
      child: const Text(
        '🇩🇪  Wähle eine freigeschaltete Stadt (grün) für den Stadtplan.',
        style: TextStyle(fontSize: 11, color: MapPalette.textMuted),
      ),
    );
  }
}

class _GermanyPainter extends CustomPainter {
  // Stilisierter Deutschland-Umriss (normalisiert).
  static const List<Offset> _outline = [
    Offset(0.40, 0.05),
    Offset(0.50, 0.07),
    Offset(0.52, 0.13),
    Offset(0.60, 0.10),
    Offset(0.70, 0.16),
    Offset(0.72, 0.30),
    Offset(0.66, 0.45),
    Offset(0.62, 0.55),
    Offset(0.68, 0.64),
    Offset(0.60, 0.82),
    Offset(0.55, 0.93),
    Offset(0.45, 0.83),
    Offset(0.36, 0.74),
    Offset(0.31, 0.62),
    Offset(0.18, 0.52),
    Offset(0.13, 0.42),
    Offset(0.21, 0.33),
    Offset(0.16, 0.24),
    Offset(0.27, 0.18),
    Offset(0.30, 0.10),
    Offset(0.38, 0.06),
  ];

  @override
  void paint(Canvas canvas, Size size) {
    final path = Path();
    for (int i = 0; i < _outline.length; i++) {
      final p = Offset(_outline[i].dx * size.width, _outline[i].dy * size.height);
      if (i == 0) {
        path.moveTo(p.dx, p.dy);
      } else {
        path.lineTo(p.dx, p.dy);
      }
    }
    path.close();

    canvas.drawPath(
      path,
      Paint()
        ..shader = const LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [MapPalette.bgPanel, MapPalette.bgCard],
        ).createShader(Offset.zero & size),
    );
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.5
        ..color = MapPalette.gold.withAlpha(150),
    );
  }

  @override
  bool shouldRepaint(covariant _GermanyPainter oldDelegate) => false;
}
