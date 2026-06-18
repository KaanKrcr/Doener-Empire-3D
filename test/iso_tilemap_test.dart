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
    test('ist 20x20 und ohne Shops ist Hero-Tile empty', () {
      final data = buildCityScene('berlin', []);

      expect(data.width, 20);
      expect(data.height, 20);
      expect(data.tiles, hasLength(20));
      expect(data.tiles.every((row) => row.length == 20), isTrue);
      expect(
        data.tileAt(data.heroTile.x, data.heroTile.y).type,
        TileType.empty,
      );
      final competitors = data.tiles
          .expand((row) => row)
          .where((tile) => tile.type == TileType.competitor);
      expect(competitors, isNotEmpty);
      expect(
        competitors.every(
          (tile) => tile.upgradeLevel == BuildingUpgrade.none,
        ),
        isTrue,
      );
    });

    testWidgets('IsoTilemapPainter rendert ohne Exception', (tester) async {
      final data = buildCityScene('berlin', []);
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

  group('DÃ¶nerladen Upgrade-Render', () {
    for (final upgrade in BuildingUpgrade.values) {
      testWidgets('${upgrade.name} rendert ohne Exception', (tester) async {
        final data = _singleShopScene(upgrade);
        final grid = const IsoGrid().centeredFor(data.width, data.height);
        final sceneSize = grid.sceneSize(data.width, data.height);

        await tester.pumpWidget(
          MaterialApp(
            home: Scaffold(
              body: CustomPaint(
                size: sceneSize,
                painter: IsoTilemapPainter(
                  data: data,
                  grid: grid,
                  selectedTile: data.heroTile,
                ),
              ),
            ),
          ),
        );
        await tester.pump();

        expect(tester.takeException(), isNull);
      });
    }
  });
}

TilemapData _singleShopScene(BuildingUpgrade upgrade) {
  final tiles = List<List<IsoTile>>.generate(
    3,
    (_) => List<IsoTile>.filled(3, const IsoTile.sidewalk()),
  );
  tiles[1][1] = IsoTile(
    type: TileType.hero,
    color: const Color(0xFF4A2A15),
    accent: const Color(0xFFF07010),
    label: 'DÃ¶ner',
    rating: 4.6,
    upgradeLevel: upgrade,
  );
  return TilemapData(
    width: 3,
    height: 3,
    tiles: tiles,
    heroTile: const math.Point<int>(1, 1),
  );
}


