import 'package:flutter/services.dart';

/// Test-/headless-sichere Haptik.
///
/// `HapticFeedback.*` liefert Futures, die ohne Plattform-Plugin (z.B. in
/// Widget-Tests) **asynchron** werfen. Ungekapselt entstehen daraus
/// unbehandelte async-Fehler (sporadische Test-Failures) — analog zu
/// [SoundService]. Hier ist jeder Aufruf mit `.catchError` gekapselt.
class Haptics {
  Haptics._();

  static void selection() => HapticFeedback.selectionClick().catchError((_) {});
  static void light() => HapticFeedback.lightImpact().catchError((_) {});
  static void medium() => HapticFeedback.mediumImpact().catchError((_) {});
  static void heavy() => HapticFeedback.heavyImpact().catchError((_) {});
}
