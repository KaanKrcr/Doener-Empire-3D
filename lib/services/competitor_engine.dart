import 'dart:math';

import '../models/competitor_model.dart';
import '../models/difficulty_model.dart';
import '../models/game_state.dart';
import '../core/constants.dart';
import '../models/city_model.dart';

/// Steuert das Verhalten der KI-Konkurrenten.
///
/// - Beim ersten Eröffnen einer Filiale in einer Stadt werden 1–3 Konkurrenten gespawnt.
/// - Täglich (oder alle paar Tage) reagieren die Konkurrenten:
///     * Aggressive eröffnen neue Filialen
///     * Premium passen Preise nach oben
///     * Günstige passen Preise nach unten
/// - Marktanteile werden täglich auf Basis von Reputation + Preisniveau neu verteilt.
class CompetitorEngine {
  static final _rng = Random();
  static const int maxRecentActions = 20;

  /// Liefert Konkurrenten für eine Stadt — spawnt sie wenn noch keine da sind.
  /// Sollte beim ersten Eröffnen einer Filiale in der Stadt aufgerufen werden.
  static List<Competitor> ensureCompetitorsForCity(
    List<Competitor> existing,
    String cityId,
    GameDifficulty difficulty,
  ) {
    final existingInCity = existing.where((c) => c.cityId == cityId).toList();
    if (existingInCity.isNotEmpty) return existing;

    final city = kAllCities.firstWhere(
      (c) => c.id == cityId,
      orElse: () => kAllCities.first,
    );

    // Wie viele Konkurrenten je nach Stadtgröße?
    int count;
    switch (city.tier) {
      case CityTier.klein:
        count = 1;
        break;
      case CityTier.mittel:
        count = 2;
        break;
      case CityTier.gross:
        count = 3;
        break;
      case CityTier.metropole:
        count = 3 + _rng.nextInt(2);
        break;
    }
    final spawnBonus = difficulty.modifiers.competitorAggressivenessMultiplier;
    final extra = ((spawnBonus - 1.0) * 2).round().clamp(0, 2);
    count += extra;

    final newCompetitors = <Competitor>[];
    for (var i = 0; i < count; i++) {
      newCompetitors.add(CompetitorFactory.create(
        id: 'comp_${cityId}_${DateTime.now().microsecondsSinceEpoch}_$i',
        cityId: cityId,
      ));
    }

    return [...existing, ...newCompetitors];
  }

  /// Tägliches Update aller Konkurrenten + Marktanteils-Berechnung.
  /// Wird in [GameEngine.processDay] aufgerufen.
  static List<Competitor> processDay(GameState state) {
    return processDayWithActions(state).competitors;
  }

  /// Tagesupdate inklusive sichtbarer Rivalen-Aktionen.
  static CompetitorDayResult processDayWithActions(GameState state) {
    final aggressiveness =
        state.difficulty.modifiers.competitorAggressivenessMultiplier;
    final actions = <CompetitorActionEvent>[];
    final updated = state.competitors.map((c) {
      c.daysSinceLastAction += 1;
      final action = _maybeAct(c, state, aggressiveness);
      if (action != null) actions.add(action);
      return c;
    }).toList();

    // Marktanteile neu berechnen pro Stadt
    final byCity = <String, List<Competitor>>{};
    for (final c in updated) {
      byCity.putIfAbsent(c.cityId, () => []).add(c);
    }
    byCity.forEach((cityId, list) {
      _recomputeMarketShares(list, state, cityId);
    });

    return CompetitorDayResult(competitors: updated, actions: actions);
  }

  /// Wie stark drückt die Konkurrenz auf eine Spieler-Filiale in dieser Stadt?
  /// Liefert 0.6..1.05 — kleiner = mehr Druck.
  static double competitionPressure(
      GameState state, String cityId, double playerShopRep) {
    final inCity = state.competitorsIn(cityId);
    if (inCity.isEmpty) return 1.0;
    final aggressiveness =
        state.difficulty.modifiers.competitorAggressivenessMultiplier;

    // Wenn der Spieler im Vergleich schlechte Rep hat → Druck.
    double avgRivalRep =
        inCity.fold(0.0, (s, c) => s + c.reputation) / inCity.length;
    final repDelta = playerShopRep - avgRivalRep; // positiv = Spieler besser

    // Konkurrenz-Anzahl × Stärke
    final density = inCity.fold(0, (s, c) => s + c.shopCount) / 3.0;

    // Basis-Druck steigt mit Density
    double pressure = 1.0 - (density * 0.05 * aggressiveness);
    // Reputations-Bonus / Malus
    final defenseFactor = (1 / aggressiveness).clamp(0.6, 1.4);
    pressure += repDelta * 0.04 * defenseFactor;

    return pressure.clamp(0.55, 1.10);
  }

  // ── Private ──────────────────────────────────────────────────────────────

