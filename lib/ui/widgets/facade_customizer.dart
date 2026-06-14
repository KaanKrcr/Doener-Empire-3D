import 'package:flutter/material.dart';

import 'building_styles.dart';

/// Bottom-Sheet, mit dem der Spieler die Akzentfarbe seiner eigenen Filiale
/// wählt. Reines Wählen — die Persistenz übernimmt der Aufrufer.
class FacadeCustomizer {
  const FacadeCustomizer._();

  static const List<int> swatches = [
    0xFFF5A623, // Döner-Orange
    0xFFD46816, // Gold
    0xFFE74C3C, // Rot
    0xFF7BC950, // Grün
    0xFF4A90D9, // Blau
    0xFF1ABC9C, // Türkis
    0xFF9B59B6, // Violett
    0xFFE84393, // Pink
    0xFFFFFAE6, // Creme
    0xFF5D6D7E, // Schiefer
  ];

  static Future<void> show(
    BuildContext context, {
    required int currentColor,
    required ValueChanged<int> onPick,
  }) {
    return showModalBottomSheet<void>(
      context: context,
      backgroundColor: MapPalette.bgPanel,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (ctx) => SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Filial-Farbe wählen',
                style: TextStyle(
                  fontFamily: 'Baloo2',
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  color: MapPalette.textMain,
                ),
              ),
              const SizedBox(height: 4),
              const Text(
                'Die Akzentfarbe färbt Schild, Markise und Neon-Kontur.',
                style: TextStyle(fontSize: 12, color: MapPalette.textMuted),
              ),
              const SizedBox(height: 16),
              Wrap(
                spacing: 14,
                runSpacing: 14,
                children: [
                  for (final argb in swatches)
                    _Swatch(
                      argb: argb,
                      selected: argb == currentColor,
                      onTap: () {
                        onPick(argb);
                        Navigator.of(ctx).pop();
                      },
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Swatch extends StatelessWidget {
  final int argb;
  final bool selected;
  final VoidCallback onTap;

  const _Swatch({
    required this.argb,
    required this.selected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 44,
        height: 44,
        decoration: BoxDecoration(
          color: Color(argb),
          shape: BoxShape.circle,
          border: Border.all(
            color: selected ? MapPalette.textMain : MapPalette.bgDeep,
            width: selected ? 3 : 1.5,
          ),
          boxShadow: [
            BoxShadow(
              color: Color(argb).withAlpha(120),
              blurRadius: selected ? 10 : 4,
              spreadRadius: selected ? 1 : 0,
            ),
          ],
        ),
        child: selected
            ? const Icon(Icons.check, size: 20, color: Colors.black87)
            : null,
      ),
    );
  }
}
