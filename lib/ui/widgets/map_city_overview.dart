import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/competitor_model.dart';
import '../../models/shop_model.dart';
import '../../models/time_profile_model.dart';
import 'building_styles.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

/// Ebene 2: Vogelperspektive (schematischer Stadtplan, Standort-Auswahl).
class MapCityOverview extends StatefulWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> cityShops;
  final List<Competitor> competitors;
  final void Function(CityMapLocation) onEnterLocation;

  const MapCityOverview({
    super.key,
    required this.city,
    required this.locations,
    required this.cityShops,
    required this.competitors,
    required this.onEnterLocation,
  });

  @override
  State<MapCityOverview> createState() => _MapCityOverviewState();
}

class _MapCityOverviewState extends State<MapCityOverview> {
  int? _selected;

  Map<int, int> get _competitorsByLocation {
    final map = <int, int>{};
    if (widget.locations.isEmpty) return map;
    for (final c in widget.competitors) {
      final idx = (c.id.hashCode & 0x7fffffff) % widget.locations.length;
      map[idx] = (map[idx] ?? 0) + 1;
    }
    return map;
  }

  bool _isOwned(CityMapLocation loc) =>
      widget.cityShops.any((s) => s.locationName == loc.template.name);

  @override
  Widget build(BuildContext context) {
    final compByLoc = _competitorsByLocation;
    return Container(
      color: MapPalette.bgBase,
      child: Column(
        children: [
          Expanded(
            child: LayoutBuilder(
              builder: (context, c) {
                return Stack(
                  children: [
                    Positioned.fill(
                      child: CustomPaint(painter: _CityPlanPainter()),
                    ),
                    for (int i = 0; i < widget.locations.length; i++)
                      _buildArea(i, widget.locations[i], compByLoc[i] ?? 0,
                          c.maxWidth, c.maxHeight),
                  ],
                );
              },
            ),
          ),
          AnimatedSize(
            duration: const Duration(milliseconds: 220),
            curve: Curves.easeOutCubic,
            child: _selected != null
                ? _OverviewPanel(
                    location: widget.locations[_selected!],
                    city: widget.city,
                    owned: _isOwned(widget.locations[_selected!]),
                    rivalCount: compByLoc[_selected!] ?? 0,
                    onClose: () => setState(() => _selected = null),
                    onEnter: () =>
                        widget.onEnterLocation(widget.locations[_selected!]),
                  )
                : const SizedBox.shrink(),
          ),
        ],
      ),
    );
  }

  Widget _buildArea(int index, CityMapLocation loc, int rivals, double w,
      double h) {
    const cardW = 116.0;
    const cardH = 64.0;
    final pos = loc.mapPosition;
    var left = pos.dx * w - cardW / 2;
    var top = pos.dy * h - cardH / 2;
    left = left.clamp(4.0, (w - cardW - 4).clamp(4.0, w));
    top = top.clamp(4.0, (h - cardH - 4).clamp(4.0, h));

    final owned = _isOwned(loc);
    final isSel = _selected == index;
    final areaColor = _personalityColor(loc.personality);

    return Positioned(
      left: left,
      top: top,
      width: cardW,
      height: cardH,
      child: GestureDetector(
        onTap: () => setState(
            () => _selected = _selected == index ? null : index),
        child: Container(
          decoration: BoxDecoration(
            color: areaColor.withAlpha(isSel ? 230 : 180),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(
              color: isSel ? MapPalette.accent : MapPalette.border,
              width: isSel ? 2 : 1,
            ),
          ),
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Text(loc.icon, style: const TextStyle(fontSize: 14)),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      loc.label,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 11,
                        fontWeight: FontWeight.w700,
                        color: MapPalette.textMain,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              Row(
                children: [
                  if (owned)
                    _Marker(color: MapPalette.accent, label: 'Du'),
                  if (owned && rivals > 0) const SizedBox(width: 4),
                  if (rivals > 0)
                    _Marker(color: MapPalette.danger, label: '$rivals'),
                  if (!owned && rivals == 0)
                    _Marker(color: MapPalette.success, label: 'frei'),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Color _personalityColor(LocationPersonality p) {
    switch (p) {
      case LocationPersonality.business:
        return const Color(0xFF2A3340);
      case LocationPersonality.transit:
        return const Color(0xFF3A2E20);
      case LocationPersonality.residential:
        return const Color(0xFF243524);
      case LocationPersonality.university:
        return const Color(0xFF2E2A40);
      case LocationPersonality.nightlife:
        return const Color(0xFF3A2438);
      case LocationPersonality.touristic:
        return const Color(0xFF38321F);
    }
  }
}

class _Marker extends StatelessWidget {
  final Color color;
  final String label;
  const _Marker({required this.color, required this.label});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 1),
      decoration: BoxDecoration(
        color: color.withAlpha(60),
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: color, width: 1),
      ),
      child: Text(
        label,
        style: TextStyle(
            fontSize: 9, fontWeight: FontWeight.w700, color: color),
      ),
    );
  }
}

// ── Schematischer Stadtplan-Hintergrund ────────────────────────────────────
class _CityPlanPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    canvas.drawRect(Offset.zero & size, Paint()..color = MapPalette.bgBase);

    // Grünfläche (Park) oben links
    canvas.drawRRect(
      RRect.fromRectAndRadius(
        Rect.fromLTWH(size.width * 0.05, size.height * 0.06,
            size.width * 0.30, size.height * 0.24),
        const Radius.circular(14),
      ),
      Paint()..color = const Color(0xFF1C2A1A),
    );

    // Wasserfläche (Hafen/Fluss) unten
    canvas.drawRRect(
      RRect.fromRectAndRadius(
        Rect.fromLTWH(size.width * 0.0, size.height * 0.82,
            size.width, size.height * 0.18),
        const Radius.circular(4),
      ),
      Paint()..color = const Color(0xFF132430),
    );

    // Strassenraster
    final road = Paint()
      ..color = MapPalette.sidewalk
      ..strokeWidth = 10
      ..strokeCap = StrokeCap.round;
    final marking = Paint()
      ..color = MapPalette.marking
      ..strokeWidth = 1.5;

    final hs = [0.34, 0.62];
    final vs = [0.30, 0.58, 0.80];
    for (final f in hs) {
      final y = size.height * f;
      canvas.drawLine(Offset(0, y), Offset(size.width, y), road);
      _dashed(canvas, Offset(0, y), Offset(size.width, y), marking);
    }
    for (final f in vs) {
      final x = size.width * f;
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), road);
      _dashed(canvas, Offset(x, 0), Offset(x, size.height), marking);
    }
  }

  void _dashed(Canvas canvas, Offset a, Offset b, Paint p) {
    const dash = 8.0, gap = 7.0;
    final total = (b - a).distance;
    if (total <= 0) return;
    final dir = (b - a) / total;
    double d = 0;
    while (d < total) {
      final start = a + dir * d;
      final end = a + dir * (d + dash).clamp(0, total);
      canvas.drawLine(start, end, p);
      d += dash + gap;
    }
  }

  @override
  bool shouldRepaint(covariant _CityPlanPainter oldDelegate) => false;
}

