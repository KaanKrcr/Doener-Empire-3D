import 'package:flutter/widgets.dart';

import '../core/constants.dart';
import '../models/city_map_model.dart';
import '../models/city_model.dart';
import '../models/competitor_model.dart';
import '../models/game_state.dart';
import '../models/shop_model.dart';
import '../models/time_profile_model.dart';

class CityMapSummary {
  final int shopCount;
  final int totalFootTraffic;
  final double weeklyRent;
  final double avgReputation;

  const CityMapSummary({
    required this.shopCount,
    required this.totalFootTraffic,
    required this.weeklyRent,
    required this.avgReputation,
  });

  bool get hasPresence => shopCount > 0;
}

class LocationOpeningForecast {
  final int estimatedCustomersPerDay;
  final double estimatedProfitPerDay;
  final int? breakEvenDays;

  const LocationOpeningForecast({
    required this.estimatedCustomersPerDay,
    required this.estimatedProfitPerDay,
    required this.breakEvenDays,
  });

  bool get isProfitable => estimatedProfitPerDay > 0;
}

class CityCompetitionBrief {
  final int rivalCount;
  final int rivalShopCount;
  final double rivalMarketShare;
  final Competitor? strongestRival;

  const CityCompetitionBrief({
    required this.rivalCount,
    required this.rivalShopCount,
    required this.rivalMarketShare,
    required this.strongestRival,
  });

  bool get hasRivals => rivalCount > 0;

  String get pressureLabel {
    if (!hasRivals) return 'Noch offen';
    if (rivalMarketShare >= 0.55) return 'Hart';
    if (rivalMarketShare >= 0.32) return 'Spuerbar';
    return 'Leicht';
  }

  String get recommendation {
    if (!hasRivals) {
      return 'Noch keine direkte KI-Konkurrenz sichtbar. Standort nach Traffic und Kaution waehlen.';
    }
    final rival = strongestRival!;
    switch (rival.personality) {
      case CompetitorPersonality.cheapMass:
        return '${rival.name} drueckt ueber Preis. Nicht blind unterbieten: Tempo und Kombis absichern.';
      case CompetitorPersonality.balanced:
        return '${rival.name} ist solide. Gleichmaessige Qualitaet und Ruf schlagen reine Rabatte.';
      case CompetitorPersonality.premium:
        return '${rival.name} verkauft Premium. Klassiker guenstig halten, Top-Produkte gezielt verteuern.';
      case CompetitorPersonality.aggressive:
        return '${rival.name} expandiert aggressiv. Cash-Reserve halten und Personalengpaesse vermeiden.';
      case CompetitorPersonality.traditional:
        return '${rival.name} lebt vom Ruf. Lokales Marketing und Bewertung zuerst staerken.';
    }
  }
}

/// Adapter zwischen bestehenden Listen-Standorten und der neuen City-Map.
/// Keine Seiteneffekte: sicher für Tests und UI-Prognosen.
class LocationEngine {
  const LocationEngine._();

  static List<CityMapLocation> locationsFor(CityData city) {
    final templates = kLocationTemplates[city.tier] ??
        kLocationTemplates[CityTier.klein] ??
        const <LocationTemplate>[];
    return List.generate(templates.length, (index) {
      final template = templates[index];
      final meta = _metaFor(template.personality, template.name);
      return CityMapLocation(
        id: '${city.id}_${_slug(template.name)}',
        label: template.name,
        icon: meta.icon,
        mapPosition: _positionFor(index, templates.length),
        template: template,
        audience: meta.audience,
        risk: meta.risk,
        recommendation: meta.recommendation,
      );
    });
  }

  static CityMapLocation? findLocation(CityData city, String locationName) {
    for (final location in locationsFor(city)) {
      if (location.template.name == locationName ||
          location.label == locationName) {
        return location;
      }
    }
    return null;
  }

  static CityMapSummary summarize(CityData city, List<Shop> allShops) {
    final shops = allShops.where((shop) => shop.cityId == city.id).toList();
    final reputation = shops.isEmpty
        ? 0.0
        : shops.fold<double>(0, (sum, shop) => sum + shop.reputation) /
            shops.length;
    return CityMapSummary(
      shopCount: shops.length,
      totalFootTraffic:
          shops.fold<int>(0, (sum, shop) => sum + shop.footTraffic),
      weeklyRent: shops.fold<double>(0, (sum, shop) => sum + shop.weeklyRent),
      avgReputation: reputation,
    );
  }

