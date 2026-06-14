import 'package:flutter/material.dart';

/// PROTOTYP — vollständiger „Übersicht"-Screen im Mockup-Look (Hybrid).
///
/// Map-Bereich = vorgerendertes Sprite (assets/iso/building_owned.png) +
/// Flutter-Overlays (Label-Bubble, Boden-Glow, Konkurrenz-Pins, Header,
/// Map-Controls). Darunter das Detail-Panel (Reputation, Stat-Kacheln,
/// Aktionen) und die Bottom-Nav — alles in Flutter gezeichnet.
///
/// Zeigt, wie nah Flutter + Sprites an den Ziel-Screen kommt, ohne 3D-Engine.
class HybridShopScreen extends StatelessWidget {
  const HybridShopScreen({super.key});

  static const _bg = Color(0xFF07080A);
  static const _panel = Color(0xFF121418);
  static const _panelHi = Color(0xFF1A1D22);
  static const _accent = Color(0xFFF5A623);
  static const _accentDeep = Color(0xFFE07B1A);
  static const _muted = Color(0xFF8A8E96);
  static const _dim = Color(0xFF5C606A);
  static const _cream = Color(0xFFF3E9D6);
  static const _white = Color(0xFFF5F3EF);
  static const _divider = Color(0xFF22252B);
  static const _money = Color(0xFF7FB069);

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: _bg,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: const [
          Expanded(child: _MapArea()),
          _DetailPanel(),
          _BottomNav(),
        ],
      ),
    );
  }
}

// ─────────────────────────────────────────────────────────── Map-Bereich ──

