import 'package:flutter/material.dart';

import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';
import 'iso_city_map_canvas.dart';
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
          child: IsoCityMapCanvas(
            scene: const Size(1200, 920),
            buildings: _buildingsFor(locs, shops, widget.selected),
            heroes: _heroesFor(locs, shops, widget.selected),
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

  // ── Iso-Projektion (identisch zu IsoMapPainter) ──────────────────────────
  static const Size _scene = Size(1200, 920);
  static const int _gridN = 6;

  int _col(CityMapLocation loc) =>
      (loc.mapPosition.dx * (_gridN - 1)).round().clamp(0, _gridN - 1);
  int _row(CityMapLocation loc) =>
      (loc.mapPosition.dy * (_gridN - 1)).round().clamp(0, _gridN - 1);

  Offset _mapOrigin(List<CityMapLocation> locs) => IsoMapPainter.originFor(
        [
          for (final loc in locs)
            IsoBuilding(tx: _col(loc), ty: _row(loc), floors: 3)
        ],
        _scene,
      );

  bool _isOwned(CityMapLocation loc, List<Shop> shops) => shops.any(
      (s) => s.cityId == widget.city.id && s.locationName == loc.template.name);

  bool _isHero(
          CityMapLocation loc, List<Shop> shops, CityMapLocation? selected) =>
      _isOwned(loc, shops) || selected?.id == loc.id;

  void _handleTap(Offset localPos, List<CityMapLocation> locs) {
    // localPos ist Viewport-Koordinate → in Szene-Koordinaten umrechnen, damit
    // Pan/Zoom des InteractiveViewer korrekt berücksichtigt werden.
    final scenePos = _transformCtrl.toScene(localPos);
    final origin = _mapOrigin(locs);

    CityMapLocation? closest;
    double minDist = 70;
    for (final loc in locs) {
      final pos = IsoMapPainter.projectTile(_col(loc), _row(loc), origin);
      final dist = (scenePos - pos).distance;
      if (dist < minDist) {
        minDist = dist;
        closest = loc;
      }
    }
    if (closest != null) widget.onSelect(closest);
  }

  List<IsoBuilding> _buildingsFor(
    List<CityMapLocation> locs,
    List<Shop> shops,
    CityMapLocation? selected,
  ) {
    final result = <IsoBuilding>[];
    for (int i = 0; i < locs.length; i++) {
      final loc = locs[i];
      final hero = _isHero(loc, shops, selected);
      result.add(IsoBuilding(
        tx: _col(loc),
        ty: _row(loc),
        floors: 3 + (i % 4),
        seed: 10 + i * 7,
        hero: hero,
        body: !hero, // Hero bekommt ein Sprite statt Vektor-Körper
      ));
    }
    return result;
  }

  List<HeroSprite> _heroesFor(
    List<CityMapLocation> locs,
    List<Shop> shops,
    CityMapLocation? selected,
  ) {
    final heroes = <HeroSprite>[];
    for (final loc in locs) {
      if (!_isHero(loc, shops, selected)) continue;
      heroes.add(HeroSprite(
        building: IsoBuilding(tx: _col(loc), ty: _row(loc), floors: 4),
        asset: _isOwned(loc, shops)
            ? 'assets/iso/building_owned.png'
            : 'assets/iso/building_empty.png',
        label: loc.label,
      ));
    }
    return heroes;
  }
}

// ── Floating-UI über der Karte (Mockup-Look) ───────────────────────────────

class _FloatingHeader extends StatelessWidget {
  final String cityName;
  final String cityEmoji;
  final double cash;
  final int day;
  final int shopCount;

  const _FloatingHeader({
    required this.cityName,
    required this.cityEmoji,
    required this.cash,
    required this.day,
    required this.shopCount,
  });

  static const _weekdays = [
    'MONTAG',
    'DIENSTAG',
    'MITTWOCH',
    'DONNERSTAG',
    'FREITAG',
    'SAMSTAG',
    'SONNTAG',
  ];

  static String _grouped(num v) {
    final s = v.round().abs().toString();
    final b = StringBuffer();
    for (var i = 0; i < s.length; i++) {
      if (i > 0 && (s.length - i) % 3 == 0) b.write('.');
      b.write(s[i]);
    }
    return b.toString();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Row(
          children: [
            Text(cityEmoji, style: const TextStyle(fontSize: 18)),
            const SizedBox(width: 6),
            Text(
              cityName,
              style: const TextStyle(
                color: Color(0xFFF3E9D6),
                fontSize: 16,
                fontWeight: FontWeight.w800,
              ),
            ),
            const Spacer(),
            Text(
              '$shopCount ${shopCount == 1 ? 'Filiale' : 'Filialen'}',
              style: const TextStyle(
                color: Color(0xFF8A8E96),
                fontSize: 11,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          decoration: BoxDecoration(
            color: const Color(0xFF121418).withAlpha(235),
            borderRadius: BorderRadius.circular(18),
            border: Border.all(color: const Color(0xFF22252B)),
          ),
          child: Row(
            children: [
              const Icon(Icons.payments_outlined,
                  color: Color(0xFF7FB069), size: 22),
              const SizedBox(width: 8),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('€ ${_grouped(cash)}',
                      style: const TextStyle(
                          color: Color(0xFFF5F3EF),
                          fontSize: 18,
                          fontWeight: FontWeight.w800)),
                  const Text('KONTOSTAND',
                      style: TextStyle(
                          color: Color(0xFF8A8E96),
                          fontSize: 9,
                          letterSpacing: 1.5)),
                ],
              ),
              const Spacer(),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text('Tag $day',
                      style: const TextStyle(
                          color: Color(0xFFF5F3EF),
                          fontSize: 18,
                          fontWeight: FontWeight.w800)),
                  Text(_weekdays[((day - 1) % 7 + 7) % 7],
                      style: const TextStyle(
                          color: Color(0xFF8A8E96),
                          fontSize: 9,
                          letterSpacing: 1.5)),
                ],
              ),
              const SizedBox(width: 6),
              const Icon(Icons.calendar_today_outlined,
                  color: Color(0xFF8A8E96), size: 20),
            ],
          ),
        ),
      ],
    );
  }
}

class _BottomBar extends StatelessWidget {
  final String selectedName;
  const _BottomBar({required this.selectedName});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: const Color(0xFF121418).withAlpha(235),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF22252B)),
      ),
      child: Row(
        children: [
          const Icon(Icons.place, color: Color(0xFFF5A623), size: 18),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              selectedName,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: Color(0xFFF3E9D6),
                fontSize: 14,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
          const Icon(Icons.chevron_right, color: Color(0xFF8A8E96), size: 20),
        ],
      ),
    );
  }
}
