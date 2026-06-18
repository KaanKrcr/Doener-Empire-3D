import 'package:flutter/material.dart';

import '../../models/building_style_model.dart';
import '../../models/city_map_model.dart';
import '../../models/shop_model.dart';

/// Zentrale Premium-Dark-Palette für das gesamte Karten-System.
class MapPalette {
  const MapPalette._();

  static const bgDeep = Color(0xFF0A0806);
  static const bgBase = Color(0xFF130E0A);
  static const bgPanel = Color(0xFF1A130E);
  static const bgCard = Color(0xFF221810);
  static const accent = Color(0xFFF07010); // Warmes Orange (Spieler)
  static const gold = Color(0xFFD46816);
  static const danger = Color(0xFFE74C3C); // Rot (Konkurrenz)
  static const success = Color(0xFF7BC950); // Grün (frei)
  static const textMain = Color(0xFFFFFAE6);
  static const textMuted = Color(0xFFC4B5A0);
  static const textDim = Color(0xFF8C7B6C);

  // Strassen-Töne (Ebene 3)
  static const asphalt = Color(0xFF0C0905);
  static const sidewalk = Color(0xFF221810);
  static const marking = Color(0xFF3D3028);
  static const border = Color(0xFF3A2C20);
}

/// Stil des eigenen Filial-Gebäudes — leitet Zustand und Ausstattung aus dem
/// realen Shop-State ab. Die Filiale startet schäbig und wird durch Ruf und
/// Upgrades sichtbar schöner.
BuildingStyle playerBuildingStyle(Shop shop) {
  final upgradeBoost = shop.upgradeIds.length.clamp(0, 4) * 0.25;
  final score = shop.reputation + upgradeBoost;

  final BuildingCondition condition;
  if (score < 3.0) {
    condition = BuildingCondition.worn;
  } else if (score < 4.2) {
    condition = BuildingCondition.normal;
  } else {
    condition = BuildingCondition.premium;
  }

  final accent = Color(shop.accentColor);
  final floors = (2 + shop.upgradeIds.length.clamp(0, 2)).clamp(2, 4);

  switch (condition) {
    case BuildingCondition.worn:
      return BuildingStyle(
        roofType: RoofType.flat,
        facadeType: FacadeType.plaster,
        accentColor: accent,
        windowPattern: WindowPattern.grid,
        floors: floors,
        hasAwning: false,
        hasSign: false,
        condition: condition,
        roofColor: const Color(0xFF1A1C20),
        facadeLightColor: const Color(0xFF20221F),
        facadeDarkColor: const Color(0xFF141511),
        windowWarmChance: 0.18,
      );
    case BuildingCondition.normal:
      return BuildingStyle(
        roofType: RoofType.flat,
        facadeType: FacadeType.brick,
        accentColor: accent,
        windowPattern: WindowPattern.grid,
        floors: floors,
        hasAwning: true,
        hasSign: true,
        condition: condition,
        roofColor: const Color(0xFF221C16),
        facadeLightColor: const Color(0xFF2A231C),
        facadeDarkColor: const Color(0xFF181410),
        windowWarmChance: 0.42,
      );
    case BuildingCondition.premium:
      return BuildingStyle(
        roofType: RoofType.stepped,
        facadeType: FacadeType.glass,
        accentColor: accent,
        windowPattern: WindowPattern.stripe,
        floors: floors,
        hasAwning: true,
        hasSign: true,
        condition: condition,
        roofColor: const Color(0xFF1B1E24),
        facadeLightColor: const Color(0xFF26323C),
        facadeDarkColor: const Color(0xFF161E26),
        windowWarmChance: 0.62,
      );
  }
}

/// Stil eines freien Bauplatzes (noch keine eigene Filiale) — wirkt leer und
/// einladend in dezentem Grün.
BuildingStyle freePlotStyle(CityMapLocation location) {
  return const BuildingStyle(
    roofType: RoofType.flat,
    facadeType: FacadeType.plaster,
    accentColor: MapPalette.success,
    windowPattern: WindowPattern.none,
    floors: 2,
    hasAwning: false,
    hasSign: false,
    condition: BuildingCondition.worn,
    roofColor: Color(0xFF15171B),
    facadeLightColor: Color(0xFF181B1A),
    facadeDarkColor: Color(0xFF101312),
    windowWarmChance: 0.0,
  );
}

const List<RoofType> _fillerRoofs = [
  RoofType.flat,
  RoofType.pointed,
  RoofType.sawtooth,
  RoofType.stepped,
];

const List<FacadeType> _fillerFacades = [
  FacadeType.plaster,
  FacadeType.brick,
  FacadeType.concrete,
  FacadeType.wood,
];

const List<Color> _fillerRoofColors = [
  Color(0xFF202229),
  Color(0xFF241D17),
  Color(0xFF1C1F26),
  Color(0xFF26201A),
];

const List<List<Color>> _fillerFacadeColors = [
  [Color(0xFF1E2027), Color(0xFF131519)], // kühl grau
  [Color(0xFF2A231C), Color(0xFF181410)], // warm braun
  [Color(0xFF222428), Color(0xFF141619)], // beton
  [Color(0xFF2C241B), Color(0xFF1A1510)], // holz
];

/// Deterministisches Füll-Gebäude (Nachbarhaus) aus einem Seed — jedes sieht
/// anders aus, ändert sich aber nicht zwischen zwei Builds.
BuildingStyle fillerBuildingStyle(int seed) {
  final s = seed & 0x7fffffff;
  final variant = s % 4;
  final floors = 2 + ((s >> 2) % 4); // 2..5
  final warm = 0.15 + ((s >> 5) % 4) * 0.07; // 0.15..0.36
  final condition =
      ((s >> 3) & 1) == 0 ? BuildingCondition.worn : BuildingCondition.normal;

  return BuildingStyle(
    roofType: _fillerRoofs[variant],
    facadeType: _fillerFacades[variant],
    accentColor: const Color(0xFF3A3D44),
    windowPattern: ((s >> 4) & 1) == 0 ? WindowPattern.grid : WindowPattern.random,
    floors: floors,
    hasAwning: false,
    hasSign: false,
    condition: condition,
    roofColor: _fillerRoofColors[variant],
    facadeLightColor: _fillerFacadeColors[variant][0],
    facadeDarkColor: _fillerFacadeColors[variant][1],
    windowWarmChance: warm,
  );
}
