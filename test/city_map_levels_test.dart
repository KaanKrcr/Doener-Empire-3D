import 'package:doener_empire/core/constants.dart';
import 'package:doener_empire/models/building_style_model.dart';
import 'package:doener_empire/models/competitor_look.dart';
import 'package:doener_empire/models/competitor_model.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/services/location_engine.dart';
import 'package:doener_empire/ui/widgets/building_styles.dart';
import 'package:doener_empire/ui/widgets/map_city_overview.dart';
import 'package:doener_empire/ui/widgets/map_deutschland.dart';
import 'package:doener_empire/ui/widgets/map_street_view.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

Shop _shop({
  String location = 'Marktplatz',
  double reputation = 3.0,
  int accent = 0xFFF5A623,
  List<String> upgrades = const [],
}) =>
    Shop(
      id: 'p1',
      name: 'Test Döner',
      cityId: 'augsburg',
      locationName: location,
      footTraffic: 5000,
      weeklyRent: 1500,
      menu: const [],
      equipment: const [],
      employees: const [],
      reputation: reputation,
      dayOpened: 1,
      upgradeIds: upgrades,
      accentColor: accent,
    );

Competitor _comp(String id, CompetitorPersonality p) => Competitor(
      id: id,
      name: 'Rivale $id',
      cityId: 'augsburg',
      personality: p,
    );

void main() {
  final city = kAllCities.firstWhere((c) => c.id == 'augsburg');
  final location = LocationEngine.locationsFor(city).first;

  group('playerBuildingStyle', () {
    test('schäbig bei niedrigem Ruf, premium bei hohem Ruf', () {
      expect(playerBuildingStyle(_shop(reputation: 2.0)).condition,
          BuildingCondition.worn);
      expect(playerBuildingStyle(_shop(reputation: 3.5)).condition,
          BuildingCondition.normal);
      expect(playerBuildingStyle(_shop(reputation: 4.6)).condition,
          BuildingCondition.premium);
    });

    test('Upgrades verbessern Zustand und Stockwerke', () {
      final base = playerBuildingStyle(_shop(reputation: 3.0));
      final upgraded = playerBuildingStyle(
          _shop(reputation: 3.0, upgrades: ['wlan', 'klima', 'musik']));
      expect(upgraded.floors, greaterThanOrEqualTo(base.floors));
      expect(upgraded.condition.index, greaterThanOrEqualTo(base.condition.index));
    });

    test('übernimmt die gewählte Akzentfarbe', () {
      final style = playerBuildingStyle(_shop(accent: 0xFF4A90D9));
      expect(style.accentColor, const Color(0xFF4A90D9));
    });
  });

  group('CompetitorLook', () {
    test('ist deterministisch pro Konkurrent', () {
      final c = _comp('rival_x', CompetitorPersonality.premium);
      final a = CompetitorLook.fromCompetitor(c);
      final b = CompetitorLook.fromCompetitor(c);
      expect(a.style.roofType, b.style.roofType);
      expect(a.style.facadeType, b.style.facadeType);
      expect(a.signText, c.name);
    });

    test('unterscheidet sich je Persönlichkeit', () {
      final cheap = CompetitorLook.fromCompetitor(
          _comp('a', CompetitorPersonality.cheapMass));
      final premium = CompetitorLook.fromCompetitor(
          _comp('a', CompetitorPersonality.premium));
      expect(cheap.style.facadeType, isNot(premium.style.facadeType));
      expect(cheap.style.roofType, isNot(premium.style.roofType));
    });
  });

  group('fillerBuildingStyle', () {
    test('ist deterministisch pro Seed', () {
      expect(fillerBuildingStyle(42).roofType, fillerBuildingStyle(42).roofType);
    });

    test('liefert sichtbare Variation über Seeds', () {
      final roofs =
          List.generate(8, (i) => fillerBuildingStyle(i).roofType).toSet();
      expect(roofs.length, greaterThan(1));
    });
  });

  group('buildStreetScene', () {
    test('eigenes Gebäude steht zentral vorne, 6 Häuser gesamt', () {
      final scene = buildStreetScene(
        location: location,
        playerShop: _shop(location: location.template.name),
        competitors: [_comp('c1', CompetitorPersonality.balanced)],
      );
      expect(scene.near.length, 3);
      expect(scene.far.length, 3);
      expect(scene.all.length, 6);
      expect(scene.near[1].kind, StreetBuildingKind.player);
    });

    test('ohne eigene Filiale ist die Mitte ein freier Bauplatz', () {
      final scene = buildStreetScene(
        location: location,
        playerShop: null,
        competitors: const [],
      );
      expect(scene.near[1].kind, StreetBuildingKind.freePlot);
    });

    test('platziert Konkurrenten als eigene Gebäude', () {
      final scene = buildStreetScene(
        location: location,
        playerShop: _shop(location: location.template.name),
        competitors: [
          _comp('c1', CompetitorPersonality.cheapMass),
          _comp('c2', CompetitorPersonality.premium),
        ],
      );
      final compCount = scene.all
          .where((b) => b.kind == StreetBuildingKind.competitor)
          .length;
      expect(compCount, 2);
    });
  });

  group('Render-Smoke (Painter werfen keine Exception)', () {
    Future<void> pump(WidgetTester tester, Widget child) async {
      await tester.pumpWidget(MaterialApp(
        home: Scaffold(
          body: SizedBox(width: 390, height: 560, child: child),
        ),
      ));
      await tester.pump();
    }

    testWidgets('MapDeutschland', (tester) async {
      await pump(
        tester,
        MapDeutschland(
          unlockedCityIds: const {'augsburg', 'fulda'},
          cityIdsWithShops: const {'augsburg'},
          onSelectCity: (_) {},
        ),
      );
      expect(tester.takeException(), isNull);
    });

    testWidgets('MapCityOverview', (tester) async {
      await pump(
        tester,
        MapCityOverview(
          city: city,
          locations: LocationEngine.locationsFor(city),
          cityShops: [_shop(location: location.template.name)],
          competitors: [
            _comp('c1', CompetitorPersonality.cheapMass),
            _comp('c2', CompetitorPersonality.premium),
          ],
          onEnterLocation: (_) {},
        ),
      );
      expect(tester.takeException(), isNull);
    });

    testWidgets('MapStreetView mit Hero + Konkurrenz', (tester) async {
      await pump(
        tester,
        MapStreetView(
          city: city,
          location: location,
          playerShop: _shop(location: location.template.name),
          competitors: [_comp('c1', CompetitorPersonality.aggressive)],
          cash: 50000,
          onManage: (_) {},
          onCustomize: (_) {},
          onOpenFree: (_) {},
          onAcquire: (_) {},
        ),
      );
      expect(tester.takeException(), isNull);
    });
  });
}
