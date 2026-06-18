import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../ui/widgets/building_styles.dart';
import '../ui/widgets/iso_tilemap_painter.dart';

const _size = 20;
const _hero = math.Point<int>(11, 11);

/// Deterministische 20×20-Pilotstadt für die prozedurale Iso-Tilemap.
///
/// Die Szene enthält bewusst keine Asset- oder Sprite-Referenzen. Sie dient als
/// visueller und technischer Unterbau, bis stadtindividuelle Layoutdaten
/// ergänzt werden.
TilemapData buildBerlinScene() {
  final tiles = List<List<IsoTile>>.generate(
    _size,
    (_) => List<IsoTile>.filled(_size, const IsoTile.grass()),
  );

  void set(int x, int y, IsoTile tile) {
    if (x >= 0 && x < _size && y >= 0 && y < _size) {
      tiles[y][x] = tile;
    }
  }

  const roadAxes = {5, 10, 15};
  for (var y = 0; y < _size; y++) {
    for (var x = 0; x < _size; x++) {
      if (roadAxes.contains(x) || roadAxes.contains(y)) {
        set(x, y, const IsoTile.road());
      } else if (_nextToRoad(x, y, roadAxes)) {
        set(x, y, const IsoTile.sidewalk());
      }
    }
  }

  // Wasserpromenade unten links.
  for (var y = 16; y < _size; y++) {
    for (var x = 0; x < 5; x++) {
      set(x, y, const IsoTile.water());
    }
  }
  for (var y = 16; y < _size; y++) {
    set(5, y, const IsoTile.sidewalk());
  }

  // Ruhiger Stadtpark oben rechts.
  for (var y = 0; y < 5; y++) {
    for (var x = 16; x < _size; x++) {
      set(x, y, const IsoTile.grass());
    }
  }

  _placeFillerBuildings(tiles, roadAxes);

  set(
    _hero.x,
    _hero.y,
    const IsoTile(
      type: TileType.hero,
      color: Color(0xFF4A2A15),
      accent: MapPalette.accent,
      label: 'Döner Empire',
      rating: 4.6,
      upgradeLevel: BuildingUpgrade.basic,
      spriteAsset: 'assets/iso/building_owned.png',
    ),
  );

  const competitors = [
    (
      tile: math.Point<int>(4, 4),
      name: 'Lezzet Döner',
      rating: 3.2,
    ),
    (
      tile: math.Point<int>(14, 11),
      name: 'City Kebap',
      rating: 3.6,
    ),
    (
      tile: math.Point<int>(7, 14),
      name: 'Berlin Döner',
      rating: 2.8,
    ),
  ];
  for (final competitor in competitors) {
    set(
      competitor.tile.x,
      competitor.tile.y,
      IsoTile(
        type: TileType.competitor,
        color: const Color(0xFF3A2020),
        accent: MapPalette.danger,
        label: competitor.name,
        rating: competitor.rating,
        upgradeLevel: BuildingUpgrade.none,
        spriteAsset: 'assets/iso/building_competitor.png',
      ),
    );
  }

  const freeLocations = [
    (tile: math.Point<int>(6, 4), label: 'Bahnhof'),
    (tile: math.Point<int>(13, 4), label: 'Uni'),
    (tile: math.Point<int>(4, 13), label: 'Innenstadt'),
  ];
  for (final location in freeLocations) {
    set(
      location.tile.x,
      location.tile.y,
      IsoTile(
        type: TileType.empty,
        color: MapPalette.bgBase,
        accent: MapPalette.success,
        label: location.label,
        spriteAsset: 'assets/iso/building_empty.png',
      ),
    );
  }

  return TilemapData(
    width: _size,
    height: _size,
    tiles: tiles,
    heroTile: _hero,
  );
}

bool _nextToRoad(int x, int y, Set<int> roadAxes) {
  return roadAxes.any(
    (axis) =>
        (x - axis).abs() == 1 ||
        (y - axis).abs() == 1,
  );
}

void _placeFillerBuildings(
  List<List<IsoTile>> tiles,
  Set<int> roadAxes,
) {
  for (var y = 1; y < _size - 1; y++) {
    for (var x = 1; x < _size - 1; x++) {
      if (roadAxes.contains(x) ||
          roadAxes.contains(y) ||
          _nextToRoad(x, y, roadAxes)) {
        continue;
      }
      if (y >= 16 && x < 6) continue;
      if (y < 5 && x >= 16) continue;

      final seed = x * 37 + y * 19;
      if (seed % 4 == 0) continue;
      tiles[y][x] = const IsoTile(
        type: TileType.building,
        color: MapPalette.textDim,
        accent: MapPalette.textDim,
        spriteAsset: 'assets/iso/building_filler.png',
      );
    }
  }
}
