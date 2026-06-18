import 'dart:math' as math;

import 'package:flutter/material.dart';

import 'building_styles.dart';
import 'iso_tilemap.dart';

enum TileType {
  empty,
  road,
  sidewalk,
  grass,
  water,
  building,
  hero,
  competitor,
}

@immutable
class IsoTile {
  final TileType type;
  final Color color;
  final Color? accent;
  final String? label;
  final double? rating;

  const IsoTile({
    required this.type,
    required this.color,
    this.accent,
    this.label,
    this.rating,
  });

  const IsoTile.grass()
      : this(type: TileType.grass, color: MapPalette.success);

  const IsoTile.road()
      : this(type: TileType.road, color: MapPalette.asphalt);

  const IsoTile.sidewalk()
      : this(type: TileType.sidewalk, color: MapPalette.sidewalk);

  const IsoTile.water()
      : this(type: TileType.water, color: const Color(0xFF10242C));
}

@immutable
class TilemapData {
  final int width;
  final int height;
  final List<List<IsoTile>> tiles;
  final math.Point<int> heroTile;

  TilemapData({
    required this.width,
    required this.height,
    required this.tiles,
    required this.heroTile,
  })  : assert(width > 0),
        assert(height > 0),
        assert(tiles.length == height),
        assert(tiles.every((row) => row.length == width)),
        assert(heroTile.x >= 0 && heroTile.x < width),
        assert(heroTile.y >= 0 && heroTile.y < height);

  IsoTile tileAt(int x, int y) => tiles[y][x];

  bool contains(math.Point<int> tile) =>
      tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height;
}

class IsoTilemapPainter extends CustomPainter {
  final TilemapData data;
  final IsoGrid grid;
  final math.Point<int>? selectedTile;

  const IsoTilemapPainter({
    required this.data,
    required this.grid,
    this.selectedTile,
  });

  @override
  void paint(Canvas canvas, Size size) {
    canvas.drawRect(
      Offset.zero & size,
      Paint()
        ..shader = const RadialGradient(
          center: Alignment(0, -0.25),
          radius: 1.1,
          colors: [Color(0xFF24170D), MapPalette.bgDeep],
        ).createShader(Offset.zero & size),
    );

    _drawGround(canvas);
    _drawBuildings(canvas);
    _drawVignette(canvas, size);
  }

  void _drawGround(Canvas canvas) {
    for (var depth = 0; depth <= data.width + data.height - 2; depth++) {
      for (var y = 0; y < data.height; y++) {
        final x = depth - y;
        if (x < 0 || x >= data.width) continue;
        final tile = data.tileAt(x, y);
        final center = grid.tileToScreen(math.Point<int>(x, y));
        _drawGroundTile(canvas, tile, center, x, y);
      }
    }
  }

  void _drawGroundTile(
    Canvas canvas,
    IsoTile tile,
    Offset center,
    int x,
    int y,
  ) {
    final path = _diamond(center);
    final fill = switch (tile.type) {
      TileType.road => MapPalette.asphalt,
      TileType.sidewalk ||
      TileType.building ||
      TileType.hero ||
      TileType.competitor =>
        MapPalette.sidewalk,
      TileType.grass => const Color(0xFF182819),
      TileType.water => const Color(0xFF10242C),
      TileType.empty => MapPalette.bgBase,
    };

    canvas.drawPath(path, Paint()..color = fill);
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 0.8
        ..color = _lighten(fill, 0.08).withAlpha(145),
    );

