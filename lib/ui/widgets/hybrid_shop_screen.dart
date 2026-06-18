import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../providers/game_provider.dart';
import '../../models/shop_model.dart';
import '../../services/game_engine.dart';
import '../widgets/building_styles.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

const _weekdays = [
  'MONTAG', 'DIENSTAG', 'MITTWOCH', 'DONNERSTAG',
  'FREITAG', 'SAMSTAG', 'SONNTAG',
];

/// Übersicht-Screen für eine einzelne Filiale.
/// Map-Bereich + Detail-Panel + Bottom-Nav — an echten GameState gebunden.
class HybridShopScreen extends ConsumerWidget {
  final String shopId;

  const HybridShopScreen({super.key, required this.shopId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final game = ref.watch(gameProvider)!;
    final shop = game.shops.firstWhere(
      (s) => s.id == shopId,
      orElse: () => game.shops.first,
    );
    final cash = game.cash;
    final day = game.currentDay;
    final weekday = _weekdays[(day - 1) % 7];

    // Berechnete Werte
    final rating = shop.reputation;
    final locationLabel =
        '${shop.locationName.toUpperCase()}, ${shop.cityId.toUpperCase()}';
    final footTraffic = shop.footTraffic;
    final weeklyRent = shop.weeklyRent;

    final dailyRevenue =
        GameEngine.calculateDailyRevenue(shop, day: day, state: game);
    final prognosis = dailyRevenue * 7;

    // Marktanteil: Anteil der Spieler-Filialen an Gesamt-Filialen in der Stadt
    final cityShops =
        game.shops.where((s) => s.cityId == shop.cityId).toList();
    final cityCompetitors = game.competitorsIn(shop.cityId);
    final totalShopCount = cityShops.length +
        cityCompetitors.fold<int>(0, (s, c) => s + c.shopCount);
    final marketShare =
        totalShopCount > 0 ? cityShops.length / totalShopCount : 0.0;

    return ColoredBox(
      color: MapPalette.bgDeep,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Expanded(child: _MapArea(shop: shop, rating: rating, cash: cash, day: day, weekday: weekday)),
          _DetailPanel(
            shop: shop,
            rating: rating,
            locationLabel: locationLabel,
            footTraffic: footTraffic,
            weeklyRent: weeklyRent,
            prognosis: prognosis,
            marketShare: marketShare,
            totalCityShops: totalShopCount,
            playerCityShops: cityShops.length,
          ),
          _BottomNav(shopId: shop.id, cityId: shop.cityId),
        ],
      ),
    );
  }
}

// ─────────────────────────────────────────────────────────── Map-Bereich ──

class _MapArea extends StatelessWidget {
  final Shop shop;
  final double rating;
  final double cash;
  final int day;
  final String weekday;

  const _MapArea({
    required this.shop,
    required this.rating,
    required this.cash,
    required this.day,
    required this.weekday,
  });

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: RadialGradient(
          center: Alignment(0, -0.1),
          radius: 1.2,
          colors: [Color(0xFF12161C), MapPalette.bgDeep],
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
                          colors: [Color(0x0007080A), MapPalette.bgDeep],
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
          Align(
            alignment: const Alignment(0, -0.32),
            child: _LabelBubble(name: shop.displayName, rating: rating),
          ),
          // Floating-Header
          Positioned(
            top: 14,
            left: 14,
            right: 14,
            child: _HeaderPill(
              cash: '€ ${_fmt.format(cash.round())}',
              day: 'Tag $day',
              weekday: weekday,
            ),
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
        color: MapPalette.bgPanel.withAlpha(205),
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: MapPalette.border),
      ),
      child: Icon(icon, color: MapPalette.textMuted, size: 20),
    );
  }
}

// ───────────────────────────────────────────────────────── Detail-Panel ──

class _DetailPanel extends StatelessWidget {
  final Shop shop;
  final double rating;
  final String locationLabel;
  final int footTraffic;
  final double weeklyRent;
  final double prognosis;
  final double marketShare;
  final int totalCityShops;
  final int playerCityShops;

