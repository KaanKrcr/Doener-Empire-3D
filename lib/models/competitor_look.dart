import 'dart:ui' show Color;

import 'building_style_model.dart';
import 'competitor_model.dart';

/// Look-Profil eines Konkurrenten im 2.5D-Straßenzug.
///
/// Jeder Konkurrent bekommt aus seiner [CompetitorPersonality] ein
/// wiedererkennbares Gebäude-Profil (Dachform, Fassade, Akzentfarbe, Schild),
/// damit sich Rivalen optisch klar voneinander und vom Spieler abheben.
class CompetitorLook {
  final BuildingStyle style;
  final String signText;

  const CompetitorLook({required this.style, required this.signText});

  factory CompetitorLook.fromCompetitor(Competitor c) {
    // Deterministische Variation pro Konkurrent (gleiche ID → gleicher Look).
    final seed = c.id.hashCode & 0x7fffffff;
    final floors = 2 + (seed % 3); // 2..4

    switch (c.personality) {
      case CompetitorPersonality.cheapMass:
        return CompetitorLook(
          signText: c.name,
          style: BuildingStyle(
            roofType: RoofType.flat,
            facadeType: FacadeType.concrete,
            accentColor: const Color(0xFFE74C3C),
            windowPattern: WindowPattern.grid,
            floors: floors,
            hasAwning: true,
            hasSign: true,
            condition: BuildingCondition.worn,
            roofColor: const Color(0xFF24262C),
            facadeLightColor: const Color(0xFF22252B),
            facadeDarkColor: const Color(0xFF15171C),
            windowWarmChance: 0.35,
          ),
        );
      case CompetitorPersonality.balanced:
        return CompetitorLook(
          signText: c.name,
          style: BuildingStyle(
            roofType: RoofType.stepped,
            facadeType: FacadeType.brick,
            accentColor: const Color(0xFFC78A4C),
            windowPattern: WindowPattern.grid,
            floors: floors,
            hasAwning: true,
            hasSign: true,
            condition: BuildingCondition.normal,
            roofColor: const Color(0xFF2A211B),
            facadeLightColor: const Color(0xFF2E241C),
            facadeDarkColor: const Color(0xFF1C1611),
            windowWarmChance: 0.4,
          ),
        );
      case CompetitorPersonality.premium:
        return CompetitorLook(
          signText: c.name,
          style: BuildingStyle(
            roofType: RoofType.pointed,
            facadeType: FacadeType.glass,
            accentColor: const Color(0xFFD9B25A),
            windowPattern: WindowPattern.stripe,
            floors: floors,
            hasAwning: false,
            hasSign: true,
            condition: BuildingCondition.premium,
            roofColor: const Color(0xFF1B1E24),
            facadeLightColor: const Color(0xFF23303A),
            facadeDarkColor: const Color(0xFF141C24),
            windowWarmChance: 0.55,
          ),
        );
      case CompetitorPersonality.aggressive:
        return CompetitorLook(
          signText: c.name,
          style: BuildingStyle(
            roofType: RoofType.sawtooth,
            facadeType: FacadeType.concrete,
            accentColor: const Color(0xFFD64531),
            windowPattern: WindowPattern.random,
            floors: floors,
            hasAwning: false,
            hasSign: true,
            condition: BuildingCondition.normal,
            roofColor: const Color(0xFF26282E),
            facadeLightColor: const Color(0xFF24272D),
            facadeDarkColor: const Color(0xFF16181D),
            windowWarmChance: 0.45,
          ),
        );
      case CompetitorPersonality.traditional:
        return CompetitorLook(
          signText: c.name,
          style: BuildingStyle(
            roofType: RoofType.pointed,
            facadeType: FacadeType.wood,
            accentColor: const Color(0xFFB9763A),
            windowPattern: WindowPattern.grid,
            floors: 2 + (seed % 2), // 2..3 — wirkt gediegen-niedrig
            hasAwning: true,
            hasSign: true,
            condition: BuildingCondition.normal,
            roofColor: const Color(0xFF2A1F17),
            facadeLightColor: const Color(0xFF31251B),
            facadeDarkColor: const Color(0xFF1E1610),
            windowWarmChance: 0.5,
          ),
        );
    }
  }
}
