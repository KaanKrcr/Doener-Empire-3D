import 'package:flutter/material.dart';
import 'package:intl/intl.dart' hide TextDirection;

import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/competitor_model.dart';
import '../../models/shop_model.dart';
import '../../models/time_profile_model.dart';
import 'building_styles.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

enum _DistrictStatus { owned, rival, free }

/// Ein organisch geformtes Stadtgebiet (kein Grid).
class _District {
  final int index;
  final Path path;
  final Offset centroid;
  final CityMapLocation location;
  final _DistrictStatus status;
  final int rivalCount;

  const _District({
    required this.index,
    required this.path,
    required this.centroid,
    required this.location,
    required this.status,
    required this.rivalCount,
  });
}

/// Unregelmässige Stadtgebiete (normalisiert) — bewusst keine Tabelle.
const List<List<Offset>> _districtShapes = [
  [
    Offset(0.10, 0.12), Offset(0.30, 0.08), Offset(0.40, 0.20),
    Offset(0.34, 0.34), Offset(0.16, 0.34), Offset(0.07, 0.24),
  ],
  [
    Offset(0.60, 0.09), Offset(0.82, 0.06), Offset(0.93, 0.18),
    Offset(0.88, 0.30), Offset(0.68, 0.32), Offset(0.58, 0.20),
  ],
  [
    Offset(0.39, 0.40), Offset(0.60, 0.39), Offset(0.67, 0.52),
    Offset(0.58, 0.63), Offset(0.41, 0.62), Offset(0.34, 0.50),
  ],
  [
    Offset(0.66, 0.47), Offset(0.90, 0.45), Offset(0.96, 0.60),
    Offset(0.88, 0.71), Offset(0.70, 0.69), Offset(0.63, 0.56),
  ],
  [
    Offset(0.17, 0.58), Offset(0.37, 0.57), Offset(0.43, 0.70),
    Offset(0.34, 0.83), Offset(0.16, 0.82), Offset(0.10, 0.69),
  ],
  [
    Offset(0.46, 0.70), Offset(0.65, 0.70), Offset(0.71, 0.82),
    Offset(0.60, 0.93), Offset(0.45, 0.91), Offset(0.39, 0.80),
  ],
];

/// Strassennetz (normalisierte Polylinien) — verläuft zwischen den Gebieten.
const List<List<Offset>> _roads = [
  [Offset(0.0, 0.44), Offset(0.30, 0.42), Offset(0.55, 0.47), Offset(0.80, 0.42), Offset(1.0, 0.45)],
  [Offset(0.50, 0.0), Offset(0.48, 0.30), Offset(0.53, 0.55), Offset(0.50, 0.84), Offset(0.52, 1.0)],
  [Offset(0.0, 0.67), Offset(0.34, 0.63), Offset(0.66, 0.67), Offset(1.0, 0.61)],
];

/// Ebene 2: Vogelperspektive — organischer Stadtplan, Standort-Auswahl.
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

  Map<int, int> _competitorsByLocation(int count) {
    final map = <int, int>{};
    if (count == 0) return map;
    for (final c in widget.competitors) {
      final idx = (c.id.hashCode & 0x7fffffff) % count;
      map[idx] = (map[idx] ?? 0) + 1;
    }
    return map;
  }

  List<_District> _buildDistricts(Size size) {
    final count = widget.locations.length.clamp(0, _districtShapes.length);
    final rivals = _competitorsByLocation(count);
    final result = <_District>[];
    for (int i = 0; i < count; i++) {
      final loc = widget.locations[i];
      final shape = _districtShapes[i];
      final path = Path();
      var cx = 0.0, cy = 0.0;
      for (int p = 0; p < shape.length; p++) {
        final pt = Offset(shape[p].dx * size.width, shape[p].dy * size.height);
        if (p == 0) {
          path.moveTo(pt.dx, pt.dy);
        } else {
          path.lineTo(pt.dx, pt.dy);
        }
        cx += pt.dx;
        cy += pt.dy;
      }
      path.close();
      final centroid = Offset(cx / shape.length, cy / shape.length);

      final owned =
          widget.cityShops.any((s) => s.locationName == loc.template.name);
      final rivalCount = rivals[i] ?? 0;
      final status = owned
          ? _DistrictStatus.owned
          : (rivalCount > 0 ? _DistrictStatus.rival : _DistrictStatus.free);

      result.add(_District(
        index: i,
        path: path,
        centroid: centroid,
        location: loc,
        status: status,
        rivalCount: rivalCount,
      ));
    }
    return result;
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Expanded(
          child: LayoutBuilder(
            builder: (context, c) {
              final size = Size(c.maxWidth, c.maxHeight);
              final districts = _buildDistricts(size);
              return GestureDetector(
                behavior: HitTestBehavior.opaque,
                onTapUp: (d) {
                  for (final dist in districts) {
                    if (dist.path.contains(d.localPosition)) {
                      setState(() => _selected =
                          _selected == dist.index ? null : dist.index);
                      return;
                    }
                  }
                  setState(() => _selected = null);
                },
                child: CustomPaint(
                  size: size,
                  painter: _OrganicCityPainter(
                    districts: districts,
                    selected: _selected,
                  ),
                ),
              );
            },
          ),
        ),
        AnimatedSize(
          duration: const Duration(milliseconds: 220),
          curve: Curves.easeOutCubic,
          child: _selected != null && _selected! < widget.locations.length
              ? _OverviewPanel(
                  location: widget.locations[_selected!],
                  city: widget.city,
                  owned: widget.cityShops.any((s) =>
                      s.locationName ==
                      widget.locations[_selected!].template.name),
                  rivalCount: _competitorsByLocation(
                          widget.locations.length.clamp(0, _districtShapes.length))[
                      _selected!] ??
                      0,
                  onClose: () => setState(() => _selected = null),
                  onEnter: () =>
                      widget.onEnterLocation(widget.locations[_selected!]),
                )
              : const SizedBox.shrink(),
        ),
      ],
    );
  }
}

