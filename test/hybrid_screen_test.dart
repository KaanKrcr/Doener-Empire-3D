// Render-Helfer: rendert HybridShopScreen headless in eine PNG.
// Ausgabe: build/hybrid_screen.png
import 'dart:convert';
import 'dart:io';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:doener_empire/core/constants.dart';
import 'package:doener_empire/models/game_state.dart';
import 'package:doener_empire/models/product_model.dart';
import 'package:doener_empire/models/shop_model.dart';
import 'package:doener_empire/providers/game_provider.dart';
import 'package:doener_empire/ui/widgets/hybrid_shop_screen.dart';

class _StaticGameNotifier extends GameNotifier {
  _StaticGameNotifier(this.seed);
  final GameState seed;
  @override
  GameState? build() => seed;
}

Shop _shop() => Shop(
      id: 's_berlin_mitte',
      name: 'Test Döner',
      cityId: 'berlin',
      locationName: 'Hauptstrasse 12',
      footTraffic: 1842,
      weeklyRent: 8750,
      menu: kAllProducts
          .where((p) => p.isDefault)
          .map((p) => ShopProduct(productId: p.id, price: p.basePrice))
          .toList(),
      equipment: const [],
      employees: const [],
      reputation: 4.6,
      dayOpened: 1,
    );

GameState _seed() => GameState.initial(
      companyName: 'Test Döner GmbH',
      founderName: 'Tester',
      startCash: 1248750,
      tutorialEnabled: false,
    ).copyWith(
      shops: [_shop()],
      currentDay: 47,
    );

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() async {
    final manifest =
        json.decode(await rootBundle.loadString('FontManifest.json'))
            as List<dynamic>;
    for (final font in manifest) {
      final loader = FontLoader(font['family'] as String);
      for (final f in (font['fonts'] as List<dynamic>)) {
        loader.addFont(rootBundle.load(f['asset'] as String));
      }
      await loader.load();
    }
  });

  testWidgets('render hybrid shop screen to PNG', (tester) async {
    const size = Size(760, 1500);
    await tester.binding.setSurfaceSize(size);

    final key = GlobalKey();
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          gameProvider.overrideWith(() => _StaticGameNotifier(_seed())),
        ],
        child: MaterialApp(
          debugShowCheckedModeBanner: false,
          theme: ThemeData(fontFamily: 'Inter'),
          home: Scaffold(
            body: RepaintBoundary(
              key: key,
              child: const HybridShopScreen(shopId: 's_berlin_mitte'),
            ),
          ),
        ),
      ),
    );

    await tester.runAsync(() async {
      final ctx = tester.element(find.byType(HybridShopScreen));
      await precacheImage(
          const AssetImage('assets/iso/building_owned.png'), ctx);
    });
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 50));

    await tester.runAsync(() async {
      final boundary =
          key.currentContext!.findRenderObject() as RenderRepaintBoundary;
      final image = await boundary.toImage(pixelRatio: 1.0);
      final bytes = await image.toByteData(format: ui.ImageByteFormat.png);
      File('build/hybrid_screen.png')
          .writeAsBytesSync(bytes!.buffer.asUint8List());
    });

    expect(File('build/hybrid_screen.png').existsSync(), isTrue);
  });
}