class _MapArea extends StatelessWidget {
  const _MapArea();

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: RadialGradient(
          center: Alignment(0, -0.1),
          radius: 1.2,
          colors: [Color(0xFF12161C), HybridShopScreen._bg],
        ),
      ),
      child: Stack(
        children: [
          // Boden-Glow unter aktiver Filiale
          Align(
            alignment: const Alignment(0, 0.45),
            child: Container(
              width: 460,
              height: 200,
              decoration: const BoxDecoration(
                shape: BoxShape.circle,
                gradient: RadialGradient(
                  colors: [Color(0x55F5A623), Color(0x00F5A623)],
                ),
              ),
            ),
          ),
          // Hero-Sprite + Vignette
          Align(
            alignment: const Alignment(0, 0.25),
            child: SizedBox(
              width: 640,
              height: 430,
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
                          colors: [Color(0x0007080A), HybridShopScreen._bg],
                          stops: [0.62, 1.0],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          // Label-Bubble
          const Align(
            alignment: Alignment(0, -0.32),
            child: _LabelBubble(name: 'HAUPTSTRASSE 12', rating: 4.6),
          ),
          // Konkurrenz-Pins
          const Align(
            alignment: Alignment(-0.78, -0.12),
            child: _CompPin(name: 'LEZZET DÖNER', rating: 3.2),
          ),
          const Align(
            alignment: Alignment(0.8, -0.2),
            child: _CompPin(name: 'CITY KEBAP', rating: 3.6),
          ),
          const Align(
            alignment: Alignment(-0.66, 0.55),
            child: _CompPin(name: 'BERLIN DÖNER', rating: 2.8, bad: true),
          ),
          // Floating-Header
          const Positioned(
            top: 14,
            left: 14,
            right: 14,
            child: _HeaderPill(cash: '€ 1.248.750', day: 'Tag 47'),
          ),
          // Map-Controls rechts
          Positioned(
            right: 12,
            top: 0,
            bottom: 0,
            child: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: const [
                  _MapButton(Icons.my_location),
                  SizedBox(height: 10),
                  _MapButton(Icons.map_outlined),
                  SizedBox(height: 10),
                  _MapButton(Icons.show_chart),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _MapButton extends StatelessWidget {
  final IconData icon;
  const _MapButton(this.icon);
  @override
  Widget build(BuildContext context) {
    return Container(
      width: 46,
      height: 46,
      decoration: BoxDecoration(
        color: HybridShopScreen._panel.withAlpha(205),
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: HybridShopScreen._divider),
      ),
      child: Icon(icon, color: HybridShopScreen._muted, size: 20),
    );
  }
}

// ───────────────────────────────────────────────────────── Detail-Panel ──

class _DetailPanel extends StatelessWidget {
  const _DetailPanel();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(18, 10, 18, 16),
      decoration: const BoxDecoration(
        color: HybridShopScreen._panel,
        borderRadius: BorderRadius.vertical(top: Radius.circular(26)),
        border: Border(top: BorderSide(color: HybridShopScreen._divider)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Center(
            child: Container(
              width: 40,
              height: 5,
              decoration: BoxDecoration(
                color: const Color(0xFF3A3D44),
                borderRadius: BorderRadius.circular(3),
              ),
            ),
          ),
          const SizedBox(height: 14),
          // Kopfzeile
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: const [
                    Text('Hauptstrasse 12',
                        style: TextStyle(
                            color: HybridShopScreen._cream,
                            fontSize: 26,
                            fontWeight: FontWeight.w800)),
                    SizedBox(height: 2),
                    Row(children: [
                      Icon(Icons.place_outlined,
                          color: HybridShopScreen._muted, size: 14),
                      SizedBox(width: 4),
                      Text('MITTE, BERLIN',
                          style: TextStyle(
                              color: HybridShopScreen._muted,
                              fontSize: 12,
                              letterSpacing: 1)),
                    ]),
                  ],
                ),
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Row(
                    children: const [
                      Text('AKTIVE FILIALE',
                          style: TextStyle(
                              color: HybridShopScreen._accent,
                              fontSize: 10,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 1)),
                      SizedBox(width: 5),
                      _GlowDot(),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Row(
                    children: [
                      ...List.generate(
                          4,
                          (_) => const Icon(Icons.star,
                              color: HybridShopScreen._accent, size: 18)),
                      const Icon(Icons.star_half,
                          color: HybridShopScreen._accent, size: 18),
                      const SizedBox(width: 6),
                      const Text('4.6',
                          style: TextStyle(
                              color: HybridShopScreen._cream,
                              fontSize: 20,
                              fontWeight: FontWeight.w800)),
                    ],
                  ),
                  const Text('REPUTATION',
                      style: TextStyle(
                          color: HybridShopScreen._muted,
                          fontSize: 9,
                          letterSpacing: 1.5)),
                ],
              ),
            ],
          ),
          const SizedBox(height: 16),
          // Stat-Kacheln
          Row(
            children: const [
              Expanded(child: _DonutTile()),
              SizedBox(width: 10),
              Expanded(
                  child: _StatTile(
                      label: 'FUSSGÄNGER',
                      icon: Icons.groups_outlined,
                      value: '1.842',
                      unit: 'PRO TAG')),
              SizedBox(width: 10),
              Expanded(
                  child: _StatTile(
                      label: 'WOCHENMIETE',
                      icon: Icons.receipt_long_outlined,
                      value: '€ 8.750',
                      unit: 'PRO WOCHE')),
              SizedBox(width: 10),
              Expanded(
                  child: _StatTile(
                      label: 'PROGNOSE',
                      icon: Icons.bar_chart,
                      value: '€ 15.430',
                      unit: 'NÄCHSTE WOCHE')),
            ],
          ),
          const SizedBox(height: 16),
          // Aktionen
          Row(
            children: [
              Expanded(
                flex: 5,
                child: Container(
                  height: 56,
                  decoration: BoxDecoration(
                    gradient: const LinearGradient(
                      colors: [
                        HybridShopScreen._accent,
                        HybridShopScreen._accentDeep
                      ],
                      begin: Alignment.topLeft,
                      end: Alignment.bottomRight,
                    ),
                    borderRadius: BorderRadius.circular(15),
                    boxShadow: const [
                      BoxShadow(
                          color: Color(0x66F5A623),
                          blurRadius: 22,
                          offset: Offset(0, 6)),
                    ],
                  ),
                  child: const Center(
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.trending_up,
                            color: Color(0xFF1A1209), size: 20),
                        SizedBox(width: 8),
                        Text('OPTIMIEREN',
                            style: TextStyle(
                                color: Color(0xFF1A1209),
                                fontSize: 16,
                                fontWeight: FontWeight.w800,
                                letterSpacing: 0.5)),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                flex: 5,
                child: Container(
                  height: 56,
                  decoration: BoxDecoration(
                    color: HybridShopScreen._panelHi,
                    borderRadius: BorderRadius.circular(15),
                    border: Border.all(
                        color: HybridShopScreen._divider, width: 1.5),
                  ),
                  child: const Center(
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.add_circle_outline,
                            color: HybridShopScreen._cream, size: 18),
                        SizedBox(width: 8),
                        Text('FILIALE ÖFFNEN',
                            style: TextStyle(
                                color: HybridShopScreen._cream,
                                fontSize: 14,
                                fontWeight: FontWeight.w700)),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _GlowDot extends StatelessWidget {
  const _GlowDot();
  @override
  Widget build(BuildContext context) {
    return Container(
      width: 9,
      height: 9,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: HybridShopScreen._accent,
        boxShadow: const [
          BoxShadow(color: Color(0xAAF5A623), blurRadius: 8, spreadRadius: 1),
        ],
      ),
    );
  }
}

class _StatTile extends StatelessWidget {
  final String label;
  final IconData icon;
  final String value;
  final String unit;
  const _StatTile(
      {required this.label,
      required this.icon,
      required this.value,
      required this.unit});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 150,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: HybridShopScreen._panelHi,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label,
              style: const TextStyle(
                  color: HybridShopScreen._muted,
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.5)),
          const SizedBox(height: 8),
          Icon(icon, color: HybridShopScreen._dim, size: 20),
          const Spacer(),
          Text(value,
              style: const TextStyle(
                  color: HybridShopScreen._white,
                  fontSize: 19,
                  fontWeight: FontWeight.w800)),
          Text(unit,
              style: const TextStyle(
                  color: HybridShopScreen._muted, fontSize: 8.5)),
          const SizedBox(height: 6),
          SizedBox(
            height: 18,
            width: double.infinity,
            child: CustomPaint(painter: _SparkPainter()),
          ),
        ],
      ),
    );
  }
}

class _DonutTile extends StatelessWidget {
  const _DonutTile();
  @override
  Widget build(BuildContext context) {
    return Container(
      height: 150,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: HybridShopScreen._panelHi,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('MARKTANTEIL',
              style: TextStyle(
                  color: HybridShopScreen._muted,
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.5)),
          const Spacer(),
          Center(
            child: SizedBox(
              width: 74,
              height: 74,
              child: CustomPaint(
                painter: _DonutPainter(0.34),
                child: const Center(
                  child: Text('34%',
                      style: TextStyle(
                          color: HybridShopScreen._white,
                          fontSize: 18,
                          fontWeight: FontWeight.w800)),
                ),
              ),
            ),
          ),
          const Spacer(),
          const Center(
            child: Text('STADT: 12%',
                style: TextStyle(
                    color: HybridShopScreen._muted, fontSize: 9)),
          ),
        ],
      ),
    );
  }
}

class _DonutPainter extends CustomPainter {
  final double value;
  _DonutPainter(this.value);
  @override
  void paint(Canvas canvas, Size size) {
    final rect = Offset.zero & size;
    final center = rect.center;
    final radius = size.width / 2 - 7;
    final track = Paint()
      ..color = const Color(0xFF2A2E35)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 12;
    final arc = Paint()
      ..color = HybridShopScreen._accent
      ..style = PaintingStyle.stroke
      ..strokeWidth = 12
      ..strokeCap = StrokeCap.round;
    canvas.drawCircle(center, radius, track);
    canvas.drawArc(Rect.fromCircle(center: center, radius: radius), -1.5708,
        value * 6.2832, false, arc);
  }

  @override
  bool shouldRepaint(covariant _DonutPainter old) => old.value != value;
}

class _SparkPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    const pts = [0.2, 0.35, 0.25, 0.5, 0.42, 0.65, 0.6, 0.8, 0.95];
    final paint = Paint()
      ..color = HybridShopScreen._accent
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round;
    final path = Path();
    for (var i = 0; i < pts.length; i++) {
      final x = i / (pts.length - 1) * size.width;
      final y = size.height - pts[i] * size.height;
      i == 0 ? path.moveTo(x, y) : path.lineTo(x, y);
    }
    canvas.drawPath(path, paint);
  }

  @override
  bool shouldRepaint(covariant _SparkPainter old) => false;
}