// ── Painter: organischer Stadtplan ─────────────────────────────────────────
class _OrganicCityPainter extends CustomPainter {
  final List<_District> districts;
  final int? selected;

  _OrganicCityPainter({required this.districts, required this.selected});

  @override
  void paint(Canvas canvas, Size size) {
    final w = size.width;
    final h = size.height;

    // Hintergrund-Verlauf
    canvas.drawRect(
      Offset.zero & size,
      Paint()
        ..shader = const RadialGradient(
          center: Alignment.topCenter,
          radius: 1.2,
          colors: [Color(0xFF0C0E11), Color(0xFF07080A)],
        ).createShader(Offset.zero & size),
    );

    _park(canvas, w, h);
    _water(canvas, w, h);
    _drawRoads(canvas, w, h);

    for (final d in districts) {
      _drawDistrict(canvas, d);
    }
  }

  void _park(Canvas canvas, double w, double h) {
    final park = _scaledPath(const [
      Offset(0.41, 0.05), Offset(0.57, 0.05), Offset(0.59, 0.19),
      Offset(0.47, 0.24), Offset(0.39, 0.16),
    ], w, h);
    canvas.drawPath(park, Paint()..color = const Color(0xFF14201A));
    final tree = Paint()..color = const Color(0xFF24371F);
    for (final t in const [
      Offset(0.45, 0.10), Offset(0.51, 0.09), Offset(0.55, 0.14),
      Offset(0.48, 0.16), Offset(0.43, 0.15),
    ]) {
      canvas.drawCircle(Offset(t.dx * w, t.dy * h), 5, tree);
    }
  }

  void _water(Canvas canvas, double w, double h) {
    final water = _scaledPath(const [
      Offset(0.0, 0.80), Offset(0.18, 0.84), Offset(0.13, 1.0), Offset(0.0, 1.0),
    ], w, h);
    canvas.drawPath(water, Paint()..color = const Color(0xFF0B1620));
    final reflex = Paint()
      ..color = const Color(0xFF13283A)
      ..strokeWidth = 2;
    for (final f in const [0.88, 0.93, 0.98]) {
      canvas.drawLine(
        Offset(0.02 * w, f * h),
        Offset(0.12 * w, f * h),
        reflex,
      );
    }
  }

  void _drawRoads(Canvas canvas, double w, double h) {
    final asphalt = Paint()
      ..color = const Color(0xFF0A0B0D)
      ..style = PaintingStyle.stroke
      ..strokeWidth = (w * 0.05).clamp(10.0, 26.0)
      ..strokeJoin = StrokeJoin.round
      ..strokeCap = StrokeCap.round;
    final curb = Paint()
      ..color = const Color(0xFF16181C)
      ..style = PaintingStyle.stroke
      ..strokeWidth = (w * 0.05).clamp(10.0, 26.0) + 3
      ..strokeJoin = StrokeJoin.round
      ..strokeCap = StrokeCap.round;

    for (final road in _roads) {
      final pts = road.map((p) => Offset(p.dx * w, p.dy * h)).toList();
      final path = Path()..moveTo(pts.first.dx, pts.first.dy);
      for (int i = 1; i < pts.length; i++) {
        path.lineTo(pts[i].dx, pts[i].dy);
      }
      canvas.drawPath(path, curb);
      canvas.drawPath(path, asphalt);
      _dashedPolyline(canvas, pts);
    }
  }

