// Render-Helfer: rendert IsoCityMapCanvas (Vektor-Feld + Hero-Sprite) → PNG.
// Ausgabe: build/iso_canvas.png
import 'dart:convert';
import 'dart:io';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:doener_empire/ui/widgets/iso_city_map_canvas.dart';
import 'package:doener_empire/ui/widgets/iso_city_map_painter.dart';

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

  testWidgets('render iso city map canvas to PNG', (tester) async {
    const size = Size(1000, 820);
    await tester.binding.setSurfaceSize(size);

    // Vektor-Nachbarn + eine Hero-Filiale (body:false → Sprite-Slot).
    final buildings = <IsoBuilding>[];
    for (var x = 0; x < 5; x++) {
      for (var y = 0; y < 5; y++) {
        if (x == 2 && y == 2) continue;
        buildings.add(IsoBuilding(
            tx: x, ty: y, floors: 3 + ((x + y) % 4), seed: x * 13 + y));
      }
    }
    const hero = IsoBuilding(tx: 2, ty: 2, floors: 4, hero: true, body: false);
    buildings.add(hero);

    final key = GlobalKey();
    await tester.pumpWidget(
      MaterialApp(
        debugShowCheckedModeBanner: false,
        theme: ThemeData(fontFamily: 'Inter'),
        home: Scaffold(
          backgroundColor: const Color(0xFF07080A),
          body: RepaintBoundary(
            key: key,
            child: Center(
              child: IsoCityMapCanvas(
                buildings: buildings,
                scene: size,
                spriteWidth: 250,
                heroes: const [
                  HeroSprite(
                    building: hero,
                    asset: 'assets/iso/building_owned.png',
                    label: 'HAUPTSTRASSE 12',
                    rating: 4.6,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );

    await tester.runAsync(() async {
      final ctx = tester.element(find.byType(IsoCityMapCanvas));
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
      File('build/iso_canvas.png')
          .writeAsBytesSync(bytes!.buffer.asUint8List());
    });

    expect(File('build/iso_canvas.png').existsSync(), isTrue);
  });
}
