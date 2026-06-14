// Schneller Compile-Guard: stellt sicher, dass die Sprite-Integration in
// city_map_view.dart (und ihre Imports) fehlerfrei kompiliert.
import 'package:flutter_test/flutter_test.dart';

import 'package:doener_empire/ui/widgets/city_map_view.dart';
import 'package:doener_empire/ui/widgets/iso_city_map_canvas.dart';
import 'package:doener_empire/ui/widgets/iso_city_map_painter.dart';

void main() {
  test('citymap widgets compile', () {
    expect(CityMapView, isNotNull);
    expect(IsoCityMapCanvas, isNotNull);
    expect(IsoMapPainter, isNotNull);
    expect(HeroSprite, isNotNull);
  });
}
