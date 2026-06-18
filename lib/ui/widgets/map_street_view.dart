import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../models/building_style_model.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/competitor_look.dart';
import '../../models/competitor_model.dart';
import '../../models/shop_model.dart';
import 'building_styles.dart';
import 'detail_panel_street.dart';
import 'street_building_painter.dart';

/// Art eines Gebäudes im Straßenzug.
enum StreetBuildingKind { player, freePlot, competitor, filler }

/// Ein einzelnes Gebäude im 2.5D-Straßenzug inkl. seiner Datenquelle.
class StreetBuilding {
  final StreetBuildingKind kind;
  final BuildingStyle style;
  final int seed;
  final Shop? shop;
  final Competitor? competitor;
  final CityMapLocation? location;

  const StreetBuilding({
    required this.kind,
    required this.style,
    required this.seed,
    this.shop,
    this.competitor,
    this.location,
  });
}

/// Zwei Strassenseiten: [near] (vorne, breiter) und [far] (hinten).
class StreetScene {
  final List<StreetBuilding> near;
  final List<StreetBuilding> far;
  const StreetScene({required this.near, required this.far});

  List<StreetBuilding> get all => [...near, ...far];
}

/// Erzeugt deterministisch die Gebäudereihe für einen Standort:
/// eigenes Gebäude (oder freier Bauplatz) zentral vorne, Konkurrenz links/
/// rechts, Füll-Gebäude dazwischen — insgesamt 6 Häuser.
StreetScene buildStreetScene({
  required CityMapLocation location,
  Shop? playerShop,
  required List<Competitor> competitors,
}) {
  final baseSeed = location.id.hashCode & 0x7fffffff;
  final comps = competitors.take(3).toList();

  final center = playerShop != null
      ? StreetBuilding(
          kind: StreetBuildingKind.player,
          style: playerBuildingStyle(playerShop),
          seed: baseSeed + 1,
          shop: playerShop,
        )
      : StreetBuilding(
          kind: StreetBuildingKind.freePlot,
          style: freePlotStyle(location),
          seed: baseSeed + 1,
          location: location,
        );

  StreetBuilding compOrFiller(int i, int seed) => i < comps.length
      ? StreetBuilding(
          kind: StreetBuildingKind.competitor,
          style: CompetitorLook.fromCompetitor(comps[i]).style,
          seed: seed,
          competitor: comps[i],
        )
      : StreetBuilding(
          kind: StreetBuildingKind.filler,
          style: fillerBuildingStyle(seed),
          seed: seed,
        );

  final near = <StreetBuilding>[
    compOrFiller(0, baseSeed + 11),
    center,
    compOrFiller(1, baseSeed + 23),
  ];
  final far = <StreetBuilding>[
    compOrFiller(2, baseSeed + 37),
    StreetBuilding(
        kind: StreetBuildingKind.filler,
        style: fillerBuildingStyle(baseSeed + 53),
        seed: baseSeed + 53),
    StreetBuilding(
        kind: StreetBuildingKind.filler,
        style: fillerBuildingStyle(baseSeed + 71),
        seed: baseSeed + 71),
  ];

  return StreetScene(near: near, far: far);
}

/// Ebene 3: 2.5D-Straßenzug mit individuellen Gebäuden.
class MapStreetView extends StatefulWidget {
  final CityData city;
  final CityMapLocation location;
  final Shop? playerShop;
  final List<Competitor> competitors;
  final double cash;
  final void Function(Shop) onManage;
  final void Function(Shop) onCustomize;
  final void Function(CityMapLocation) onOpenFree;
  final void Function(Competitor) onAcquire;

  const MapStreetView({
    super.key,
    required this.city,
    required this.location,
    required this.competitors,
    required this.cash,
    required this.onManage,
    required this.onCustomize,
    required this.onOpenFree,
    required this.onAcquire,
    this.playerShop,
  });

  @override
  State<MapStreetView> createState() => _MapStreetViewState();
}

class _MapStreetViewState extends State<MapStreetView> {
  int? _selected;

  @override
  Widget build(BuildContext context) {
    final scene = buildStreetScene(
      location: widget.location,
      playerShop: widget.playerShop,
      competitors: widget.competitors,
    );
    final all = scene.all;
    if (_selected != null && _selected! >= all.length) _selected = null;

    return Container(
      color: MapPalette.bgDeep,
      child: Column(
        children: [
          Expanded(
            child: LayoutBuilder(
              builder: (context, c) {
                final h = c.maxHeight;
                return Stack(
                  children: [
                    Positioned.fill(
                      child: CustomPaint(painter: _StreetGroundPainter()),
                    ),
                    // Hintere Strassenseite
                    Positioned(
                      top: h * 0.04,
                      left: 8,
                      right: 8,
                      height: h * 0.42,
                      child: _buildRow(scene.far, all, scaleDown: true),
                    ),
                    // Vordere Strassenseite (näher, größer)
                    Positioned(
                      bottom: 0,
                      left: 0,
                      right: 0,
                      height: h * 0.56,
                      child: _buildRow(scene.near, all, scaleDown: false),
                    ),
                  ],
                );
              },
            ),
          ),
          AnimatedSize(
            duration: const Duration(milliseconds: 220),
            curve: Curves.easeOutCubic,
            child: _selected != null
                ? _buildPanel(all[_selected!])
                : const SizedBox.shrink(),
          ),
        ],
      ),
    );
  }

