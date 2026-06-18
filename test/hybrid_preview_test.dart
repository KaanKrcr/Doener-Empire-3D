// Render-Helfer: rendert HybridMapPreview headless in eine PNG.
// Ausgabe: build/hybrid_preview.png
import 'dart:convert';
import 'dart:io';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:doener_empire/ui/widgets/hybrid_map_preview.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  // ALLE Fonts aus dem FontManifest laden (Inter, Baloo2 UND MaterialIcons),
  // sonst zeichnet die Test-Engine für Text und Icons Platzhalter-Boxen.
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

  testWidgets('render hybrid map preview to PNG', (tester) async {
    const size = Size(720, 1280);
    await tester.binding.setSurfaceSize(size);

    final key = GlobalKey();
    await tester.pumpWidget(
      MaterialApp(
        debugShowCheckedModeBanner: false,
        theme: ThemeData(fontFamily: 'Inter'),
        home: Scaffold(
          body: RepaintBoundary(key: key, child: const HybridMapPreview()),
        ),
      ),
    );

    // Asset real laden (im Test sonst asynchron blockiert).
    await tester.runAsync(() async {
      final ctx = tester.element(find.byType(HybridMapPreview));
      await precacheImage(
        const AssetImage('assets/iso/building_owned.png'),
        ctx,
      );
    });
    await tester.pump();
    await tester.pump(const Duration(milliseconds: 50));

    await tester.runAsync(() async {
      final boundary =
          key.currentContext!.findRenderObject() as RenderRepaintBoundary;
      final image = await boundary.toImage(pixelRatio: 1.0);
      final bytes = await image.toByteData(format: ui.ImageByteFormat.png);
      File('build/hybrid_preview.png')
          .writeAsBytesSync(bytes!.buffer.asUint8List());
    });

    expect(File('build/hybrid_preview.png').existsSync(), isTrue);
  });
}