  /// Konkurrent macht ggf. eine Aktion (Preiskampf, Expansion, Rep-Update).
  static CompetitorActionEvent? _maybeAct(
    Competitor c,
    GameState state,
    double aggressiveness,
  ) {
    final minDays = (5 / aggressiveness).round().clamp(2, 9);
    if (c.daysSinceLastAction < minDays) return null;

    final baseActionChance = switch (c.personality) {
      CompetitorPersonality.aggressive => 0.40,
      CompetitorPersonality.cheapMass => 0.25,
      CompetitorPersonality.balanced => 0.18,
      CompetitorPersonality.premium => 0.15,
      CompetitorPersonality.traditional => 0.10,
    };
    final actionChance = (baseActionChance * aggressiveness).clamp(0.05, 0.90);
    if (_rng.nextDouble() > actionChance) return null;

    c.daysSinceLastAction = 0;
    final targetLocationName = _targetLocationName(c, state);
    late final CompetitorActionType actionType;

    // Welche Aktion?
    final r = _rng.nextDouble();
    final expansionChance = (0.30 * aggressiveness).clamp(0.15, 0.55);
    final priceChance =
        (0.30 + (aggressiveness - 1.0) * 0.10).clamp(0.20, 0.50);
    if (r < expansionChance && c.shopCount < 5) {
      // Expansion
      c.shopCount = (c.shopCount + 1).clamp(1, 5);
      c.reputation = (c.reputation - 0.05).clamp(1.0, 5.0); // dilution
      actionType = CompetitorActionType.expansion;
    } else if (r < expansionChance + priceChance) {
      // Preis-Anpassung
      final hasPlayer = state.hasShopIn(c.cityId);
      if (c.personality == CompetitorPersonality.aggressive && hasPlayer) {
        c.priceLevel = (c.priceLevel - 0.05).clamp(0.65, 1.4);
        actionType = CompetitorActionType.priceWar;
      } else if (c.personality == CompetitorPersonality.premium) {
        c.priceLevel = (c.priceLevel + 0.04).clamp(0.65, 1.4);
        actionType = CompetitorActionType.qualityPush;
      } else if (c.personality == CompetitorPersonality.cheapMass) {
        c.priceLevel = (c.priceLevel - 0.02).clamp(0.65, 1.4);
        actionType = CompetitorActionType.priceWar;
      } else {
        // balanced/traditional: minimal jiggle
        c.priceLevel =
            (c.priceLevel + (_rng.nextDouble() - 0.5) * 0.04).clamp(0.65, 1.4);
        actionType = CompetitorActionType.localMarketing;
      }
    } else {
      // Reputations-Pflege oder -Schwächung
      final delta = (_rng.nextDouble() - 0.45) * 0.20;
      c.reputation = (c.reputation + delta).clamp(1.0, 5.0);
      actionType = c.personality == CompetitorPersonality.premium
          ? CompetitorActionType.qualityPush
          : CompetitorActionType.localMarketing;
    }

    final severity = (actionChance * aggressiveness).clamp(0.10, 1.0);
    return CompetitorActionEvent(
      id: 'comp_action_${state.currentDay}_${c.id}_${actionType.name}',
      day: state.currentDay,
      competitorId: c.id,
      competitorName: c.name,
      cityId: c.cityId,
      locationName: targetLocationName,
      type: actionType,
      severity: severity,
      message: _actionMessage(c, actionType, targetLocationName),
    );
  }

  static String? _targetLocationName(Competitor c, GameState state) {
    final cityShops = state.shops.where((s) => s.cityId == c.cityId).toList();
    if (cityShops.isEmpty) return null;
    cityShops.sort((a, b) => a.reputation.compareTo(b.reputation));
    return cityShops.first.locationName;
  }

  static String _actionMessage(
    Competitor competitor,
    CompetitorActionType type,
    String? locationName,
  ) {
    final target = locationName == null ? '' : ' bei $locationName';
    switch (type) {
      case CompetitorActionType.expansion:
        return '${competitor.name} expandiert$target: Standortdruck steigt.';
      case CompetitorActionType.priceWar:
        return '${competitor.name} startet Preiskampf$target: Preis und Tempo pruefen.';
      case CompetitorActionType.qualityPush:
        return '${competitor.name} setzt auf Qualitaet$target: Reputation wird wichtiger.';
      case CompetitorActionType.localMarketing:
        return '${competitor.name} wirbt lokal$target: Stammkunden verteidigen.';
    }
  }

  /// Verteilt die Marktanteile auf Konkurrenten + Spieler-Anteil basierend auf
  /// Reputation × Preisniveau × Filialdichte. Spieler ist separat — die
  /// Konkurrenten-Anteile addieren sich nicht auf 1.0, sondern auf 1.0 - playerShare.
  static void _recomputeMarketShares(
    List<Competitor> competitors,
    GameState state,
    String cityId,
  ) {
    if (competitors.isEmpty) return;
    final aggressiveness =
        state.difficulty.modifiers.competitorAggressivenessMultiplier;

    // Spieler-Power in dieser Stadt
    final playerShops = state.shops.where((s) => s.cityId == cityId).toList();
    final playerPower = playerShops.fold(0.0, (sum, s) {
      final repScore = s.reputation / 5.0; // 0..1
      const priceScore = 1.0; // im einfachen Modell neutral
      return sum + repScore * priceScore;
    });

    // Konkurrenten-Power
    final compPower = competitors.fold(0.0, (sum, c) {
      // niedrigerer Preis = etwas mehr Power
      final priceScore = (2.0 - c.priceLevel).clamp(0.5, 1.5);
      return sum +
          (c.reputation / 5.0) * priceScore * c.shopCount * aggressiveness;
    });

    final totalPower = playerPower + compPower;
    if (totalPower <= 0) return;

    for (final c in competitors) {
      final priceScore = (2.0 - c.priceLevel).clamp(0.5, 1.5);
      final p =
          (c.reputation / 5.0) * priceScore * c.shopCount * aggressiveness;
      c.marketShare = (p / totalPower).clamp(0.0, 1.0);
    }
  }
}

class CompetitorDayResult {
  final List<Competitor> competitors;
  final List<CompetitorActionEvent> actions;

  const CompetitorDayResult({
    required this.competitors,
    required this.actions,
  });
}