  void _dashedPolyline(Canvas canvas, List<Offset> pts) {
    final marking = Paint()
      ..color = const Color(0xFF3A3D44)
      ..strokeWidth = 2;
    const dash = 12.0, gap = 16.0;
    for (int i = 1; i < pts.length; i++) {
      final a = pts[i - 1];
      final b = pts[i];
      final total = (b - a).distance;
      if (total <= 0) continue;
      final dir = (b - a) / total;
      var d = 0.0;
      while (d < total) {
        final s = a + dir * d;
        final e = a + dir * (d + dash > total ? total : d + dash);
        canvas.drawLine(s, e, marking);
        d += dash + gap;
      }
    }
  }

  void _drawDistrict(Canvas canvas, _District d) {
    final isSel = selected == d.index;
    canvas.drawPath(
      d.path,
      Paint()..color = _areaColor(d.location.personality).withAlpha(isSel ? 235 : 200),
    );
    canvas.drawPath(
      d.path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = isSel ? 2.5 : 1.2
        ..color = isSel ? const Color(0xFFF5A623) : const Color(0xFF2A2E36),
    );

    // Bezirksname + Kurzbeschreibung
    _text(canvas, d.location.label, d.centroid.translate(0, -2),
        const Color(0xFFFFFAE6), 15, FontWeight.w700);
    _text(canvas, _shortAudience(d.location.audience),
        d.centroid.translate(0, 16), const Color(0xFFC4B5A0), 10,
        FontWeight.w500);

    // Marker-Pin oberhalb des Namens
    _pin(canvas, d.centroid.translate(0, -26), d.status);
  }

  void _pin(Canvas canvas, Offset center, _DistrictStatus status) {
    final (color, emoji) = switch (status) {
      _DistrictStatus.owned => (const Color(0xFFF5A623), '🥙'),
      _DistrictStatus.rival => (const Color(0xFFE74C3C), '👤'),
      _DistrictStatus.free => (const Color(0xFF7BC950), '📋'),
    };
    if (status == _DistrictStatus.owned) {
      canvas.drawCircle(
        center,
        13,
        Paint()
          ..color = color.withAlpha(110)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
      );
    }
    canvas.drawCircle(center, 10, Paint()..color = const Color(0xFF121418));
    canvas.drawCircle(
      center,
      10,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2
        ..color = color,
    );
    _text(canvas, emoji, center.translate(0, 1), const Color(0xFFFFFFFF), 11,
        FontWeight.w400);
  }

  void _text(Canvas canvas, String s, Offset center, Color color, double size,
      FontWeight weight) {
    final tp = TextPainter(
      text: TextSpan(
        text: s,
        style: TextStyle(
          color: color,
          fontSize: size,
          fontWeight: weight,
          fontFamily: 'Baloo2',
        ),
      ),
      textAlign: TextAlign.center,
      textDirection: TextDirection.ltr,
      maxLines: 1,
      ellipsis: '…',
    )..layout(maxWidth: 130);
    tp.paint(canvas, center.translate(-tp.width / 2, -tp.height / 2));
  }

  Path _scaledPath(List<Offset> norm, double w, double h) {
    final path = Path();
    for (int i = 0; i < norm.length; i++) {
      final pt = Offset(norm[i].dx * w, norm[i].dy * h);
      if (i == 0) {
        path.moveTo(pt.dx, pt.dy);
      } else {
        path.lineTo(pt.dx, pt.dy);
      }
    }
    path.close();
    return path;
  }

  String _shortAudience(String a) => a.length <= 22 ? a : '${a.substring(0, 21)}…';

  Color _areaColor(LocationPersonality p) {
    switch (p) {
      case LocationPersonality.business:
        return const Color(0xFF222A33);
      case LocationPersonality.transit:
        return const Color(0xFF2E2519);
      case LocationPersonality.residential:
        return const Color(0xFF1E2A1D);
      case LocationPersonality.university:
        return const Color(0xFF272338);
      case LocationPersonality.nightlife:
        return const Color(0xFF2E1D2C);
      case LocationPersonality.touristic:
        return const Color(0xFF2C2718);
    }
  }

  @override
  bool shouldRepaint(covariant _OrganicCityPainter old) =>
      old.selected != selected || old.districts != districts;
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
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: [
                  _Chip(
                    label: owned ? 'Deine Filiale' : 'Frei',
                    color: owned ? MapPalette.accent : MapPalette.success,
                  ),
                  _Chip(
                    label:
                        'Miete ${_fmt.format(location.weeklyRentFor(city).round())} €/W',
                    color: MapPalette.textMuted,
                  ),
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
