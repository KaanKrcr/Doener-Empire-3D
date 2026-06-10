import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Isometrische 2.5D-City-Map. Öffentliche API unverändert (city/locations/
/// shops/selected/onSelect) — die Spiellogik bleibt unangetastet; nur die
/// Render-Schicht ist pseudo-3D (Iso-Grid + gestapelte Gebäude + Schatten +
/// Tiefensortierung + Pan/Zoom).
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;

  const CityMapView({
    super.key,
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
  });

  // ── Iso-Geometrie ──────────────────────────────────────────────────────────
  static const int _gridN = 6; // 6×6 Tile-Grundfläche
  static const double _tileW = 132;
  static const double _tileH = 76;
  // Logische Szenengröße (wird per InteractiveViewer skaliert/verschoben).
  static const double _sceneW = 980;
  static const double _sceneH = 760;
  static const double _originX = _sceneW / 2;
  static const double _originY = 150;

  static Offset _iso(double col, double row) => Offset(
        _originX + (col - row) * (_tileW / 2),
        _originY + (col + row) * (_tileH / 2),
      );

  /// Tile-Koordinate einer Location aus ihrer normalisierten mapPosition.
  static _Tile _tileFor(CityMapLocation loc) {
    final c = (loc.mapPosition.dx * (_gridN - 1)).round().clamp(0, _gridN - 1);
    final r = (loc.mapPosition.dy * (_gridN - 1)).round().clamp(0, _gridN - 1);
    return _Tile(c, r);
  }

  @override
  Widget build(BuildContext context) {
    // Belegte Tiles (für Dekor-Aussparung) + stabile Index-basierte Entzerrung,
    // damit zwei Locations nicht auf demselben Tile landen.
    final placed = <_Tile, CityMapLocation>{};
    for (final loc in locations) {
      var t = _tileFor(loc);
      var guard = 0;
      while (placed.containsKey(t) && guard < _gridN * _gridN) {
        t = _Tile((t.col + 1) % _gridN, t.col + 1 >= _gridN ? (t.row + 1) % _gridN : t.row);
        guard++;
      }
      placed[t] = loc;
    }

    // Tiefensortierung: hintere Tiles (kleines col+row) zuerst → vorne oben.
    final ordered = placed.entries.toList()
      ..sort((a, b) => (a.key.col + a.key.row).compareTo(b.key.col + b.key.row));

    return AspectRatio(
      aspectRatio: 1.15,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(28),
        child: ColoredBox(
          color: const Color(0xFF0E1A14),
          child: Stack(
            children: [
              InteractiveViewer(
                minScale: 0.6,
                maxScale: 2.4,
                boundaryMargin: const EdgeInsets.all(220),
                constrained: false,
                child: SizedBox(
                  width: _sceneW,
                  height: _sceneH,
                  child: Stack(
                    clipBehavior: Clip.none,
                    children: [
                      // Boden, Straßen, Dekor-Gebäude, Licht
                      Positioned.fill(
                        child: CustomPaint(
                          painter: _IsoGroundPainter(
                            occupied: placed.keys.toSet(),
                          ),
                        ),
                      ),
                      // Interaktive Hotspot-Gebäude (tiefensortiert)
                      for (final entry in ordered)
                        _buildHotspot(entry.key, entry.value),
                    ],
                  ),
                ),
              ),
              Positioned(left: 16, top: 16, child: _CityBadge(city: city)),
              const Positioned(
                right: 14,
                bottom: 12,
                child: _HintChip(text: '↔ ziehen · ⊕ zoomen'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildHotspot(_Tile tile, CityMapLocation location) {
    final base = _iso(tile.col.toDouble(), tile.row.toDouble());
    final owned = shops
        .where((s) => s.cityId == city.id && s.locationName == location.template.name)
        .length;
    final isSelected = selected?.id == location.id;
    final score = location.attractivenessScore(city).round();

    // Gebäudehöhe: leer = niedrig, im Besitz wächst sie mit Filialzahl.
    final height = 46.0 + (owned > 0 ? 26.0 + math.min(owned, 4) * 14.0 : 0.0);
    const footprint = 96.0;

    final color = owned > 0
        ? AppColors.accent
        : isSelected
            ? AppColors.gold
            : AppColors.primary;

    // Positionierung: Gebäude-Footprint zentriert auf dem Tile, Höhe nach oben.
    final left = base.dx - footprint / 2;
    final top = base.dy - height - footprint * 0.30;

    return Positioned(
      left: left,
      top: top,
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: () => onSelect(location),
        child: AnimatedScale(
          scale: isSelected ? 1.06 : 1.0,
          duration: const Duration(milliseconds: 160),
          curve: Curves.easeOutBack,
          child: SizedBox(
            width: footprint,
            height: height + footprint * 0.6 + 38,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                _Label(
                  icon: location.icon,
                  text: owned > 0 ? '×$owned' : '$score',
                  color: color,
                  highlight: isSelected,
                ),
                const SizedBox(height: 3),
                CustomPaint(
                  size: Size(footprint, height + footprint * 0.5),
                  painter: _IsoBuildingPainter(
                    color: color,
                    height: height,
                    footprint: footprint,
                    lit: isSelected,
                    windows: owned > 0,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _Tile {
  final int col;
  final int row;
  const _Tile(this.col, this.row);
  @override
  bool operator ==(Object other) =>
      other is _Tile && other.col == col && other.row == row;
  @override
  int get hashCode => col * 31 + row;
}

// ── Schwebende Beschriftung über dem Gebäude ──────────────────────────────────
class _Label extends StatelessWidget {
  final String icon;
  final String text;
  final Color color;
  final bool highlight;
  const _Label({
    required this.icon,
    required this.text,
    required this.color,
    required this.highlight,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.bgCard.withAlpha(238),
        borderRadius: BorderRadius.circular(13),
        border: Border.all(color: color, width: highlight ? 2 : 1),
        boxShadow: [
          BoxShadow(
            color: color.withAlpha(70),
            blurRadius: 14,
            offset: const Offset(0, 6),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(icon, style: const TextStyle(fontSize: 16)),
          const SizedBox(width: 4),
          Text(
            text,
            style: TextStyle(color: color, fontSize: 11, fontWeight: FontWeight.w800),
          ),
        ],
      ),
    );
  }
}

class _HintChip extends StatelessWidget {
  final String text;
  const _HintChip({required this.text});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: AppColors.bg.withAlpha(160),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        text,
        style: const TextStyle(color: AppColors.textMuted, fontSize: 10),
      ),
    );
  }
}

class _CityBadge extends StatelessWidget {
  final CityData city;
  const _CityBadge({required this.city});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 9),
      decoration: BoxDecoration(
        color: AppColors.bg.withAlpha(220),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.borderLight),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(city.emoji, style: const TextStyle(fontSize: 22)),
          const SizedBox(width: 8),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(city.name, style: AppText.display(size: 16, weight: FontWeight.w800)),
              Text(
                city.tier.label,
                style: const TextStyle(color: AppColors.textSecondary, fontSize: 11),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

// ── Ein isometrisches Gebäude (Dach + zwei beleuchtete Wände + Bodenschatten) ──
class _IsoBuildingPainter extends CustomPainter {
  final Color color;
  final double height;
  final double footprint;
  final bool lit;
  final bool windows;

  _IsoBuildingPainter({
    required this.color,
    required this.height,
    required this.footprint,
    required this.lit,
    required this.windows,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final w = footprint;
    final th = w * 0.5; // Tile-Diamant-Höhe
    final cx = size.width / 2;
    final baseY = size.height - th / 2; // Mittelpunkt der Bodenraute

    // Bodenschatten (gestauchte, weiche Ellipse)
    final shadow = Paint()
      ..color = Colors.black.withAlpha(95)
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);
    canvas.drawOval(
      Rect.fromCenter(center: Offset(cx + 8, baseY + 4), width: w * 0.9, height: th * 0.7),
      shadow,
    );

    // Eckpunkte der Bodenraute
    final top = Offset(cx, baseY - th / 2);
    final right = Offset(cx + w / 2, baseY);
    final bottom = Offset(cx, baseY + th / 2);
    final left = Offset(cx - w / 2, baseY);

    final hh = height;
    Offset up(Offset p) => Offset(p.dx, p.dy - hh);

    // Wandfarben: linke Wand dunkler, rechte heller (Lichteinfall von rechts).
    final hsl = HSLColor.fromColor(color);
    final roofPaint = Paint()..color = hsl.withLightness((hsl.lightness * 1.18).clamp(0, 1)).toColor();
    final leftWall = Paint()..color = hsl.withLightness((hsl.lightness * 0.62).clamp(0, 1)).toColor();
    final rightWall = Paint()..color = hsl.withLightness((hsl.lightness * 0.85).clamp(0, 1)).toColor();

    // Linke Wand (left→bottom)
    canvas.drawPath(
      Path()
        ..moveTo(left.dx, left.dy)
        ..lineTo(bottom.dx, bottom.dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..lineTo(up(left).dx, up(left).dy)
        ..close(),
      leftWall,
    );
    // Rechte Wand (bottom→right)
    canvas.drawPath(
      Path()
        ..moveTo(bottom.dx, bottom.dy)
        ..lineTo(right.dx, right.dy)
        ..lineTo(up(right).dx, up(right).dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..close(),
      rightWall,
    );
    // Dach
    canvas.drawPath(
      Path()
        ..moveTo(up(top).dx, up(top).dy)
        ..lineTo(up(right).dx, up(right).dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..lineTo(up(left).dx, up(left).dy)
        ..close(),
      roofPaint,
    );

    // Fenster (kleine helle Punkte auf der rechten Wand, nur bei eigenen Filialen)
    if (windows) {
      final winPaint = Paint()..color = AppColors.gold.withAlpha(210);
      for (var i = 0; i < 3; i++) {
        final t = 0.25 + i * 0.22;
        final wx = bottom.dx + (right.dx - bottom.dx) * 0.5;
        final wy = bottom.dy + (right.dy - bottom.dy) * 0.5 - hh * t;
        canvas.drawRect(
          Rect.fromCenter(center: Offset(wx, wy), width: 6, height: 8),
          winPaint,
        );
      }
    }

    // Auswahl-Glow
    if (lit) {
      final glow = Paint()
        ..color = color.withAlpha(60)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 16);
      canvas.drawCircle(up(top).translate(0, hh * 0.4), w * 0.5, glow);
    }
  }

  @override
  bool shouldRepaint(covariant _IsoBuildingPainter old) =>
      old.color != color || old.height != height || old.lit != lit || old.windows != windows;
}

// ── Iso-Boden: Tile-Grid, Straßen, Dekor-Gebäude, Ambient-Licht ───────────────
class _IsoGroundPainter extends CustomPainter {
  final Set<_Tile> occupied;
  _IsoGroundPainter({required this.occupied});

  static const int n = CityMapView._gridN;

  Offset _iso(double col, double row) => CityMapView._iso(col, row);

  @override
  void paint(Canvas canvas, Size size) {
    // Himmel/Hintergrund-Verlauf
    final bg = Paint()
      ..shader = const LinearGradient(
        colors: [Color(0xFF13241B), Color(0xFF1E1812), Color(0xFF2C1F13)],
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
      ).createShader(Offset.zero & size);
    canvas.drawRect(Offset.zero & size, bg);

    // Boden-Tiles (Schachbrett-Schattierung)
    final tileA = Paint()..color = const Color(0xFF24351F);
    final tileB = Paint()..color = const Color(0xFF1E2D1B);
    final edge = Paint()
      ..color = Colors.black.withAlpha(40)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1;
    for (var c = 0; c < n; c++) {
      for (var r = 0; r < n; r++) {
        final center = _iso(c.toDouble(), r.toDouble());
        final path = _diamond(center, CityMapView._tileW, CityMapView._tileH);
        canvas.drawPath(path, (c + r).isEven ? tileA : tileB);
        canvas.drawPath(path, edge);
      }
    }

    // Straßen entlang zweier Tile-Achsen
    _road(canvas, _iso(0, 2.5), _iso(n - 1, 2.5));
    _road(canvas, _iso(3, 0), _iso(3, n - 1));

    // Dekor-Gebäude auf freien Tiles (nicht interaktiv)
    final deco = Paint()..color = const Color(0xFF3A4A33);
    final decoRoof = Paint()..color = const Color(0xFF4A5C40);
    var seed = 7;
    for (var c = 0; c < n; c++) {
      for (var r = 0; r < n; r++) {
        if (occupied.contains(_Tile(c, r))) continue;
        seed = (seed * 1103515245 + 12345) & 0x7fffffff;
        if (seed % 100 < 42) continue; // ~42% Tiles bleiben leer
        final center = _iso(c.toDouble(), r.toDouble());
        final h = 18.0 + (seed % 30);
        _decoBuilding(canvas, center, CityMapView._tileW * 0.62, h, deco, decoRoof);
      }
    }

    // Ambientes Warm-Licht oben rechts
    final glow = Paint()
      ..color = AppColors.primary.withAlpha(26)
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 40);
    canvas.drawCircle(Offset(size.width * 0.7, size.height * 0.2), 160, glow);
  }

  Path _diamond(Offset c, double w, double h) => Path()
    ..moveTo(c.dx, c.dy - h / 2)
    ..lineTo(c.dx + w / 2, c.dy)
    ..lineTo(c.dx, c.dy + h / 2)
    ..lineTo(c.dx - w / 2, c.dy)
    ..close();

  void _road(Canvas canvas, Offset a, Offset b) {
    final road = Paint()
      ..color = const Color(0xFF5A4634)
      ..strokeWidth = 22
      ..strokeCap = StrokeCap.round;
    final line = Paint()
      ..color = AppColors.cream.withAlpha(120)
      ..strokeWidth = 2
      ..strokeCap = StrokeCap.round;
    canvas.drawLine(a, b, road);
    canvas.drawLine(a, b, line);
  }

  void _decoBuilding(Canvas canvas, Offset center, double w, double h, Paint wall, Paint roof) {
    final th = w * 0.5;
    final top = Offset(center.dx, center.dy - th / 2);
    final right = Offset(center.dx + w / 2, center.dy);
    final bottom = Offset(center.dx, center.dy + th / 2);
    final left = Offset(center.dx - w / 2, center.dy);
    Offset up(Offset p) => Offset(p.dx, p.dy - h);

    canvas.drawPath(
      Path()
        ..moveTo(left.dx, left.dy)
        ..lineTo(bottom.dx, bottom.dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..lineTo(up(left).dx, up(left).dy)
        ..close(),
      Paint()..color = (wall.color).withAlpha(200),
    );
    canvas.drawPath(
      Path()
        ..moveTo(bottom.dx, bottom.dy)
        ..lineTo(right.dx, right.dy)
        ..lineTo(up(right).dx, up(right).dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..close(),
      wall,
    );
    canvas.drawPath(
      Path()
        ..moveTo(up(top).dx, up(top).dy)
        ..lineTo(up(right).dx, up(right).dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..lineTo(up(left).dx, up(left).dy)
        ..close(),
      roof,
    );
  }

  @override
  bool shouldRepaint(covariant _IsoGroundPainter old) => old.occupied != occupied;
}