// ── Kurzinfo-Panel für den ausgewählten Standort ───────────────────────────
class _OverviewPanel extends StatelessWidget {
  final CityMapLocation location;
  final CityData city;
  final bool owned;
  final int rivalCount;
  final VoidCallback onClose;
  final VoidCallback onEnter;

  const _OverviewPanel({
    required this.location,
    required this.city,
    required this.owned,
    required this.rivalCount,
    required this.onClose,
    required this.onEnter,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: MapPalette.bgPanel,
        borderRadius: BorderRadius.vertical(top: Radius.circular(18)),
        border: Border(top: BorderSide(color: MapPalette.border)),
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Text(location.icon, style: const TextStyle(fontSize: 22)),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          location.label,
                          style: const TextStyle(
                            fontFamily: 'Baloo2',
                            fontSize: 18,
                            fontWeight: FontWeight.w700,
                            color: MapPalette.textMain,
                          ),
                        ),
                        Text(
                          location.audience,
                          style: const TextStyle(
                              fontSize: 12, color: MapPalette.textMuted),
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    onPressed: onClose,
                    icon: const Icon(Icons.close,
                        color: MapPalette.textDim, size: 20),
                    padding: EdgeInsets.zero,
                    constraints: const BoxConstraints(),
                  ),
                ],
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  _Chip(
                    label: owned ? 'Deine Filiale' : 'Frei',
                    color: owned ? MapPalette.accent : MapPalette.success,
                  ),
                  const SizedBox(width: 8),
                  _Chip(
                    label: 'Miete ${_fmt.format(location.weeklyRentFor(city).round())} €/W',
                    color: MapPalette.textMuted,
                  ),
                  const SizedBox(width: 8),
                  if (rivalCount > 0)
                    _Chip(
                      label: '$rivalCount Rivale${rivalCount > 1 ? 'n' : ''}',
                      color: MapPalette.danger,
                    ),
                ],
              ),
              const SizedBox(height: 14),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton.icon(
                  onPressed: onEnter,
                  icon: const Icon(Icons.login_rounded, size: 16),
                  label: const Text('Standort betreten'),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: MapPalette.accent,
                    foregroundColor: Colors.black87,
                    padding: const EdgeInsets.symmetric(vertical: 12),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10)),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Chip extends StatelessWidget {
  final String label;
  final Color color;
  const _Chip({required this.label, required this.color});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withAlpha(40),
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: color.withAlpha(120)),
      ),
      child: Text(
        label,
        style: TextStyle(
            fontSize: 10, fontWeight: FontWeight.w600, color: color),
      ),
    );
  }
}
