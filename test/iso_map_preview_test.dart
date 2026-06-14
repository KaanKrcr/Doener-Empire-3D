// Hilfs-"Test", der die Iso-Stadtkarte headless in ein PNG rendert,
// damit man den Look ohne laufendes Gerät begutachten kann.
// Ausgabe: build/iso_map_preview.png
import 'dart:io';
import 'dart:math' as math;
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:doener_empire/ui/widgets/iso_city_map_painter.dart';

void main() {
  testWidgets('render iso city map to PNG', (tester) async {
    const size = Size(640, 900);
    await tester.binding.setSurfaceSize(size);

    final key = GlobalKey();
    final rng = math.Random(42);
    final buildings = <IsoBuilding>[];
    for (var x = 0; x < 7; x++) {
      for (var y = 0; y < 7; y++) {
        if (x == 3 || y == 3) continue; // Straßenreihen freilassen
        buildings.add(IsoBuilding(
          tx: x,
          ty: y,
          floors: 2 + rng.nextInt(7),
          seed: x * 31 + y,
        ));
      }
    }
    buildings.add(const IsoBuilding(tx: 2, ty: 2, floors: 5, seed: 99, hero: true));

    await tester.pumpWidget(
      RepaintBoundary(
        key: key,
        child: Directionality(
          textDirection: TextDirection.ltr,
          child: CustomPaint(
            size: size,
            painter: IsoMapPainter(buildings: buildings),
          ),
        ),
      ),
    );
    await tester.pump();

    await tester.runAsync(() async {
      final boundary =
          key.currentContext!.findRenderObject() as RenderRepaintBoundary;
      final image = await boundary.toImage(pixelRatio: 1.0);
      final bytes = await image.toByteData(format: ui.ImageByteFormat.png);
      final file = File('build/iso_map_preview.png');
      file.writeAsBytesSync(bytes!.buffer.asUint8List());
    });

    expect(File('build/iso_map_preview.png').existsSync(), isTrue);
  });
}
