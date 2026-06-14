import 'package:flutter/material.dart';

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';
import 'iso_city_map_painter.dart';

/// Coffee Inc 2-inspirierte 2.5D-Stadtkarte mit isometrischen Gebäuden,
/// Pins für eigene Filialen/Konkurrenz und Floating-UI.
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;
  final double cash;
  final int currentDay;

  const CityMapView({
    super.key,
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
    this.cash = 0,
    this.currentDay = 1,
  });

  @override
  Widget build(BuildContext context) {
    return _CoffeeMapLayout(
      city: city,
      locations: locations,
      shops: shops,
      selected: selected,
      onSelect: onSelect,
      cash: cash,
      currentDay: currentDay,
    );
  }
}

// ─── Layout (Stack: Karte + Floating UI) ──────────────────────────────────
class _CoffeeMapLayout extends StatefulWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;
  final double cash;
  final int currentDay;

  const _CoffeeMapLayout({
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
    this.cash = 0,
    this.currentDay = 1,
  });

  @override
  State<_CoffeeMapLayout> createState() => _CoffeeMapLayoutState();
}

class _CoffeeMapLayoutState extends State<_CoffeeMapLayout> {
  final TransformationController _transformCtrl = TransformationController();

  @override
  void dispose() {
    _transformCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final locs = widget.locations;
    final shops = widget.shops;

    return Stack(
      children: [
        // Layer 1: Stadtkarte (pan & zoom)
        InteractiveViewer(
          transformationController: _transformCtrl,
          minScale: 0.5,
          maxScale: 2.5,
          boundaryMargin: const EdgeInsets.all(80),
          constrained: false,
          child: SizedBox(
            width: 1200,
            height: 920,
            child: CustomPaint(
              painter: IsoMapPainter(
                buildings: _buildingsFor(locs, shops, widget.selected),
              ),
            ),
          ),
        ),

        // Layer 2: Floating Header
        Positioned(
          top: 8,
          left: 12,
          right: 12,
          child: _FloatingHeader(
            cityName: widget.city.name,
            cityEmoji: widget.city.emoji,
            cash: widget.cash,
            day: widget.currentDay,
            shopCount: shops.length,
          ),
        ),

        // Layer 3: Tap-Handler (da InteractiveViewer Tap blockiert)
        Positioned.fill(
          child: GestureDetector(
            behavior: HitTestBehavior.translucent,
            onTapUp: (details) {
              _handleTap(details.localPosition, locs);
            },
          ),
        ),

        // Layer 4: Bottom Controls
        Positioned(
          bottom: 4,
          left: 12,
          right: 12,
          child: _BottomBar(
            selectedName: widget.selected?.label ?? 'Standort wählen',
          ),
        ),
      ],
    );
  }

  void _handleTap(Offset localPos, List<CityMapLocation> locs) {
    const gridN = 6;
    const tileW = 132.0;
    const tileH = 76.0;
    const sceneW = 1200.0;
    // sceneH = 920.0 (for future use)
    const originX = sceneW / 2;
    const originY = 180.0;

    // Inverse isometric: find closest location
    CityMapLocation? closest;
    double minDist = 40;

    for (final loc in locs) {
      final c = (loc.mapPosition.dx * (gridN - 1)).round().clamp(0, gridN - 1);
      final r = (loc.mapPosition.dy * (gridN - 1)).round().clamp(0, gridN - 1);
      final isoX = originX + (c - r) * (tileW / 2);
      final isoY = originY + (c + r) * (tileH / 2);
      final dist = (localPos - Offset(isoX, isoY)).distance;
      if (dist < minDist) {
        minDist = dist;
        closest = loc;
      }
    }

    if (closest != null) {
      widget.onSelect(closest);
    }
  }
  List<IsoBuilding> _buildingsFor(
    List<CityMapLocation> locs,
    List<Shop> shops,
    CityMapLocation? selected,
  ) {
    final cityId = widget.city.id;
    const gridN = 6;
    final result = <IsoBuilding>[];
    for (int i = 0; i < locs.length; i++) {
      final loc = locs[i];
      final c = (loc.mapPosition.dx * (gridN - 1)).round().clamp(0, gridN - 1);
      final r = (loc.mapPosition.dy * (gridN - 1)).round().clamp(0, gridN - 1);
      final hasOwn = shops.any(
          (s) => s.cityId == cityId && s.locationName == loc.template.name);
      final isSelected = selected?.id == loc.id;
      result.add(IsoBuilding(
        tx: c,
        ty: r,
        floors: 3 + (i % 4),
        seed: 10 + i * 7,
        hero: hasOwn || isSelected,
      ));
    }
    return result;
  }
}
