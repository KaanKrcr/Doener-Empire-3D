import 'package:flutter/material.dart';

import '../../core/constants.dart';
import '../../models/city_model.dart';
import 'building_styles.dart';

/// Ungefähre, stilisierte Position jeder Stadt auf dem Deutschland-Umriss
/// (normalisiert 0..1; x = West→Ost, y = Nord→Süd).
const Map<String, Offset> kCityMapPositions = {
  'hamburg': Offset(0.47, 0.18),
  'berlin': Offset(0.70, 0.25),
  'muenster': Offset(0.30, 0.34),
  'braunschweig': Offset(0.55, 0.30),
  'goettingen': Offset(0.46, 0.40),
  'duesseldorf': Offset(0.27, 0.42),
  'koeln': Offset(0.29, 0.47),
  'frankfurt': Offset(0.40, 0.52),
  'fulda': Offset(0.47, 0.49),
  'bayreuth': Offset(0.62, 0.54),
  'stuttgart': Offset(0.42, 0.64),
  'augsburg': Offset(0.54, 0.70),
  'muenchen': Offset(0.60, 0.72),
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
      decoration: const BoxDecoration(
        gradient: RadialGradient(
          center: Alignment.topCenter,
          radius: 1.2,
          colors: [Color(0xFF0C0E11), Color(0xFF07080A)],
        ),
      ),
      padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
      child: Column(
        children: [
          const _Hint(),
          const SizedBox(height: 8),
          Expanded(
            child: LayoutBuilder(
              builder: (context, c) {
                const aspect = 0.82; // Deutschland ist höher als breit
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
                      ..._buildCity(city, ox, oy, mapW, mapH),
                  ],
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  List<Widget> _buildCity(
      CityData city, double ox, double oy, double mapW, double mapH) {
    final pos = kCityMapPositions[city.id] ?? const Offset(0.5, 0.5);
    final unlocked = unlockedCityIds.contains(city.id);
    final hasShop = cityIdsWithShops.contains(city.id);
    const dot = 14.0;
    final dx = ox + pos.dx * mapW;
    final dy = oy + pos.dy * mapH;

    return [
      // Punkt
      Positioned(
        left: dx - dot / 2,
        top: dy - dot / 2,
        child: GestureDetector(
          behavior: HitTestBehavior.opaque,
          onTap: unlocked ? () => onSelectCity(city.id) : null,
          child: Container(
            width: dot,
            height: dot,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              color: unlocked
                  ? const Color(0xFF7BC950)
                  : const Color(0xFF5C606A),
              border: Border.all(
                color: hasShop ? MapPalette.accent : const Color(0xFF07080A),
                width: hasShop ? 2.5 : 1.5,
              ),
              boxShadow: unlocked
                  ? [
                      const BoxShadow(
                        color: Color(0x807BC950),
                        blurRadius: 8,
                        spreadRadius: 1,
                      )
                    ]
                  : null,
            ),
          ),
        ),
      ),
      // Name rechts vom Punkt
      Positioned(
        left: dx + dot / 2 + 4,
        top: dy - 7,
        child: IgnorePointer(
          child: SizedBox(
            width: 78,
            child: Text(
              city.name,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: TextStyle(
                fontSize: 11,
                fontWeight: FontWeight.w600,
                color: unlocked
                    ? MapPalette.textMain
                    : const Color(0xFF5C606A),
              ),
            ),
          ),
        ),
      ),
    ];
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
  // Vereinfachte Deutschland-Kontur (normalisiert, im Uhrzeigersinn ab Norden):
  // Kiel → Usedom → Görlitz → Passau → Oberstdorf → Saarbrücken → Aachen → Nordsee.
  static const List<Offset> _outline = [
    Offset(0.50, 0.05), // N — Kiel/Flensburg
    Offset(0.66, 0.08),
    Offset(0.85, 0.10), // NO — Usedom
    Offset(0.88, 0.22),
    Offset(0.90, 0.35), // O — Görlitz
    Offset(0.85, 0.50),
    Offset(0.80, 0.60), // SO — Passau
    Offset(0.66, 0.70),
    Offset(0.55, 0.76), // S — Oberstdorf
    Offset(0.45, 0.70),
    Offset(0.30, 0.65), // SW — Saarbrücken
    Offset(0.24, 0.50),
    Offset(0.20, 0.35), // W — Aachen
    Offset(0.22, 0.22),
    Offset(0.25, 0.10), // NW — Borkum/Nordsee
    Offset(0.37, 0.07),
  ];

  @override
  void paint(Canvas canvas, Size size) {
    final pts = _outline
        .map((o) => Offset(o.dx * size.width, o.dy * size.height))
        .toList();
    final n = pts.length;

    Offset mid(int i) => (pts[i] + pts[(i + 1) % n]) / 2;

    // Geglättete Kontur: quadratische Beziers durch die Kantenmittelpunkte,
    // mit den Eckpunkten als Kontrollpunkten → organischer Umriss.
    final path = Path();
    final start = mid(n - 1);
    path.moveTo(start.dx, start.dy);
    for (int i = 0; i < n; i++) {
      final c = pts[i];
      final m = mid(i);
      path.quadraticBezierTo(c.dx, c.dy, m.dx, m.dy);
    }
    path.close();

    canvas.drawPath(
      path,
      Paint()..color = const Color(0xF2121418), // #121418, leicht transparent
    );
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2
        ..color = const Color(0xFF22252B),
    );
  }

  @override
  bool shouldRepaint(covariant _GermanyPainter oldDelegate) => false;
}
