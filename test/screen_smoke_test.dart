import 'package:doener_empire/core/constants.dart';
import 'package:doener_empire/models/game_state.dart';
import 'package:doener_empire/models/product_model.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/providers/game_provider.dart';
import 'package:doener_empire/ui/main_scaffold.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_riverpod/legacy.dart';
import 'package:flutter_test/flutter_test.dart';

/// Render-Smoke-Tests für Screens mit Laufzeit-Risiko nach den Major-Bumps
/// (fl_chart 1 LineChart im Finanz-Tab, riverpod 3 im Dashboard). `analyze`
/// sieht solche Laufzeit-/Render-Fehler nicht — diese Tests schon.
class _StaticGameNotifier extends GameNotifier {
  _StaticGameNotifier(this.seed);
  final GameState seed;
  @override
  GameState? build() => seed;
}

Shop _shop() => Shop(
      id: 's1',
      name: 'Test Döner',
      cityId: 'berlin',
      locationName: 'Test',
      footTraffic: 50000,
      weeklyRent: 5000,
      menu: kAllProducts
          .where((p) => p.isDefault)
          .map((p) => ShopProduct(productId: p.id, price: p.basePrice))
          .toList(),
      equipment: const [],
      employees: const [],
      dayOpened: 1,
    );

/// 35 Tage History, damit der Finanz-LineChart echte Datenpunkte rendert.
List<DailyRecord> _history() => List.generate(35, (i) {
      final day = i + 1;
      final revenue = 1000.0 + i * 25;
      return DailyRecord(
        day: day,
        revenue: revenue,
        costs: 700.0 + i * 10,
        rentCosts: 100,
        salaryCosts: 200.0 + i,
        ingredientCosts: 300.0 + i * 5,
      );
    });

GameState _seed() => GameState.initial(
      companyName: 'SmokeCo',
      founderName: 'Tester',
      startCash: 80000,
    ).copyWith(
      shops: [_shop()],
      history: _history(),
      currentDay: 36,
    );

Future<void> _pumpTab(WidgetTester tester, int navIndex) async {
  await tester.pumpWidget(
    ProviderScope(
      overrides: [
        gameProvider.overrideWith(() => _StaticGameNotifier(_seed())),
        navIndexProvider.overrideWith((ref) => navIndex),
      ],
      child: const MaterialApp(home: MainScaffold()),
    ),
  );
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('Finanz-Tab rendert (fl_chart LineChart) ohne Exception',
      (tester) async {
    await _pumpTab(tester, 4); // FinanceScreen
    expect(tester.takeException(), isNull);
  });

  testWidgets('Dashboard-Tab rendert ohne Exception', (tester) async {
    await _pumpTab(tester, 0); // DashboardScreen
    expect(tester.takeException(), isNull);
  });
}
