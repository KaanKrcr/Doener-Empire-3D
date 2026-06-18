import 'package:flutter/material.dart';

/// PROTOTYP — Hybrid-Map-Look.
///
/// Demonstriert das produktive Muster für Mockup-Qualität in Flutter:
///   • Basis = vorgerendertes Gebäude-Sprite (assets/iso/building_owned.png)
///   • darüber = Flutter-gezeichnete, datengetriebene UI-Overlays:
///     Label-Bubble, Boden-Glow (Selektion), Konkurrenz-Pins, Floating-Header.
///
/// Der bräunliche Hintergrund-Halo des Sprites wird per Vignette in den
/// dunklen Map-Hintergrund geblendet (bis ein Sprite mit echtem Alpha vorliegt).
class HybridMapPreview extends StatelessWidget {
  const HybridMapPreview({super.key});

  static const _bg = Color(0xFF07080A);
  static const _accent = Color(0xFFF5A623);
  static const _panel = Color(0xFF121418);
  static const _muted = Color(0xFF8A8E96);
  static const _cream = Color(0xFFF3E9D6);
  static const _divider = Color(0xFF22252B);

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: RadialGradient(
          center: Alignment(0, -0.05),
          radius: 1.15,
          colors: [Color(0xFF12161C), _bg],
        ),
      ),
      child: Stack(
        children: [
          // Boden-Glow unter der aktiven Filiale.
          Align(
            alignment: const Alignment(0, 0.16),
            child: Container(
              width: 480,
              height: 210,
              decoration: const BoxDecoration(
                shape: BoxShape.circle,
                gradient: RadialGradient(
                  colors: [Color(0x55F5A623), Color(0x00F5A623)],
                ),
              ),
            ),
          ),

          // Hero-Sprite + Vignette (blendet den Sprite-Halo in den BG).
          Align(
            alignment: const Alignment(0, 0.0),
            child: SizedBox(
              width: 700,
              height: 480,
              child: Stack(
                children: [
                  Positioned.fill(
                    child: Image.asset(
                      'assets/iso/building_owned.png',
                      fit: BoxFit.contain,
                      filterQuality: FilterQuality.medium,
                    ),
                  ),
                  const Positioned.fill(
                    child: DecoratedBox(
                      decoration: BoxDecoration(
                        gradient: RadialGradient(
                          center: Alignment(0, 0.05),
                          radius: 0.72,
                          colors: [Color(0x0007080A), _bg],
                          stops: [0.6, 1.0],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),

          // Label-Bubble der aktiven Filiale (über dem Gebäude).
          const Align(
            alignment: Alignment(0, -0.42),
            child: _LabelBubble(name: 'HAUPTSTRASSE 12', rating: 4.6),
          ),

          // Konkurrenz-Pins (gedämpft, kein Glow → treten zurück).
          const Align(
            alignment: Alignment(-0.74, -0.30),
            child: _CompPin(name: 'LEZZET DÖNER', rating: 3.2),
          ),
          const Align(
            alignment: Alignment(0.76, -0.34),
            child: _CompPin(name: 'CITY KEBAP', rating: 3.6),
          ),
          const Align(
            alignment: Alignment(-0.62, 0.40),
            child: _CompPin(name: 'BERLIN DÖNER', rating: 2.8, bad: true),
          ),

          // Floating-Header (Kontostand + Tag) wie im Mockup.
          const Positioned(
            top: 14,
            left: 14,
            right: 14,
            child: _HeaderPill(cash: '€ 1.248.750', day: 'Tag 47'),
          ),
        ],
      ),
    );
  }
}

class _LabelBubble extends StatelessWidget {
  final String name;
  final double rating;
  const _LabelBubble({required this.name, required this.rating});

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 11),
          decoration: BoxDecoration(
            color: HybridMapPreview._panel.withAlpha(235),
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: HybridMapPreview._accent, width: 1.5),
            boxShadow: const [
              BoxShadow(color: Color(0x99000000), blurRadius: 20, offset: Offset(0, 8)),
            ],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                name,
                style: const TextStyle(
                  color: HybridMapPreview._cream,
                  fontSize: 17,
                  fontWeight: FontWeight.w800,
                  letterSpacing: 0.3,
                ),
              ),
              const SizedBox(width: 10),
              const Icon(Icons.star, color: HybridMapPreview._accent, size: 15),
              const SizedBox(width: 3),
              Text(
                rating.toStringAsFixed(1),
                style: const TextStyle(
                  color: HybridMapPreview._cream,
                  fontSize: 15,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),
        ),
        // Tail nach unten
        CustomPaint(
          size: const Size(16, 9),
          painter: _TailPainter(HybridMapPreview._panel.withAlpha(235)),
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

class _CompPin extends StatelessWidget {
  final String name;
  final double rating;
  final bool bad;
  const _CompPin({required this.name, required this.rating, this.bad = false});

  @override
  Widget build(BuildContext context) {
    final pinColor = bad ? const Color(0xFF3E6B4F) : const Color(0xFF5A6470);
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
          decoration: BoxDecoration(
            color: const Color(0xFF1C1F25).withAlpha(220),
            borderRadius: BorderRadius.circular(8),
            border: Border.all(color: HybridMapPreview._divider),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                name,
                style: const TextStyle(
                  color: HybridMapPreview._muted,
                  fontSize: 11,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.4,
                ),
              ),
              const SizedBox(height: 1),
              Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.star, color: pinColor, size: 10),
                  const SizedBox(width: 2),
                  Text(
                    rating.toStringAsFixed(1),
                    style: TextStyle(
                      color: HybridMapPreview._muted.withAlpha(220),
                      fontSize: 10,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        const SizedBox(height: 2),
        Icon(Icons.location_on, color: pinColor, size: 22),
      ],
    );
  }
}

class _HeaderPill extends StatelessWidget {
  final String cash;
  final String day;
  const _HeaderPill({required this.cash, required this.day});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
      decoration: BoxDecoration(
        color: HybridMapPreview._panel.withAlpha(235),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: HybridMapPreview._divider),
      ),
      child: Row(
        children: [
          const Icon(Icons.payments_outlined, color: Color(0xFF7FB069), size: 22),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(cash,
                  style: const TextStyle(
                      color: Color(0xFFF5F3EF),
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              const Text('KONTOSTAND',
                  style: TextStyle(
                      color: HybridMapPreview._muted,
                      fontSize: 9,
                      letterSpacing: 1.5)),
            ],
          ),
          const Spacer(),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(day,
                  style: const TextStyle(
                      color: Color(0xFFF5F3EF),
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              const Text('MITTWOCH',
                  style: TextStyle(
                      color: HybridMapPreview._muted,
                      fontSize: 9,
                      letterSpacing: 1.5)),
            ],
          ),
          const SizedBox(width: 6),
          const Icon(Icons.calendar_today_outlined,
              color: HybridMapPreview._muted, size: 20),
        ],
      ),
    );
  }
}
