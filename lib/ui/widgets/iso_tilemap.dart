import 'dart:math' as math;

import 'package:flutter/widgets.dart';

/// Reine 2:1-Iso-Projektion ohne Abhängigkeit von Spielzustand oder Rendering.
///
/// [origin] bezeichnet den oberen Mittelpunkt der Tilemap-Szene. Ein Tile mit
/// den Koordinaten (0, 0) hat dort seine obere Rautenspitze.
class IsoGrid {
  final double tileWidth;
  final double tileHeight;
  final Offset origin;

  const IsoGrid({
    this.tileWidth = 64,
    this.tileHeight = 32,
    this.origin = Offset.zero,
  })  : assert(tileWidth > 0),
        assert(tileHeight > 0);

  double get halfWidth => tileWidth / 2;
  double get halfHeight => tileHeight / 2;

  /// Projiziert eine Tile-Koordinate auf den Mittelpunkt ihrer Bodenraute.
  Offset tileToScreen(math.Point<num> tile) {
    final x = tile.x.toDouble();
    final y = tile.y.toDouble();
    return Offset(
      origin.dx + (x - y) * halfWidth,
      origin.dy + (x + y) * halfHeight,
    );
  }

  /// Inverse Projektion eines Szenenpunkts auf kontinuierliche Tile-Koordinaten.
  ///
  /// Für Hit-Tests kann das Ergebnis mit [screenToNearestTile] auf das
  /// nächstgelegene Tile gerundet werden.
  math.Point<double> screenToTile(Offset screen) {
    final localX = screen.dx - origin.dx;
    final localY = screen.dy - origin.dy;
    final x = (localX / halfWidth + localY / halfHeight) / 2;
    final y = (localY / halfHeight - localX / halfWidth) / 2;
    return math.Point<double>(x, y);
  }

  math.Point<int> screenToNearestTile(Offset screen) {
    final tile = screenToTile(screen);
    return math.Point<int>(tile.x.round(), tile.y.round());
  }

  /// Szenengröße inklusive Rand für Gebäudehöhe, Glow und Labels.
  Size sceneSize(
    int width,
    int height, {
    double topPadding = 220,
    double sidePadding = 160,
    double bottomPadding = 160,
  }) {
    assert(width > 0);
    assert(height > 0);
    return Size(
      (width + height) * halfWidth + sidePadding * 2,
      (width + height) * halfHeight + topPadding + bottomPadding,
    );
  }

  /// Grid mit einem Origin, der eine vollständige Karte in ihrer Szene hält.
  IsoGrid centeredFor(
    int width,
    int height, {
    double topPadding = 220,
    double sidePadding = 160,
  }) {
    final size = sceneSize(
      width,
      height,
      topPadding: topPadding,
      sidePadding: sidePadding,
    );
    return IsoGrid(
      tileWidth: tileWidth,
      tileHeight: tileHeight,
      origin: Offset(size.width / 2, topPadding),
    );
  }
}
