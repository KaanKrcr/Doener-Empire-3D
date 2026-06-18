import 'dart:math' as math;

import 'package:doener_empire/data/berlin_scene.dart';
import 'package:doener_empire/ui/widgets/iso_tilemap.dart';
import 'package:doener_empire/ui/widgets/iso_tilemap_painter.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('IsoGrid', () {
    const grid = IsoGrid(
      tileWidth: 64,
      tileHeight: 32,
      origin: Offset(320, 120),
    );

    test('tileToScreen und screenToTile sind invers', () {
      const tile = math.Point<int>(7, 12);
      final screen = grid.tileToScreen(tile);
      final restored = grid.screenToTile(screen);

      expect(restored.x, closeTo(tile.x, 0.0001));
      expect(restored.y, closeTo(tile.y, 0.0001));
    });

    test('benachbarte Tiles folgen dem 2:1-Raster', () {
      final origin = grid.tileToScreen(const math.Point<int>(0, 0));
      final xNeighbor = grid.tileToScreen(const math.Point<int>(1, 0));
      final yNeighbor = grid.tileToScreen(const math.Point<int>(0, 1));

      expect(xNeighbor - origin, const Offset(32, 16));
      expect(yNeighbor - origin, const Offset(-32, 16));
    });
  });

  group('Berlin scene', () {
    test('ist 20x20 und enthält den Hero am angegebenen Tile', () {
      final data = buildBerlinScene();

      expect(data.width, 20);
      expect(data.height, 20);
      expect(data.tiles, hasLength(20));
      expect(data.tiles.every((row) => row.length == 20), isTrue);
      expect(
        data.tileAt(data.heroTile.x, data.heroTile.y).type,
        TileType.hero,
      );
    });

    testWidgets('IsoTilemapPainter rendert ohne Exception', (tester) async {
      final data = buildBerlinScene();
      final grid = const IsoGrid().centeredFor(data.width, data.height);
      final sceneSize = grid.sceneSize(data.width, data.height);

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: SingleChildScrollView(
              scrollDirection: Axis.horizontal,
              child: CustomPaint(
                size: sceneSize,
                painter: IsoTilemapPainter(
                  data: data,
                  grid: grid,
                  selectedTile: data.heroTile,
                ),
              ),
            ),
          ),
        ),
      );
      await tester.pump();

      expect(tester.takeException(), isNull);
      expect(find.byType(CustomPaint), findsWidgets);
    });
  });
}
