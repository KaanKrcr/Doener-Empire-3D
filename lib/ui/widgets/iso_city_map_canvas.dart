import 'package:flutter/material.dart';

import 'iso_city_map_painter.dart';

/// Eine Hero-Filiale, die als vorgerendertes Sprite über dem Vektor-Feld
/// gezeichnet wird (statt des Vektor-Körpers — [IsoBuilding.body] = false).
class HeroSprite {
  final IsoBuilding building;
  final String asset;
  final String? label;
  final double? rating;
  const HeroSprite({
    required this.building,
    required this.asset,
    this.label,
    this.rating,
  });
}

/// Präsentations-Layer der Iso-Stadtkarte: dunkles Vektor-Feld
/// ([IsoMapPainter]) plus darüberliegende Hero-Sprites mit Label-Bubble.
///
/// Sprite und Painter nutzen dieselbe statische Projektion
/// ([IsoMapPainter.originFor]/[IsoMapPainter.projectTile]), daher sitzt das
/// Sprite exakt auf seiner Kachel. Der eingebackene Sprite-Halo wird per
/// [ShaderMask] (radialer Alpha-Cut) entfernt, funktioniert über jedem BG.
class IsoCityMapCanvas extends StatelessWidget {
  final List<IsoBuilding> buildings;
  final List<HeroSprite> heroes;
  final Size scene;
  final double spriteWidth;

  const IsoCityMapCanvas({
    super.key,
    required this.buildings,
    required this.heroes,
    required this.scene,
    this.spriteWidth = 230,
  });

  static const _bg = Color(0xFF07080A);
  static const _accent = Color(0xFFF5A623);
  static const _cream = Color(0xFFF3E9D6);
  static const _panel = Color(0xFF121418);

  @override
  Widget build(BuildContext context) {
    final origin = IsoMapPainter.originFor(buildings, scene);
    final spriteHeight = spriteWidth * 1024 / 1536; // Sprite-Seitenverhältnis

    return SizedBox(
      width: scene.width,
      height: scene.height,
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          Positioned.fill(
            child: CustomPaint(painter: IsoMapPainter(buildings: buildings)),
          ),
          for (final h in heroes) ..._heroLayer(h, origin, spriteHeight),
        ],
      ),
    );
  }

  List<Widget> _heroLayer(HeroSprite h, Offset origin, double spriteHeight) {
    final pos = IsoMapPainter.projectTile(h.building.tx, h.building.ty, origin);
    final left = pos.dx - spriteWidth / 2;
    final top = pos.dy - spriteHeight * 0.80; // Gebäude ragt über den Fußpunkt
    return [
      Positioned(
        left: left,
        top: top,
        width: spriteWidth,
        height: spriteHeight,
        child: ShaderMask(
          blendMode: BlendMode.dstIn,
          shaderCallback: (rect) => const RadialGradient(
            center: Alignment(0, 0.05),
            radius: 0.72,
            colors: [Colors.white, Colors.white, Colors.transparent],
            stops: [0.0, 0.6, 1.0],
          ).createShader(rect),
          child: Image.asset(
            h.asset,
            fit: BoxFit.contain,
            filterQuality: FilterQuality.medium,
            errorBuilder: (_, __, ___) => const SizedBox.shrink(),
          ),
        ),
      ),
      if (h.label != null)
        Positioned(
          left: pos.dx - 110,
          top: top - 30,
          width: 220,
          child: Center(child: _LabelBubble(name: h.label!, rating: h.rating)),
        ),
    ];
  }
}

class _LabelBubble extends StatelessWidget {
  final String name;
  final double? rating;
  const _LabelBubble({required this.name, this.rating});

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
          decoration: BoxDecoration(
            color: IsoCityMapCanvas._panel.withAlpha(235),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: IsoCityMapCanvas._accent, width: 1.5),
            boxShadow: const [
              BoxShadow(
                  color: Color(0x99000000),
                  blurRadius: 16,
                  offset: Offset(0, 6)),
            ],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Flexible(
                child: Text(
                  name,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                      color: IsoCityMapCanvas._cream,
                      fontSize: 14,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.3),
                ),
              ),
              if (rating != null) ...[
                const SizedBox(width: 8),
                const Icon(Icons.star,
                    color: IsoCityMapCanvas._accent, size: 13),
                const SizedBox(width: 3),
                Text(rating!.toStringAsFixed(1),
                    style: const TextStyle(
                        color: IsoCityMapCanvas._cream,
                        fontSize: 13,
                        fontWeight: FontWeight.w700)),
              ],
            ],
          ),
        ),
        CustomPaint(
          size: const Size(14, 8),
          painter: _TailPainter(IsoCityMapCanvas._panel.withAlpha(235)),
        ),
      ],
    );
  }
}

class _TailPainter extends CustomPainter {
  final Color color;
  _TailPainter(this.color);
  @override
  void paint(Canvas canvas, Size size) {
    final p = Path()
      ..moveTo(0, 0)
      ..lineTo(size.width / 2, size.height)
      ..lineTo(size.width, 0)
      ..close();
    canvas.drawPath(p, Paint()..color = color);
  }

  @override
  bool shouldRepaint(covariant _TailPainter old) => old.color != color;
}
