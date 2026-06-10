import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/services.dart' show rootBundle;

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Isometrische 2.5D-City-Map. Öffentliche API unverändert (city/locations/
/// shops/selected/onSelect) — die Spiellogik bleibt unangetastet; nur die
/// Render-Schicht ist pseudo-3D (Iso-Grid + gestapelte Gebäude + Schatten +
/// Tiefensortierung + Pan/Zoom).
class CityMapView extends StatefulWidget {
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
  State<CityMapView> createState() => _CityMapViewState();
}

class _CityMapViewState extends State<CityMapView> {
  /// Geladene Iso-Sprites (Slot → Bild). Leer, solange keine PNGs vorliegen →
  /// Vektor-Fallback. Wird beim Ablegen echter Assets automatisch befüllt.
  final Map<String, ui.Image> _sprites = {};

  @override
  void initState() {
    super.initState();
    _loadSprites();
  }

  Future<void> _loadSprites() async {
    final loaded = <String, ui.Image>{};
    for (final entry in IsoArt.manifest.entries) {
      final img = await _tryLoad(entry.value);
      if (img != null) loaded[entry.key] = img;
    }
    if (loaded.isNotEmpty && mounted) {
      setState(() => _sprites
        ..clear()
        ..addAll(loaded));
    }
  }

  /// Lädt ein Asset-Bild; liefert null, wenn das Asset (noch) nicht existiert.
  Future<ui.Image?> _tryLoad(String path) async {
    try {
      final data = await rootBundle.load(path);
      final codec = await ui.instantiateImageCodec(data.buffer.asUint8List());
      final frame = await codec.getNextFrame();
      return frame.image;
    } catch (_) {
      return null; // Asset fehlt → Vektor-Fallback
    }
  }

  @override
  Widget build(BuildContext context) {
    // Belegte Tiles (für Dekor-Aussparung) + stabile Index-basierte Entzerrung,
    // damit zwei Locations nicht auf demselben Tile landen.
    final placed = <_Tile, CityMapLocation>{};
    for (final loc in widget.locations) {
      var t = CityMapView._tileFor(loc);
      var guard = 0;
      while (placed.containsKey(t) && guard < CityMapView._gridN * CityMapView._gridN) {
        t = _Tile((t.col + 1) % CityMapView._gridN,
            t.col + 1 >= CityMapView._gridN ? (t.row + 1) % CityMapView._gridN : t.row);
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
                  width: CityMapView._sceneW,
                  height: CityMapView._sceneH,
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
              Positioned(left: 16, top: 16, child: _CityBadge(city: widget.city)),
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
    final base = CityMapView._iso(tile.col.toDouble(), tile.row.toDouble());
    final owned = widget.shops
        .where((s) => s.cityId == widget.city.id && s.locationName == location.template.name)
        .length;
    final isSelected = widget.selected?.id == location.id;
    final score = location.attractivenessScore(widget.city).round();

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

    // Sprite-Slot (falls echtes Asset geladen) → sonst Vektor-Fallback.
    final sprite = _sprites[IsoArt.slotFor(owned: owned > 0, ownedCount: owned)];

    return Positioned(
      left: left,
      top: top,
      child: GestureDetector(
        behavior: HitTestBehavior.opaque,
        onTap: () => widget.onSelect(location),
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
                  painter: sprite != null
                      ? _SpriteBuildingPainter(image: sprite, lit: isSelected, glow: color)
                      : _IsoBuildingPainter(
                          color: color,
                          height: height,
                          footprint: footprint,
                          lit: isSelected,
                          owned: owned > 0,
                          seed: location.id.hashCode,
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

/// Isometrisches Gebäude mit Fenster-Rastern, Dach-Details und — bei eigenen
/// Filialen — Hero-Look (Neon-Umrandung, Schildband, Innenglühen, Terrasse).
///
/// SPRITE-UPGRADE: Sobald echte Iso-PNGs vorliegen, kann [IsoArt.spriteFor]
/// einen Asset-Pfad liefern; der Renderer zeichnet dann das Sprite statt der
/// Vektor-Geometrie (siehe `_buildHotspot`). Fehlt ein Sprite → dieser Vektor-
/// Fallback. So ist die Map heute spielbar und später auf Foto-Look upgradebar.
class _IsoBuildingPainter extends CustomPainter {
  final Color color;
  final double height;
  final double footprint;
  final bool lit; // ausgewählt
  final bool owned; // eigene Filiale → Hero-Look
  final int seed;

  _IsoBuildingPainter({
    required this.color,
    required this.height,
    required this.footprint,
    required this.lit,
    required this.owned,
    this.seed = 0,
  });

  @override
  void paint(Canvas canvas, Size size) {
    final w = footprint;
    final th = w * 0.5;
    final cx = size.width / 2;
    final baseY = size.height - th / 2;
    final hh = height;

    final top = Offset(cx, baseY - th / 2);
    final right = Offset(cx + w / 2, baseY);
    final bottom = Offset(cx, baseY + th / 2);
    final left = Offset(cx - w / 2, baseY);
    Offset up(Offset p) => Offset(p.dx, p.dy - hh);

    // Bodenschatten
    canvas.drawOval(
      Rect.fromCenter(center: Offset(cx + 7, baseY + 4), width: w * 0.92, height: th * 0.7),
      Paint()
        ..color = Colors.black.withAlpha(105)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 7),
    );

    // Terrasse vor dem Eingang (nur eigene Filiale): Tische + Gäste
    if (owned) _terrace(canvas, bottom, w);

    // Wandfarben (Licht von rechts oben)
    final hsl = HSLColor.fromColor(color);
    Color shade(double f) => hsl.withLightness((hsl.lightness * f).clamp(0.0, 1.0)).toColor();
    final roofPaint = Paint()..color = shade(1.15);
    final leftWall = Paint()..color = shade(0.55);
    final rightWall = Paint()..color = shade(0.80);

    // Wände
    final leftPath = Path()
      ..moveTo(left.dx, left.dy)
      ..lineTo(bottom.dx, bottom.dy)
      ..lineTo(up(bottom).dx, up(bottom).dy)
      ..lineTo(up(left).dx, up(left).dy)
      ..close();
    final rightPath = Path()
      ..moveTo(bottom.dx, bottom.dy)
      ..lineTo(right.dx, right.dy)
      ..lineTo(up(right).dx, up(right).dy)
      ..lineTo(up(bottom).dx, up(bottom).dy)
      ..close();
    canvas.drawPath(leftPath, leftWall);
    canvas.drawPath(rightPath, rightWall);

    // Fenster-Raster auf beiden Wänden
    final cols = (w / 22).round().clamp(2, 5);
    final rows = (hh / 22).round().clamp(1, 6);
    _windows(canvas, left, bottom, hh, cols, rows, warm: owned, dim: true);
    _windows(canvas, bottom, right, hh, cols, rows, warm: owned, dim: false);

    // Dach + Parapet + Aufbauten
    canvas.drawPath(
      Path()
        ..moveTo(up(top).dx, up(top).dy)
        ..lineTo(up(right).dx, up(right).dy)
        ..lineTo(up(bottom).dx, up(bottom).dy)
        ..lineTo(up(left).dx, up(left).dy)
        ..close(),
      roofPaint,
    );
    // Dach-Aufbauten (AC-Units)
    final acPaint = Paint()..color = shade(0.7);
    final roofC = Offset(cx, up(top).dy + (up(bottom).dy - up(top).dy) * 0.5);
    canvas.drawRect(Rect.fromCenter(center: roofC.translate(-w * 0.12, -2), width: 12, height: 8), acPaint);
    canvas.drawRect(Rect.fromCenter(center: roofC.translate(w * 0.14, 3), width: 9, height: 6), acPaint);

    if (owned) {
      // Schildband oben auf der Frontwand (rechts)
      _signBand(canvas, bottom, right, hh);
      // Döner-Leuchtschild auf dem Dach
      _roofSign(canvas, roofC);
      // Neon-Umrandung der Silhouette
      _neonRim(canvas, [up(left), up(top), up(right), right, bottom, left], up(bottom), bottom);
    }

    // Auswahl-Glow
    if (lit) {
      canvas.drawCircle(
        up(top).translate(0, hh * 0.4),
        w * 0.55,
        Paint()
          ..color = color.withAlpha(64)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 18),
      );
    }
  }

  Offset _wp(Offset p0, Offset p1, double h, double s, double t) {
    final base = Offset.lerp(p0, p1, s)!;
    return Offset(base.dx, base.dy - h * t);
  }

  void _windows(Canvas canvas, Offset p0, Offset p1, double h, int cols, int rows,
      {required bool warm, required bool dim}) {
    final dark = Paint()..color = Colors.black.withAlpha(dim ? 70 : 55);
    var s = seed == 0 ? 11 : seed;
    for (var c = 0; c < cols; c++) {
      for (var r = 0; r < rows; r++) {
        s = (s * 1103515245 + 12345) & 0x7fffffff;
        final sx = 0.12 + (c + 0.5) / cols * 0.76;
        final ty = 0.14 + (r + 0.5) / rows * 0.74;
        const wsf = 0.5; // Fensterbreite relativ zur Zellbreite
        final ws = (0.76 / cols) * wsf;
        final wt = (0.74 / rows) * 0.55;
        final quad = Path()
          ..moveTo(_wp(p0, p1, h, sx - ws / 2, ty - wt / 2).dx, _wp(p0, p1, h, sx - ws / 2, ty - wt / 2).dy)
          ..lineTo(_wp(p0, p1, h, sx + ws / 2, ty - wt / 2).dx, _wp(p0, p1, h, sx + ws / 2, ty - wt / 2).dy)
          ..lineTo(_wp(p0, p1, h, sx + ws / 2, ty + wt / 2).dx, _wp(p0, p1, h, sx + ws / 2, ty + wt / 2).dy)
          ..lineTo(_wp(p0, p1, h, sx - ws / 2, ty + wt / 2).dx, _wp(p0, p1, h, sx - ws / 2, ty + wt / 2).dy)
          ..close();
        final isLit = (s % 100) < (warm ? 70 : 38);
        if (isLit) {
          final c2 = warm ? AppColors.gold : const Color(0xFF9FD0FF);
          canvas.drawPath(quad, Paint()..color = c2.withAlpha(dim ? 150 : 205));
        } else {
          canvas.drawPath(quad, dark);
        }
      }
    }
  }

  void _signBand(Canvas canvas, Offset p0, Offset p1, double h) {
    final band = Path()
      ..moveTo(_wp(p0, p1, h, 0.1, 0.80).dx, _wp(p0, p1, h, 0.1, 0.80).dy)
      ..lineTo(_wp(p0, p1, h, 0.9, 0.80).dx, _wp(p0, p1, h, 0.9, 0.80).dy)
      ..lineTo(_wp(p0, p1, h, 0.9, 0.92).dx, _wp(p0, p1, h, 0.9, 0.92).dy)
      ..lineTo(_wp(p0, p1, h, 0.1, 0.92).dx, _wp(p0, p1, h, 0.1, 0.92).dy)
      ..close();
    canvas.drawPath(band, Paint()..color = AppColors.primary);
    canvas.drawPath(
      band,
      Paint()
        ..color = AppColors.gold.withAlpha(120)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6),
    );
  }

  void _roofSign(Canvas canvas, Offset roofC) {
    final p = roofC.translate(0, -10);
    canvas.drawCircle(p, 7, Paint()
      ..color = AppColors.primary.withAlpha(160)
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 8));
    canvas.drawCircle(p, 4, Paint()..color = AppColors.gold);
  }

  void _neonRim(Canvas canvas, List<Offset> roofAndBase, Offset upBottom, Offset bottom) {
    final outline = Path()..moveTo(roofAndBase.first.dx, roofAndBase.first.dy);
    for (final pt in roofAndBase.skip(1)) {
      outline.lineTo(pt.dx, pt.dy);
    }
    outline.close();
    // Vordere Vertikalkante betonen
    final front = Path()
      ..moveTo(bottom.dx, bottom.dy)
      ..lineTo(upBottom.dx, upBottom.dy);

    final glow = Paint()
      ..color = AppColors.primary.withAlpha(200)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 4
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6);
    final crisp = Paint()
      ..color = AppColors.gold
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.6;
    canvas.drawPath(outline, glow);
    canvas.drawPath(front, glow);
    canvas.drawPath(outline, crisp);
    canvas.drawPath(front, crisp);
  }

  void _terrace(Canvas canvas, Offset bottom, double w) {
    final ground = bottom.translate(0, 10);
    final table = Paint()..color = const Color(0xFF6B5640);
    final guest = Paint()..color = AppColors.gold.withAlpha(180);
    final spots = [
      ground.translate(-w * 0.22, 6),
      ground.translate(w * 0.05, 12),
      ground.translate(w * 0.26, 4),
    ];
    for (final s in spots) {
      canvas.drawOval(Rect.fromCenter(center: s, width: 10, height: 5), table);
      canvas.drawCircle(s.translate(-6, -3), 1.8, guest);
      canvas.drawCircle(s.translate(6, -3), 1.8, guest);
    }
  }

  @override
  bool shouldRepaint(covariant _IsoBuildingPainter old) =>
      old.color != color || old.height != height || old.lit != lit || old.owned != owned || old.seed != seed;
}

/// Zeichnet ein geladenes Iso-Sprite (PNG) als Gebäude: Bodenschatten,
/// breitenfüllend + seitenverhältnistreu, unten verankert. Auswahl-Glow.
class _SpriteBuildingPainter extends CustomPainter {
  final ui.Image image;
  final bool lit;
  final Color glow;
  _SpriteBuildingPainter({required this.image, required this.lit, required this.glow});

  @override
  void paint(Canvas canvas, Size size) {
    final th = size.width * 0.5;
    final baseY = size.height - th / 2;

    // Bodenschatten unter dem Sprite
    canvas.drawOval(
      Rect.fromCenter(center: Offset(size.width / 2 + 6, baseY + 2), width: size.width * 0.8, height: th * 0.6),
      Paint()
        ..color = Colors.black.withAlpha(100)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 7),
    );

    final iw = image.width.toDouble();
    final ih = image.height.toDouble();
    final dw = size.width;
    final dh = dw * ih / iw;
    // Bodenkontakt des Sprites ~ etwas oberhalb der Box-Unterkante (auf der Raute).
    final dst = Rect.fromLTWH(0, (baseY + th * 0.18) - dh, dw, dh);

    if (lit) {
      canvas.drawCircle(
        Offset(size.width / 2, dst.top + dh * 0.45),
        size.width * 0.55,
        Paint()
          ..color = glow.withAlpha(60)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 18),
      );
    }

    canvas.drawImageRect(
      image,
      Rect.fromLTWH(0, 0, iw, ih),
      dst,
      Paint()..filterQuality = FilterQuality.medium,
    );
  }

  @override
  bool shouldRepaint(covariant _SpriteBuildingPainter old) =>
      old.image != image || old.lit != lit || old.glow != glow;
}

/// Sprite-Pipeline. [manifest] listet Slot → Asset-Pfad. Liegt das PNG unter
/// `assets/iso/` (in pubspec deklariert) vor, nutzt die Map automatisch das
/// Sprite (Foto-Look); fehlt es, greift der Vektor-Fallback.
///
/// Aktivieren: PNGs gemäß manifest unter `assets/iso/` ablegen (Namen exakt),
/// dann `flutter pub get` + Neustart. Siehe assets/iso/README.md.
class IsoArt {
  const IsoArt._();

  static const Map<String, String> manifest = {
    'owned': 'assets/iso/building_owned.png', // eigene Filiale (Hero-Restaurant)
    'empty': 'assets/iso/building_empty.png', // freier/baubarer Standort
  };

  /// Slot-Auswahl je Gebäudeart.
  static String slotFor({required bool owned, required int ownedCount}) =>
      owned ? 'owned' : 'empty';
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