  static CityCompetitionBrief competitionBrief(
    GameState state,
    String cityId,
  ) {
    final rivals = state.competitorsIn(cityId);
    if (rivals.isEmpty) {
      return const CityCompetitionBrief(
        rivalCount: 0,
        rivalShopCount: 0,
        rivalMarketShare: 0,
        strongestRival: null,
      );
    }

    final strongest = rivals.reduce(
      (best, next) => next.marketShare > best.marketShare ? next : best,
    );
    final marketShare = rivals
        .fold<double>(0, (sum, rival) => sum + rival.marketShare)
        .clamp(0.0, 0.95);

    return CityCompetitionBrief(
      rivalCount: rivals.length,
      rivalShopCount:
          rivals.fold<int>(0, (sum, rival) => sum + rival.shopCount),
      rivalMarketShare: marketShare,
      strongestRival: strongest,
    );
  }

  static LocationOpeningForecast forecastOpening(
    CityData city,
    CityMapLocation location,
  ) {
    final starterProduct = kAllProducts.firstWhere(
      (product) => product.id == 'doener_fladen',
      orElse: () => kAllProducts.first,
    );
    final timeProfile = kTimeProfiles[location.personality] ??
        kTimeProfiles[LocationPersonality.touristic]!;
    final weeklyTimeMultiplier = List.generate(
          7,
          (day) => timeProfile.dailyAverage(day),
        ).fold<double>(0, (sum, value) => sum + value) /
        7;
    final customers =
        (location.footTrafficFor(city) * 0.06 * weeklyTimeMultiplier)
            .round()
            .clamp(0, 999999);
    final grossMargin =
        starterProduct.basePrice - starterProduct.ingredientCostPerUnit;
    final profitPerDay =
        (customers * grossMargin) - (location.weeklyRentFor(city) / 7);
    final breakEvenDays = profitPerDay <= 0
        ? null
        : (location.depositFor(city) / profitPerDay).ceil();

    return LocationOpeningForecast(
      estimatedCustomersPerDay: customers,
      estimatedProfitPerDay: profitPerDay,
      breakEvenDays: breakEvenDays,
    );
  }

  static ({String icon, String audience, String risk, String recommendation})
      _metaFor(LocationPersonality personality, String name) {
    switch (personality) {
      case LocationPersonality.business:
        return (
          icon: '🏢',
          audience: 'Büroarbeiter & Pendler',
          risk: 'Hohe Mittagsspitzen, Personalengpässe werden teuer.',
          recommendation:
              'Premium-Preis + schnelle Kasse funktionieren hier gut.',
        );
      case LocationPersonality.transit:
        return (
          icon: '🚉',
          audience: 'Pendler & Laufkundschaft',
          risk: 'Wenig Loyalität: Wartezeit kostet sofort Kunden.',
          recommendation: 'Kapazität und günstige Klassiker priorisieren.',
        );
      case LocationPersonality.residential:
        return (
          icon: '🏘️',
          audience: 'Stammkunden & Familien',
          risk: 'Weniger Laufkundschaft, Wachstum braucht Reputation.',
          recommendation:
              'Qualität, faire Preise und lokale Flyer stärken Stammkunden.',
        );
      case LocationPersonality.university:
        return (
          icon: '🎓',
          audience: 'Studierende',
          risk: 'Preissensibel, Rabatte drücken die Marge.',
          recommendation:
              'Combos, Social Media und günstige Dürüm-Angebote testen.',
        );
      case LocationPersonality.nightlife:
        return (
          icon: '🌙',
          audience: 'Nachtschwärmer',
          risk: 'Starke Abendspitzen, schwächere Tagesauslastung.',
          recommendation: 'Späte Öffnung, Getränke und Boxen pushen.',
        );
      case LocationPersonality.touristic:
        return (
          icon: name.toLowerCase().contains('shopping') ? '🛍️' : '📍',
          audience: 'Touristen & gemischte Laufkundschaft',
          risk: 'Teure Lage: Miete muss durch hohen Durchsatz getragen werden.',
          recommendation:
              'Sichtbares Marketing und solide Qualität zahlen sich aus.',
        );
    }
  }

  static Offset _positionFor(int index, int count) {
    const presets = [
      Offset(0.20, 0.68),
      Offset(0.42, 0.42),
      Offset(0.66, 0.62),
      Offset(0.78, 0.30),
      Offset(0.28, 0.24),
      Offset(0.54, 0.78),
    ];
    if (index < presets.length) return presets[index];
    final t = count <= 1 ? 0.0 : index / (count - 1);
    return Offset(
      0.18 + 0.64 * t,
      0.25 + 0.45 * ((index % 2) == 0 ? 1.0 : 0.0),
    );
  }

  static String _slug(String value) => value
      .toLowerCase()
      .replaceAll(RegExp(r'[^a-z0-9äöüß]+'), '_')
      .replaceAll(RegExp(r'_+'), '_')
      .replaceAll(RegExp(r'^_|_$'), '');
}
