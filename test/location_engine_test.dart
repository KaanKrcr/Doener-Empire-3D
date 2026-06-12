import 'package:flutter_test/flutter_test.dart';
import 'package:doener_empire/core/constants.dart';
import 'package:doener_empire/models/competitor_model.dart';
import 'package:doener_empire/models/game_state.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/models/time_profile_model.dart';
import 'package:doener_empire/services/location_engine.dart';

void main() {
  test('City map exposes all configured location templates for a city', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final mapLocations = LocationEngine.locationsFor(city);
    final templates = kLocationTemplates[city.tier]!;

    expect(mapLocations, hasLength(templates.length));
    expect(mapLocations.first.label, templates.first.name);
    expect(mapLocations.first.footTrafficFor(city), greaterThan(0));
    expect(mapLocations.first.weeklyRentFor(city), greaterThan(0));
    expect(
        mapLocations.first.attractivenessScore(city), inInclusiveRange(0, 100));
  });

  test('City map summary aggregates only shops in selected city', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final otherCity = kAllCities.firstWhere((c) => c.id == 'berlin');
    final state = GameState.initial(
      companyName: 'Test Döner',
      founderName: 'Kaan',
      startCash: 50000,
    ).copyWith(
      shops: [
        Shop(
          id: 'fulda-shop',
          name: 'Test Döner',
          cityId: city.id,
          locationName: 'Marktplatz',
          footTraffic: 1200,
          weeklyRent: 900,
          menu: const [],
          equipment: const [],
          employees: const [],
          dayOpened: 1,
          reputation: 4.2,
          personality: LocationPersonality.touristic,
        ),
        Shop(
          id: 'berlin-shop',
          name: 'Test Döner',
          cityId: otherCity.id,
          locationName: 'Top-Lage Mitte',
          footTraffic: 9000,
          weeklyRent: 7000,
          menu: const [],
          equipment: const [],
          employees: const [],
          dayOpened: 1,
          reputation: 2.0,
          personality: LocationPersonality.touristic,
        ),
      ],
    );

    final summary = LocationEngine.summarize(city, state.shops);

    expect(summary.shopCount, 1);
    expect(summary.totalFootTraffic, 1200);
    expect(summary.weeklyRent, 900);
    expect(summary.avgReputation, 4.2);
  });

  test('Competition brief exposes rival pressure for selected city', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final otherCity = kAllCities.firstWhere((c) => c.id == 'berlin');
    final state = GameState.initial(
      companyName: 'Test Doener',
      founderName: 'Kaan',
      startCash: 50000,
    ).copyWith(
      competitors: [
        Competitor(
          id: 'c1',
          name: 'Rival Grill',
          cityId: city.id,
          personality: CompetitorPersonality.aggressive,
          shopCount: 3,
          marketShare: 0.35,
        ),
        Competitor(
          id: 'c2',
          name: 'Other City Grill',
          cityId: otherCity.id,
          personality: CompetitorPersonality.balanced,
          shopCount: 5,
          marketShare: 0.80,
        ),
      ],
    );

    final brief = LocationEngine.competitionBrief(state, city.id);

    expect(brief.rivalCount, 1);
    expect(brief.rivalShopCount, 3);
    expect(brief.rivalMarketShare, 0.35);
    expect(brief.strongestRival?.name, 'Rival Grill');
    expect(brief.pressureLabel, 'Spuerbar');
    expect(brief.recommendation, contains('Cash-Reserve'));
  });

  test('Opening forecast provides decision metrics for a map location', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final location = LocationEngine.locationsFor(city).first;

    final forecast = LocationEngine.forecastOpening(city, location);

    expect(forecast.estimatedCustomersPerDay, greaterThan(0));
    expect(forecast.estimatedProfitPerDay.isFinite, isTrue);
    if (forecast.isProfitable) {
      expect(forecast.breakEvenDays, isNotNull);
      expect(forecast.breakEvenDays!, greaterThan(0));
    }
  });

  test('Decision brief blocks openings without enough capital', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final location = LocationEngine.locationsFor(city).first;
    const competition = CityCompetitionBrief(
      rivalCount: 0,
      rivalShopCount: 0,
      rivalMarketShare: 0,
      strongestRival: null,
    );

    final decision = LocationEngine.decisionBrief(
      city: city,
      location: location,
      cash: 0,
      competition: competition,
    );

    expect(decision.recommended, isFalse);
    expect(decision.label, 'Warten');
    expect(decision.nextStep, contains('Cash'));
  });

  test('Decision brief warns when hard rivals leave too little reserve', () {
    final city = kAllCities.firstWhere((c) => c.id == 'fulda');
    final location = LocationEngine.locationsFor(city).first;
    final deposit = location.depositFor(city);
    final rival = Competitor(
      id: 'rival',
      name: 'Rival Grill',
      cityId: city.id,
      personality: CompetitorPersonality.aggressive,
      shopCount: 5,
      marketShare: 0.70,
    );
    final competition = CityCompetitionBrief(
      rivalCount: 1,
      rivalShopCount: 5,
      rivalMarketShare: 0.70,
      strongestRival: rival,
    );

    final decision = LocationEngine.decisionBrief(
      city: city,
      location: location,
      cash: deposit * 1.2,
      competition: competition,
    );

    expect(decision.recommended, isFalse);
    expect(decision.label, 'Nur mit Reserve');
    expect(decision.reason, contains('Rival Grill'));
  });
}
