import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../models/shop_model.dart';
import '../ui/widgets/building_styles.dart';
import '../ui/widgets/iso_tilemap_painter.dart';

const _size = 20;
const _heroDefault = math.Point<int>(11, 11);

/// Baut eine 20×20-Iso-Stadt basierend auf tatsächlichen Spieldaten.
///
/// [cityId] bestimmt, welche Stadt geladen wird.
/// [shops] sind die geöffneten Filialen des Spielers in dieser Stadt.
/// Freie Standorte werden immer angezeigt — sobald eine Filiale existiert,
/// wird das entsprechende Tile zum Hero.
TilemapData buildCityScene(String cityId, List<Shop> shops) {
  final tiles = List<List<IsoTile>>.generate(
    _size,
    (_) => List<IsoTile>.filled(_size, const IsoTile.grass()),
  );

  void set(int x, int y, IsoTile tile) {
    if (x >= 0 && x < _size && y >= 0 && y < _size) {
      tiles[y][x] = tile;
    }
  }

  // ── Strassen + Gehwege ──
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

  // ── Wasserpromenade ──
  for (var y = 16; y < _size; y++) {
    for (var x = 0; x < 5; x++) {
      set(x, y, const IsoTile.water());
    }
    set(5, y, const IsoTile.sidewalk());
  }

  // ── Stadtpark ──
  for (var y = 0; y < 5; y++) {
    for (var x = 16; x < _size; x++) {
      set(x, y, const IsoTile.grass());
    }
  }

  // ── Füll-Gebäude ──
  _placeFillerBuildings(tiles, roadAxes);

  // ── Hero-Tile: erste Filiale des Spielers in dieser Stadt ──
  final cityShops = shops.where((s) => s.cityId == cityId).toList();
  final hasOwnShop = cityShops.isNotEmpty;
  var heroTile = _heroDefault;

  if (hasOwnShop) {
    // Erste Filiale = Hero (später: mehrere = mehrere Heroes)
    heroTile = _heroDefault;
    final shop = cityShops.first;
    set(heroTile.x, heroTile.y, IsoTile(
      type: TileType.hero,
      color: const Color(0xFF4A2A15),
      accent: MapPalette.accent,
      label: shop.displayName,
      rating: shop.reputation,
      upgradeLevel: _shopToUpgrade(shop),
      spriteAsset: 'assets/iso/building_owned.png',
    ));
  } else {
    // Keine Filiale → Hero-Tile ist ein freier Standort
    set(heroTile.x, heroTile.y, IsoTile(
      type: TileType.empty,
      color: MapPalette.bgBase,
      accent: MapPalette.success,
      label: 'Premium-Standort',
      spriteAsset: 'assets/iso/building_empty.png',
    ));
  }

  // ── Konkurrenz (fest, kann später dynamisch aus game.competitors kommen) ──
  const competitors = [
    (tile: math.Point<int>(4, 4), name: 'Lezzet Döner', rating: 3.2),
    (tile: math.Point<int>(14, 11), name: 'City Kebap', rating: 3.6),
    (tile: math.Point<int>(7, 14), name: 'Berlin Döner', rating: 2.8),
  ];
  for (final comp in competitors) {
    set(comp.tile.x, comp.tile.y, IsoTile(
      type: TileType.competitor,
      color: const Color(0xFF3A2020),
      accent: MapPalette.danger,
      label: comp.name,
      rating: comp.rating,
      upgradeLevel: BuildingUpgrade.none,
      spriteAsset: 'assets/iso/building_competitor.png',
    ));
  }

  // ── Freie Standorte (wo keine eigene Filiale ist) ──
  const freeLocations = [
    (tile: math.Point<int>(6, 4), label: 'Bahnhof'),
    (tile: math.Point<int>(13, 4), label: 'Uni'),
    (tile: math.Point<int>(4, 13), label: 'Innenstadt'),
  ];
  for (final loc in freeLocations) {
    // Nur anzeigen wenn nicht bereits eine Filiale des Spielers hier ist
    final alreadyOwned = cityShops.any((s) =>
        s.locationName == loc.label);
    if (!alreadyOwned) {
      set(loc.tile.x, loc.tile.y, IsoTile(
        type: TileType.empty,
        color: MapPalette.bgBase,
        accent: MapPalette.success,
        label: loc.label,
        spriteAsset: 'assets/iso/building_empty.png',
      ));
    }
  }

  // Falls mehrere Filialen: weitere auf passende freie Standorte setzen
  // (einfache Zuordnung: zweite Filiale auf 6,4; dritte auf 13,4 usw.)
  if (cityShops.length >= 2) {
    for (var i = 1; i < cityShops.length && i <= freeLocations.length; i++) {
      final loc = freeLocations[i - 1];
      final shop = cityShops[i];
      set(loc.tile.x, loc.tile.y, IsoTile(
        type: TileType.hero,
        color: const Color(0xFF4A2A15),
        accent: MapPalette.accent,
        label: shop.displayName,
        rating: shop.reputation,
        upgradeLevel: _shopToUpgrade(shop),
        spriteAsset: 'assets/iso/building_owned.png',
      ));
    }
  }

  return TilemapData(
    width: _size,
    height: _size,
    tiles: tiles,
    heroTile: heroTile,
  );
}

BuildingUpgrade _shopToUpgrade(Shop shop) {
  final score = shop.reputation + shop.upgradeIds.length * 0.25;
  if (score < 3.0) return BuildingUpgrade.basic;
  if (score < 4.5) return BuildingUpgrade.normal;
  return BuildingUpgrade.premium;
}

bool _nextToRoad(int x, int y, Set<int> roadAxes) {
  return roadAxes.any(
    (axis) => (x - axis).abs() == 1 || (y - axis).abs() == 1,
  );
}

void _placeFillerBuildings(
  List<List<IsoTile>> tiles,
  Set<int> roadAxes,
) {
  for (var y = 1; y < _size - 1; y++) {
    for (var x = 1; x < _size - 1; x++) {
      if (roadAxes.contains(x) || roadAxes.contains(y) || _nextToRoad(x, y, roadAxes)) continue;
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