  Widget _buildRow(List<StreetBuilding> row, List<StreetBuilding> all,
      {required bool scaleDown}) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        for (final b in row)
          Expanded(
            child: _BuildingTile(
              building: b,
              isSelected: _selected != null && all[_selected!] == b,
              onTap: () =>
                  setState(() => _selected = _selected == all.indexOf(b)
                      ? null
                      : all.indexOf(b)),
              scaleDown: scaleDown,
            ),
          ),
      ],
    );
  }

  Widget _buildPanel(StreetBuilding b) {
    return DetailPanelStreet(
      city: widget.city,
      cash: widget.cash,
      playerShop: b.kind == StreetBuildingKind.player ? b.shop : null,
      competitor: b.kind == StreetBuildingKind.competitor ? b.competitor : null,
      freeLocation: b.kind == StreetBuildingKind.freePlot ? b.location : null,
      isFiller: b.kind == StreetBuildingKind.filler,
      onClose: () => setState(() => _selected = null),
      onManage: (s) {
        setState(() => _selected = null);
        widget.onManage(s);
      },
      onCustomize: widget.onCustomize,
      onOpenFree: (l) {
        setState(() => _selected = null);
        widget.onOpenFree(l);
      },
      onAcquire: (c) {
        setState(() => _selected = null);
        widget.onAcquire(c);
      },
    );
  }
}

class _BuildingTile extends StatelessWidget {
  final StreetBuilding building;
  final bool isSelected;
  final VoidCallback onTap;
  final bool scaleDown;

  const _BuildingTile({
    required this.building,
    required this.isSelected,
    required this.onTap,
    required this.scaleDown,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onTap: onTap,
      child: Padding(
        padding: EdgeInsets.symmetric(
          horizontal: 2,
          vertical: scaleDown ? 4 : 0,
        ),
        child: SizedBox.expand(
          child: CustomPaint(
            painter: StreetBuildingPainter(
              style: building.style,
              isPlayer: building.kind == StreetBuildingKind.player,
              isSelected: isSelected,
              seed: building.seed,
            ),
          ),
        ),
      ),
    );
  }
}

// ── Strassen-Untergrund (Asphalt, Gehweg, Markierungen, Bäume, Laternen) ───
class _StreetGroundPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final w = size.width;
    final h = size.height;

    // Gehweg hinten
    canvas.drawRect(
      Rect.fromLTWH(0, h * 0.42, w, h * 0.10),
      Paint()..color = MapPalette.sidewalk,
    );
    // Asphalt-Strasse
    canvas.drawRect(
      Rect.fromLTWH(0, h * 0.52, w, h * 0.16),
      Paint()..color = MapPalette.asphalt,
    );
    // Gehweg vorne
    canvas.drawRect(
      Rect.fromLTWH(0, h * 0.68, w, h * 0.32),
      Paint()..color = MapPalette.sidewalk,
    );

    // Mittel-Markierung (gestrichelt)
    final marking = Paint()
      ..color = MapPalette.marking
      ..strokeWidth = 2;
    final y = h * 0.60;
    const dash = 16.0, gap = 12.0;
    for (double x = 0; x < w; x += dash + gap) {
      canvas.drawLine(Offset(x, y), Offset(math.min(x + dash, w), y), marking);
    }
    // Gehweg-Kanten
    final edge = Paint()
      ..color = MapPalette.marking.withAlpha(120)
      ..strokeWidth = 1.5;
    canvas.drawLine(Offset(0, h * 0.52), Offset(w, h * 0.52), edge);
    canvas.drawLine(Offset(0, h * 0.68), Offset(w, h * 0.68), edge);

    // Bäume an der hinteren Gehwegkante
    for (double x = w * 0.12; x < w; x += w * 0.26) {
      _tree(canvas, Offset(x, h * 0.50), 10);
    }
    // Strassenlaternen vorne (kleine Lichter)
    for (double x = w * 0.22; x < w; x += w * 0.30) {
      _lamp(canvas, Offset(x, h * 0.70), h * 0.06);
    }
  }

  void _tree(Canvas canvas, Offset base, double r) {
    canvas.drawRect(
      Rect.fromCenter(center: Offset(base.dx, base.dy), width: 2.5, height: r),
      Paint()..color = const Color(0xFF2A1E14),
    );
    canvas.drawCircle(
      Offset(base.dx, base.dy - r),
      r,
      Paint()..color = const Color(0xFF1E2A18),
    );
  }

  void _lamp(Canvas canvas, Offset base, double height) {
    canvas.drawLine(
      base,
      Offset(base.dx, base.dy - height),
      Paint()
        ..color = MapPalette.marking
        ..strokeWidth = 2,
    );
    canvas.drawCircle(
      Offset(base.dx, base.dy - height),
      3,
      Paint()..color = const Color(0xFFFFCB6B),
    );
    canvas.drawCircle(
      Offset(base.dx, base.dy - height),
      7,
      Paint()
        ..color = const Color(0xFFFFCB6B).withAlpha(60)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
    );
  }

  @override
  bool shouldRepaint(covariant _StreetGroundPainter oldDelegate) => false;
}
