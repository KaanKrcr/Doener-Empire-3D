import 'package:doener_empire/core/constants.dart';
import 'package:doener_empire/models/competitor_model.dart';
import 'package:doener_empire/models/difficulty_model.dart';
import 'package:doener_empire/models/game_state.dart';
import 'package:doener_empire/models/product_model.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/services/competitor_engine.dart';
import 'package:doener_empire/services/game_engine.dart';
import 'package:flutter_test/flutter_test.dart';

Shop _shop({double reputation = 3.0}) {
  return Shop(
    id: 'shop_1',
    name: 'Test',
    cityId: 'berlin',
    locationName: 'Bahnhof',
    footTraffic: 26000,
    weeklyRent: 4200,
    menu: kAllProducts
        .where((p) => p.isDefault)
        .map((p) => ShopProduct(productId: p.id, price: p.basePrice))
        .toList(),
    equipment: const [],
    employees: const [],
    reputation: reputation,
    dayOpened: 1,
  );
}

Competitor _competitor(
  String id, {
  CompetitorPersonality personality = CompetitorPersonality.aggressive,
}) {
  return Competitor(
    id: id,
    name: 'Rival $id',
    cityId: 'berlin',
    personality: personality,
    shopCount: 2,
    reputation: 3.4,
    priceLevel: 0.95,
    marketShare: 0.2,
    daysSinceLastAction: 99,
  );
}

GameState _state(
  GameDifficulty difficulty, {
  int competitors = 1,
  CompetitorPersonality personality = CompetitorPersonality.aggressive,
}) {
  return GameState.initial(
    companyName: 'Tester',
    founderName: 'Kaan',
    startCash: 25000,
    difficulty: difficulty,
  ).copyWith(
    shops: [_shop()],
    competitors: List.generate(
      competitors,
      (i) => _competitor('$i', personality: personality),
    ),
  );
}

void main() {
  test('CompetitorActionEvent serialisiert und laedt stabil', () {
    const event = CompetitorActionEvent(
      id: 'event_1',
      day: 7,
      competitorId: 'comp_1',
      competitorName: 'King Doener',
      cityId: 'berlin',
      locationName: 'Bahnhof',
      type: CompetitorActionType.priceWar,
      severity: 0.6,
      message: 'King Doener startet Preiskampf.',
    );

    final loaded = CompetitorActionEvent.fromJson(event.toJson());

    expect(loaded.type, CompetitorActionType.priceWar);
    expect(loaded.locationName, 'Bahnhof');
    expect(loaded.message, contains('Preiskampf'));
  });

  test('Legacy GameState ohne Konkurrenzaktionen laedt mit leerer History', () {
    final json = _state(GameDifficulty.normal).toJson()
      ..remove('recentCompetitorActions');

    final loaded = GameState.fromJson(json);

    expect(loaded.recentCompetitorActions, isEmpty);
  });

  test('Konkurrenzaktionen sind auf impossible haeufiger als auf easy', () {
    final easy = CompetitorEngine.processDayWithActions(
      _state(GameDifficulty.easy, competitors: 200),
    );
    final impossible = CompetitorEngine.processDayWithActions(
      _state(GameDifficulty.impossible, competitors: 200),
    );

    expect(impossible.actions.length, greaterThan(easy.actions.length));
  });

  test('GameEngine speichert neue Konkurrenzaktionen und trimmt alte Events',
      () {
    final oldEvents = List.generate(
      CompetitorEngine.maxRecentActions,
      (i) => CompetitorActionEvent(
        id: 'old_$i',
        day: i,
        competitorId: 'old_comp',
        competitorName: 'Alt Rival',
        cityId: 'berlin',
        type: CompetitorActionType.localMarketing,
        severity: 0.2,
        message: 'Alter Marktbericht.',
      ),
    );
    final state = _state(GameDifficulty.impossible, competitors: 60).copyWith(
      recentCompetitorActions: oldEvents,
    );

    final next = GameEngine.processDay(state);

    expect(
      next.recentCompetitorActions.length,
      lessThanOrEqualTo(CompetitorEngine.maxRecentActions),
    );
    expect(
      next.recentCompetitorActions
          .any((event) => event.day == state.currentDay),
      isTrue,
    );
  });
}
