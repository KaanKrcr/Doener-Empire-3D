import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:doener_empire/models/game_state.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/providers/game_provider.dart';
import 'package:doener_empire/services/sound_service.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUp(() {
    SharedPreferences.setMockInitialValues({});
    SoundService.enabled = false;
  });

  test('endDay über 35 Tage läuft stabil (Steuer/Challenge/Reports)', () async {
    final container = ProviderContainer();
    addTearDown(container.dispose);
    final notifier = container.read(gameProvider.notifier);

    await notifier.startNewGame('Test Döner', 'Kaan');
    notifier.stopTimers(); // kein periodischer Timer im Test

    // Filiale eröffnen (bekommt Default-Menü)
    notifier.openShop(const Shop(
      id: 's1',
      name: 'Test Döner',
      cityId: 'fulda',
      locationName: 'Marktplatz',
      footTraffic: 1800,
      weeklyRent: 500,
      menu: [],
      equipment: [],
      employees: [],
      dayOpened: 1,
    ));

    // 35 Tage abschließen — überschreitet Wochen- (7) und Steuer-Grenze (30)
    for (var i = 0; i < 35; i++) {
      notifier.endDay();
    }

    final state = container.read(gameProvider)!;
    expect(state.currentDay, 36); // Start 1 + 35
    expect(state.cash.isFinite, isTrue);
    expect(notifier.lastDayResult, isNotNull);
    // Die Filiale existiert weiterhin
    expect(state.shops.length, 1);
  });

  test('Day-End-Ursachenbericht erkennt fehlende Filialen', () {
    final state = GameState.initial(
      companyName: 'Test Döner',
      founderName: 'Kaan',
      startCash: 50000,
    );

    final brief = GameNotifier.buildDayEndCauseBrief(
      state: state,
      revenue: 0,
      costs: 0,
      loanPayments: 0,
      customers: 0,
    );

    expect(brief.headline, 'Noch kein Verkauf');
    expect(brief.nextAction, contains('Eröffne'));
    expect(brief.tone, DayEndCauseTone.warning);
  });

  test('Day-End-Ursachenbericht priorisiert Kreditdruck bei Verlust', () {
    final state = GameState.initial(
      companyName: 'Test Döner',
      founderName: 'Kaan',
      startCash: 50000,
    ).copyWith(
      shops: const [
        Shop(
          id: 's1',
          name: 'Test Döner',
          cityId: 'fulda',
          locationName: 'Marktplatz',
          footTraffic: 1800,
          weeklyRent: 500,
          menu: [],
          equipment: [],
          employees: [],
          dayOpened: 1,
        ),
      ],
    );

    final brief = GameNotifier.buildDayEndCauseBrief(
      state: state,
      revenue: 500,
      costs: 300,
      loanPayments: 250,
      customers: 20,
    );

    expect(brief.headline, 'Kreditraten drücken den Tag');
    expect(brief.tone, DayEndCauseTone.danger);
    expect(brief.nextAction, contains('Cash-Reserve'));
  });
}