  const _DetailPanel({
    required this.shop,
    required this.rating,
    required this.locationLabel,
    required this.footTraffic,
    required this.weeklyRent,
    required this.prognosis,
    required this.marketShare,
    required this.totalCityShops,
    required this.playerCityShops,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(18, 10, 18, 16),
      decoration: const BoxDecoration(
        color: MapPalette.bgPanel,
        borderRadius: BorderRadius.vertical(top: Radius.circular(26)),
        border: Border(top: BorderSide(color: MapPalette.border)),
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
                  children: [
                    Text(
                      shop.displayName,
                      style: const TextStyle(
                          color: MapPalette.textMain,
                          fontSize: 26,
                          fontWeight: FontWeight.w800),
                    ),
                    const SizedBox(height: 2),
                    Row(children: [
                      const Icon(Icons.place_outlined,
                          color: MapPalette.textMuted, size: 14),
                      const SizedBox(width: 4),
                      Text(
                        locationLabel,
                        style: const TextStyle(
                            color: MapPalette.textMuted,
                            fontSize: 12,
                            letterSpacing: 1),
                      ),
                    ]),
                  ],
                ),
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  const Row(
                    children: [
                      Text('AKTIVE FILIALE',
                          style: TextStyle(
                              color: MapPalette.accent,
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
                        rating.floor(),
                        (_) => const Icon(Icons.star,
                            color: MapPalette.accent, size: 18),
                      ),
                      if (rating - rating.floor() >= 0.5 && rating < 5.0)
                        const Icon(Icons.star_half,
                            color: MapPalette.accent, size: 18),
                      if (rating < 5.0)
                        ...List.generate(
                          (5 -
                                  rating.floor() -
                                  (rating - rating.floor() >= 0.5 ? 1 : 0))
                              .clamp(0, 5),
                          (_) => const Icon(Icons.star_border,
                              color: MapPalette.textDim, size: 18),
                        ),
                      const SizedBox(width: 6),
                      Text(
                        rating.toStringAsFixed(1),
                        style: const TextStyle(
                            color: MapPalette.textMain,
                            fontSize: 20,
                            fontWeight: FontWeight.w800),
                      ),
                    ],
                  ),
                  const Text('REPUTATION',
                      style: TextStyle(
                          color: MapPalette.textMuted,
                          fontSize: 9,
                          letterSpacing: 1.5)),
                ],
              ),
            ],
          ),
          const SizedBox(height: 16),
          // Stat-Kacheln
          Row(
            children: [
              Expanded(
                child: _DonutTile(
                  value: marketShare,
                  cityShare: _cityShare(),
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: _StatTile(
                  label: 'FUSSGÄNGER',
                  icon: Icons.groups_outlined,
                  value: _fmt.format(footTraffic),
                  unit: 'PRO TAG',
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: _StatTile(
                  label: 'WOCHENMIETE',
                  icon: Icons.receipt_long_outlined,
                  value: '€ ${_fmt.format(weeklyRent.round())}',
                  unit: 'PRO WOCHE',
                ),
              ),
              const SizedBox(width: 10),
              Expanded(
                child: _StatTile(
                  label: 'PROGNOSE',
                  icon: Icons.bar_chart,
                  value: '€ ${_fmt.format(prognosis.round())}',
                  unit: 'NÄCHSTE WOCHE',
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          // Aktionen
          Row(
            children: [
              Expanded(
                flex: 5,
                child: InkWell(
                  onTap: () => context.push('/shop/${shop.id}'),
                  child: Container(
                    height: 56,
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [
                          MapPalette.accent,
                          MapPalette.gold,
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
              ),
              const SizedBox(width: 12),
              Expanded(
                flex: 5,
                child: InkWell(
                  onTap: () =>
                      context.push('/open-shop/${shop.cityId}'),
                  child: Container(
                    height: 56,
                    decoration: BoxDecoration(
                      color: MapPalette.bgCard,
                      borderRadius: BorderRadius.circular(15),
                      border: Border.all(
                          color: MapPalette.border, width: 1.5),
                    ),
                    child: const Center(
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Icon(Icons.add_circle_outline,
                              color: MapPalette.textMain, size: 18),
                          SizedBox(width: 8),
                          Text('FILIALE ÖFFNEN',
                              style: TextStyle(
                                  color: MapPalette.textMain,
                                  fontSize: 14,
                                  fontWeight: FontWeight.w700)),
                        ],
                      ),
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

  double _cityShare() {
    if (totalCityShops <= 0 || playerCityShops <= 0) return 0;
    final others = totalCityShops - playerCityShops;
    if (others <= 0) return 1.0;
    return playerCityShops / (playerCityShops + others);
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
        color: MapPalette.accent,
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
  const _StatTile({
    required this.label,
    required this.icon,
    required this.value,
    required this.unit,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 150,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: MapPalette.bgCard,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label,
              style: const TextStyle(
                  color: MapPalette.textMuted,
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.5)),
          const SizedBox(height: 8),
          Icon(icon, color: MapPalette.textDim, size: 20),
          const Spacer(),
          Text(value,
              style: const TextStyle(
                  color: MapPalette.textMain,
                  fontSize: 19,
                  fontWeight: FontWeight.w800)),
          Text(unit,
              style: const TextStyle(
                  color: MapPalette.textMuted, fontSize: 8.5)),
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
  final double value;
  final double cityShare;
  const _DonutTile({
    required this.value,
    required this.cityShare,
  });

  @override
  Widget build(BuildContext context) {
    final pct = (value * 100).round();
    final cityPct = (cityShare * 100).round();
    return Container(
      height: 150,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: MapPalette.bgCard,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('MARKTANTEIL',
              style: TextStyle(
                  color: MapPalette.textMuted,
                  fontSize: 9,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.5)),
          const Spacer(),
          Center(
            child: SizedBox(
              width: 74,
              height: 74,
              child: CustomPaint(
                painter: _DonutPainter(value),
                child: Center(
                  child: Text('$pct%',
                      style: const TextStyle(
                          color: MapPalette.textMain,
                          fontSize: 18,
                          fontWeight: FontWeight.w800)),
                ),
              ),
            ),
          ),
          const Spacer(),
          Center(
            child: Text('STADT: $cityPct%',
                style: const TextStyle(
                    color: MapPalette.textMuted, fontSize: 9)),
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
      ..color = MapPalette.accent
      ..style = PaintingStyle.stroke
      ..strokeWidth = 12
      ..strokeCap = StrokeCap.round;
    canvas.drawCircle(center, radius, track);
    canvas.drawArc(Rect.fromCircle(center: center, radius: radius), -1.5708,
        value.clamp(0.0, 1.0) * 6.2832, false, arc);
  }

  @override
  bool shouldRepaint(covariant _DonutPainter old) => old.value != value;
}

class _SparkPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    const pts = [0.2, 0.35, 0.25, 0.5, 0.42, 0.65, 0.6, 0.8, 0.95];
    final paint = Paint()
      ..color = MapPalette.accent
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
  final String shopId;
  final String cityId;

  const _BottomNav({
    required this.shopId,
    required this.cityId,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.only(top: 8, bottom: 10),
      color: MapPalette.bgDeep,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _NavItem(Icons.home_filled, 'ÜBERSICHT', active: true,
              onTap: () {}),
          _NavItem(Icons.location_on_outlined, 'FILIALEN',
              onTap: () {}),
          _NavItem(Icons.groups_outlined, 'MANAGER', onTap: () {}),
          _NavItem(Icons.science_outlined, 'FORSCHUNG',
              onTap: () {}),
          _NavItem(Icons.shopping_cart_outlined, 'SHOP',
              onTap: () {}),
        ],
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final String label;
  final bool active;
  final VoidCallback onTap;
  const _NavItem(this.icon, this.label,
      {this.active = false, required this.onTap});
  @override
  Widget build(BuildContext context) {
    final c = active ? MapPalette.accent : MapPalette.textDim;
    return GestureDetector(
      onTap: onTap,
      child: Column(
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
      ),
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
            color: MapPalette.bgPanel.withAlpha(235),
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: MapPalette.accent, width: 1.5),
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
                      color: MapPalette.textMain,
                      fontSize: 16,
                      fontWeight: FontWeight.w800,
                      letterSpacing: 0.3)),
              const SizedBox(width: 10),
              const Icon(Icons.star,
                  color: MapPalette.accent, size: 14),
              const SizedBox(width: 3),
              Text(rating.toStringAsFixed(1),
                  style: const TextStyle(
                      color: MapPalette.textMain,
                      fontSize: 14,
                      fontWeight: FontWeight.w700)),
            ],
          ),
        ),
        CustomPaint(
          size: const Size(16, 9),
          painter: _TailPainter(MapPalette.bgPanel.withAlpha(235)),
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
class _HeaderPill extends StatelessWidget {
  final String cash;
  final String day;
  final String weekday;
  const _HeaderPill({
    required this.cash,
    required this.day,
    required this.weekday,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
      decoration: BoxDecoration(
        color: MapPalette.bgPanel.withAlpha(235),
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: MapPalette.border),
      ),
      child: Row(
        children: [
          const Icon(Icons.payments_outlined,
              color: MapPalette.success, size: 22),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(cash,
                  style: const TextStyle(
                      color: MapPalette.textMain,
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              const Text('KONTOSTAND',
                  style: TextStyle(
                      color: MapPalette.textMuted,
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
                      color: MapPalette.textMain,
                      fontSize: 18,
                      fontWeight: FontWeight.w800)),
              Text(weekday,
                  style: const TextStyle(
                      color: MapPalette.textMuted,
                      fontSize: 9,
                      letterSpacing: 1.5)),
            ],
          ),
          const SizedBox(width: 6),
          const Icon(Icons.calendar_today_outlined,
              color: MapPalette.textMuted, size: 20),
        ],
      ),
    );
  }
}
