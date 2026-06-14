import 'dart:ui' show Color;

/// Dachform eines Strassenzug-Gebäudes.
enum RoofType { flat, pointed, sawtooth, stepped }

/// Fassadentyp (bestimmt Textur-Andeutung im Painter).
enum FacadeType { plaster, brick, glass, wood, concrete }

/// Fenster-Anordnung auf der Fassade.
enum WindowPattern { grid, random, stripe, none }

/// Zustand eines Gebäudes — schäbig bis premium.
enum BuildingCondition { worn, normal, premium }

/// Vollständig parametrisierte Beschreibung eines Gebäudes im 2.5D-Straßenzug.
/// Wird ausschliesslich vom [StreetBuildingPainter] gezeichnet — keine Bitmaps.
class BuildingStyle {
  final RoofType roofType;
  final FacadeType facadeType;
  final Color accentColor;
  final WindowPattern windowPattern;
  final int floors;
  final bool hasAwning;
  final bool hasSign;
  final BuildingCondition condition;
  final Color roofColor;
  final Color facadeLightColor;
  final Color facadeDarkColor;

  /// Anteil der Fenster, die warm leuchten (0..1).
  final double windowWarmChance;

  const BuildingStyle({
    this.roofType = RoofType.flat,
    this.facadeType = FacadeType.plaster,
    this.accentColor = const Color(0xFFF5A623),
    this.windowPattern = WindowPattern.grid,
    this.floors = 3,
    this.hasAwning = false,
    this.hasSign = false,
    this.condition = BuildingCondition.normal,
    this.roofColor = const Color(0xFF1E2127),
    this.facadeLightColor = const Color(0xFF1C1F25),
    this.facadeDarkColor = const Color(0xFF101216),
    this.windowWarmChance = 0.25,
  });

  BuildingStyle copyWith({
    RoofType? roofType,
    FacadeType? facadeType,
    Color? accentColor,
    WindowPattern? windowPattern,
    int? floors,
    bool? hasAwning,
    bool? hasSign,
    BuildingCondition? condition,
    Color? roofColor,
    Color? facadeLightColor,
    Color? facadeDarkColor,
    double? windowWarmChance,
  }) {
    return BuildingStyle(
      roofType: roofType ?? this.roofType,
      facadeType: facadeType ?? this.facadeType,
      accentColor: accentColor ?? this.accentColor,
      windowPattern: windowPattern ?? this.windowPattern,
      floors: floors ?? this.floors,
      hasAwning: hasAwning ?? this.hasAwning,
      hasSign: hasSign ?? this.hasSign,
      condition: condition ?? this.condition,
      roofColor: roofColor ?? this.roofColor,
      facadeLightColor: facadeLightColor ?? this.facadeLightColor,
      facadeDarkColor: facadeDarkColor ?? this.facadeDarkColor,
      windowWarmChance: windowWarmChance ?? this.windowWarmChance,
    );
  }
}