// ─────────────────────────────────────────────────────────── Bottom-Nav ──

class _BottomNav extends StatelessWidget {
  const _BottomNav();
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.only(top: 8, bottom: 10),
      color: HybridShopScreen._bg,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: const [
          _NavItem(Icons.home_filled, 'ÜBERSICHT', active: true),
          _NavItem(Icons.location_on_outlined, 'FILIALEN'),
          _NavItem(Icons.groups_outlined, 'MANAGER'),
          _NavItem(Icons.science_outlined, 'FORSCHUNG'),
          _NavItem(Icons.shopping_cart_outlined, 'SHOP'),
        ],
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final bool active;
  const _NavItem(this.icon, this.label, {this.active = false});
  @override
  Widget build(BuildContext context) {
    final c = active ? HybridShopScreen._accent : HybridShopScreen._dim;
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, color: c, size: 22),
        const SizedBox(height: 4),
        Text(label,
            style: TextStyle(
                color: c,
                fontSize: 9,
                fontWeight: active ? FontWeight.w700 : FontWeight.w500)),
      ],
    );
  }
}

// ─────────────────────────────────── geteilte Overlay-Bausteine (Map) ──

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
            color: HybridShopScreen._panel.withAlpha(235),
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: HybridShopScreen._accent, width: 1.5),
            boxShadow: const [
              BoxShadow(
                  color: Color(0x99000000),
                  blurRadius: 20,
                  offset: Offset(0, 8)),
            ],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(name,
                  style: const TextStyle(
                      color: HybridShopScreen._cream,
                      fontSize: 16,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.3)),
              const SizedBox(width: 10),
              const Icon(Icons.star,
                  color: HybridShopScreen._accent, size: 14),
              const SizedBox(width: 3),
              Text(rating.toStringAsFixed(1),
                  style: const TextStyle(
                      color: HybridShopScreen._cream,
                      fontSize: 14,
                      fontWeight: FontWeight.w700)),
            ],
          ),
        ),
        CustomPaint(
          size: const Size(16, 9),
          painter: _TailPainter(HybridShopScreen._panel.withAlpha(235)),
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
            border: Border.all(color: HybridShopScreen._divider),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(name,
                  style: const TextStyle(
                      color: HybridShopScreen._muted,
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      letterSpacing: 0.4)),
              const SizedBox(height: 1),
              Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.star, color: pinColor, size: 10),
                  const SizedBox(width: 2),
                  Text(rating.toStringAsFixed(1),
                      style: TextStyle(
                          color: HybridShopScreen._muted.withAlpha(220),
                          fontSize: 10,
                          fontWeight: FontWeight.w600)),
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
        color: HybridShopScreen._panel.withAlpha(235),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: HybridShopScreen._divider),
      ),
      child: Row(
        children: [
          const Icon(Icons.payments_outlined,
              color: HybridShopScreen._money, size: 22),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(cash,
                  style: const TextStyle(
                      color: HybridShopScreen._white,
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              const Text('KONTOSTAND',
                  style: TextStyle(
                      color: HybridShopScreen._muted,
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
                      color: HybridShopScreen._white,
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              const Text('MITTWOCH',
                  style: TextStyle(
                      color: HybridShopScreen._muted,
                      fontSize: 9,
                      letterSpacing: 1.5)),
            ],
          ),
          const SizedBox(width: 6),
          const Icon(Icons.calendar_today_outlined,
              color: HybridShopScreen._muted, size: 20),
        ],
      ),
    );
  }
}