    switch (tile.type) {
      case TileType.road:
        _drawRoadMarking(canvas, center, x, y);
        break;
      case TileType.water:
        _drawWaterHighlight(canvas, center);
        break;
      case TileType.grass:
        _drawGrassDetail(canvas, center, x, y);
        break;
      case TileType.empty:
        _drawFreePlot(canvas, path, center, tile);
        break;
      case TileType.sidewalk:
      case TileType.building:
      case TileType.hero:
      case TileType.competitor:
        break;
    }
  }

  void _drawRoadMarking(Canvas canvas, Offset center, int x, int y) {
    final horizontal = (x + y).isEven;
    final delta = horizontal
        ? Offset(grid.halfWidth * 0.36, grid.halfHeight * 0.36)
        : Offset(-grid.halfWidth * 0.36, grid.halfHeight * 0.36);
    canvas.drawLine(
      center - delta,
      center + delta,
      Paint()
        ..color = MapPalette.marking.withAlpha(170)
        ..strokeWidth = 1.4
        ..strokeCap = StrokeCap.round,
    );
  }

  void _drawWaterHighlight(Canvas canvas, Offset center) {
    final paint = Paint()
      ..color = const Color(0xFF2D6470).withAlpha(100)
      ..strokeWidth = 1.2;
    canvas.drawLine(
      center.translate(-grid.halfWidth * 0.45, 0),
      center.translate(grid.halfWidth * 0.08, grid.halfHeight * 0.26),
      paint,
    );
  }

  void _drawGrassDetail(Canvas canvas, Offset center, int x, int y) {
    if ((x * 17 + y * 31) % 5 != 0) return;
    canvas.drawCircle(
      center.translate(0, -2),
      2.2,
      Paint()..color = MapPalette.success.withAlpha(90),
    );
  }

  void _drawFreePlot(
    Canvas canvas,
    Path path,
    Offset center,
    IsoTile tile,
  ) {
    final accent = tile.accent ?? MapPalette.success;
    canvas.drawPath(
      path,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 2
        ..color = accent.withAlpha(210),
    );
    canvas.drawCircle(
      center,
      5,
      Paint()
        ..color = accent.withAlpha(70)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
    );
    canvas.drawCircle(center, 2.5, Paint()..color = accent);
    _drawLabel(
      canvas,
      tile,
      center.translate(0, -grid.tileHeight * 0.9),
      accent,
    );
  }

  void _drawBuildings(Canvas canvas) {
    for (var depth = 0; depth <= data.width + data.height - 2; depth++) {
      for (var y = 0; y < data.height; y++) {
        final x = depth - y;
        if (x < 0 || x >= data.width) continue;
        final tile = data.tileAt(x, y);
        if (!_isBuilding(tile.type)) continue;

        final center = grid.tileToScreen(math.Point<int>(x, y));
        final selected = selectedTile == math.Point<int>(x, y);
        _drawBuilding(canvas, tile, center, x, y, selected: selected);
      }
    }
  }

  bool _isBuilding(TileType type) =>
      type == TileType.building ||
      type == TileType.hero ||
      type == TileType.competitor;

  void _drawBuilding(
    Canvas canvas,
    IsoTile tile,
    Offset center,
    int x,
    int y, {
    required bool selected,
  }) {
    final accent = switch (tile.type) {
      TileType.hero => MapPalette.accent,
      TileType.competitor => MapPalette.danger,
      _ => tile.accent ?? MapPalette.textDim,
    };
    final height = switch (tile.type) {
      TileType.hero => grid.tileHeight * 3.45,
      TileType.competitor => grid.tileHeight * 2.45,
      _ => grid.tileHeight * (1.65 + ((x * 13 + y * 7) % 4) * 0.28),
    };
    final footprintScale = tile.type == TileType.hero ? 0.94 : 0.78;
    final halfW = grid.halfWidth * footprintScale;
    final halfH = grid.halfHeight * footprintScale;

    if (tile.type == TileType.hero || selected) {
      _drawBuildingGlow(canvas, center, accent, selected: selected);
    }

    final bottomTop = center.translate(0, -halfH);
    final bottomRight = center.translate(halfW, 0);
    final bottomBottom = center.translate(0, halfH);
    final bottomLeft = center.translate(-halfW, 0);
    final topTop = bottomTop.translate(0, -height);
    final topRight = bottomRight.translate(0, -height);
    final topBottom = bottomBottom.translate(0, -height);
    final topLeft = bottomLeft.translate(0, -height);

    final leftFace = Path()
      ..moveTo(bottomLeft.dx, bottomLeft.dy)
      ..lineTo(bottomBottom.dx, bottomBottom.dy)
      ..lineTo(topBottom.dx, topBottom.dy)
      ..lineTo(topLeft.dx, topLeft.dy)
      ..close();
    final rightFace = Path()
      ..moveTo(bottomBottom.dx, bottomBottom.dy)
      ..lineTo(bottomRight.dx, bottomRight.dy)
      ..lineTo(topRight.dx, topRight.dy)
      ..lineTo(topBottom.dx, topBottom.dy)
      ..close();
    final roof = Path()
      ..moveTo(topTop.dx, topTop.dy)
      ..lineTo(topRight.dx, topRight.dy)
      ..lineTo(topBottom.dx, topBottom.dy)
      ..lineTo(topLeft.dx, topLeft.dy)
      ..close();

    final base = tile.color;
    _fillGradient(
      canvas,
      leftFace,
      _lighten(base, 0.08),
      _darken(base, 0.12),
    );
    _fillGradient(
      canvas,
      rightFace,
      _darken(base, 0.02),
      _darken(base, 0.24),
    );
    canvas.drawPath(
      roof,
      Paint()
        ..shader = LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [_lighten(base, 0.18), base],
        ).createShader(roof.getBounds()),
    );

    _drawBuildingEdges(canvas, leftFace, rightFace, roof, accent, tile.type);
    _drawWindows(
      canvas,
      bottomLeft,
      bottomBottom,
      bottomRight,
      height,
      accent,
      tile.type,
    );
    _drawRoofUnit(canvas, Offset.lerp(topTop, topBottom, 0.48)!, base);
    _drawLabel(canvas, tile, topTop.translate(0, -18), accent);
  }

  void _drawBuildingGlow(
    Canvas canvas,
    Offset center,
    Color accent, {
    required bool selected,
  }) {
    final radius = grid.tileWidth * (selected ? 1.25 : 1.55);
    final rect = Rect.fromCircle(center: center, radius: radius);
    canvas.drawCircle(
      center,
      radius,
      Paint()
        ..shader = RadialGradient(
          colors: [
            accent.withAlpha(selected ? 95 : 80),
            accent.withAlpha(0),
          ],
        ).createShader(rect),
    );
  }

  void _drawBuildingEdges(
    Canvas canvas,
    Path leftFace,
    Path rightFace,
    Path roof,
    Color accent,
    TileType type,
  ) {
    final highlighted =
        type == TileType.hero || type == TileType.competitor;
    if (highlighted) {
      for (final path in [leftFace, rightFace, roof]) {
        canvas.drawPath(
          path,
          Paint()
            ..style = PaintingStyle.stroke
            ..strokeWidth = 4
            ..color = accent.withAlpha(100)
            ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 7),
        );
      }
    }

    final edge = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = highlighted ? 2 : 0.9
      ..color = highlighted ? accent : MapPalette.border.withAlpha(210);
    canvas.drawPath(leftFace, edge);
    canvas.drawPath(rightFace, edge);
    canvas.drawPath(roof, edge);
  }

  void _drawWindows(
    Canvas canvas,
    Offset bottomLeft,
    Offset bottomBottom,
    Offset bottomRight,
    double height,
    Color accent,
    TileType type,
  ) {
    final rows = math.max(2, (height / 28).floor());
    for (var row = 0; row < rows; row++) {
      final v = (row + 1) / (rows + 1);
      final leftA = Offset.lerp(bottomLeft, bottomBottom, 0.2)!;
      final leftB = Offset.lerp(bottomLeft, bottomBottom, 0.8)!;
      final rightA = Offset.lerp(bottomBottom, bottomRight, 0.2)!;
      final rightB = Offset.lerp(bottomBottom, bottomRight, 0.8)!;
      final yOffset = -height * v;
      final warm = (row + rows) % 3 != 0;
      final windowColor = warm
          ? (type == TileType.hero
              ? MapPalette.accent
              : const Color(0xFFD69A51))
          : const Color(0xFF17130F);
      final paint = Paint()
        ..color = windowColor.withAlpha(warm ? 205 : 230)
        ..strokeWidth = type == TileType.hero ? 2.4 : 1.8
        ..strokeCap = StrokeCap.round;
      canvas.drawLine(
        leftA.translate(0, yOffset),
        leftB.translate(0, yOffset),
        paint,
      );
      canvas.drawLine(
        rightA.translate(0, yOffset),
        rightB.translate(0, yOffset),
        paint,
      );
    }

    if (type == TileType.hero || type == TileType.competitor) {
      final signPaint = Paint()
        ..color = accent
        ..strokeWidth = 3
        ..strokeCap = StrokeCap.round;
      canvas.drawLine(
        Offset.lerp(bottomLeft, bottomBottom, 0.18)!.translate(0, -16),
        Offset.lerp(bottomLeft, bottomBottom, 0.82)!.translate(0, -16),
        signPaint,
      );
      canvas.drawLine(
        Offset.lerp(bottomBottom, bottomRight, 0.18)!.translate(0, -16),
        Offset.lerp(bottomBottom, bottomRight, 0.82)!.translate(0, -16),
        signPaint,
      );
    }
  }

  void _drawRoofUnit(Canvas canvas, Offset center, Color base) {
    final unit = Path()
      ..moveTo(center.dx, center.dy - 4)
      ..lineTo(center.dx + 8, center.dy)
      ..lineTo(center.dx, center.dy + 4)
      ..lineTo(center.dx - 8, center.dy)
      ..close();
    canvas.drawPath(unit, Paint()..color = _lighten(base, 0.22));
    canvas.drawPath(
      unit,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 0.8
        ..color = MapPalette.border,
    );
  }

  void _drawLabel(
    Canvas canvas,
    IsoTile tile,
    Offset anchor,
    Color accent,
  ) {
    final label = tile.label;
    if (label == null || label.isEmpty) return;
    final rating = tile.rating;
    final text = rating == null ? label : '$label  ★ ${rating.toStringAsFixed(1)}';
    final textPainter = TextPainter(
      text: TextSpan(
        text: text,
        style: const TextStyle(
          color: MapPalette.textMain,
          fontFamily: 'Inter',
          fontSize: 11,
          fontWeight: FontWeight.w700,
        ),
      ),
      maxLines: 1,
      ellipsis: '…',
      textDirection: TextDirection.ltr,
    )..layout(maxWidth: 150);
    final rect = Rect.fromCenter(
      center: anchor,
      width: textPainter.width + 18,
      height: textPainter.height + 10,
    );
    final rrect = RRect.fromRectAndRadius(rect, const Radius.circular(9));
    canvas.drawRRect(
      rrect,
      Paint()..color = MapPalette.bgPanel.withAlpha(235),
    );
    canvas.drawRRect(
      rrect,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.2
        ..color = accent.withAlpha(220),
    );
    textPainter.paint(
      canvas,
      Offset(
        rect.center.dx - textPainter.width / 2,
        rect.center.dy - textPainter.height / 2,
      ),
    );
  }

  void _fillGradient(Canvas canvas, Path path, Color top, Color bottom) {
    final bounds = path.getBounds();
    canvas.drawPath(
      path,
      Paint()
        ..shader = LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [top, bottom],
        ).createShader(bounds),
    );
  }

  Path _diamond(Offset center) {
    return Path()
      ..moveTo(center.dx, center.dy - grid.halfHeight)
      ..lineTo(center.dx + grid.halfWidth, center.dy)
      ..lineTo(center.dx, center.dy + grid.halfHeight)
      ..lineTo(center.dx - grid.halfWidth, center.dy)
      ..close();
  }

  void _drawVignette(Canvas canvas, Size size) {
    canvas.drawRect(
      Offset.zero & size,
      Paint()
        ..shader = const RadialGradient(
          radius: 0.95,
          colors: [Colors.transparent, Color(0xAA0A0806)],
          stops: [0.58, 1],
        ).createShader(Offset.zero & size),
    );
  }

  Color _lighten(Color color, double amount) {
    final hsl = HSLColor.fromColor(color);
    return hsl
        .withLightness((hsl.lightness + amount).clamp(0.0, 1.0))
        .toColor();
  }

  Color _darken(Color color, double amount) {
    final hsl = HSLColor.fromColor(color);
    return hsl
        .withLightness((hsl.lightness - amount).clamp(0.0, 1.0))
        .toColor();
  }

  @override
  bool shouldRepaint(covariant IsoTilemapPainter oldDelegate) {
    return oldDelegate.data != data ||
        oldDelegate.grid != grid ||
        oldDelegate.selectedTile != selectedTile;
  }
}
