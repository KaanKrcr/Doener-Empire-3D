import 'dart:math' as math;

import 'package:flutter/material.dart';

import '../../models/building_style_model.dart';

/// Zeichnet EIN isometrisches Gebäude komplett aus [BuildingStyle]-Parametern
/// — keine Texturen/Bitmaps. Optik nach dem mobilen Art-Direction-Referenzbild:
/// Anthrazit-Fassaden mit vertikalem Verlauf, warm leuchtende Fenster mit Glow,
/// Flachdach mit Lüftungskästen, Iso-Bodenschatten. Die Hero-Filiale erhält
/// Neon-Outline, Boden-Glow und einen Döner-Pin.
class StreetBuildingPainter extends CustomPainter {
  final BuildingStyle style;
  final bool isPlayer;
  final bool isSelected;
  final int seed;

  const StreetBuildingPainter({
    required this.style,
    this.isPlayer = false,
    this.isSelected = false,
    this.seed = 0,
  });

  // Deterministische Pseudo-Zufallszahl 0..1.
  double _rng(int n) {
    final x = math.sin((n + seed) * 12.9898) * 43758.5453;
    return x - x.floorToDouble();
  }

  Offset _facePoint(Offset origin, Offset uVec, double wallH, double u, double v) {
    return Offset(origin.dx + uVec.dx * u, origin.dy + uVec.dy * u - wallH * v);
  }

  @override
  void paint(Canvas canvas, Size size) {
    final w = size.width;
    final h = size.height;
    if (w <= 8 || h <= 8) return;

    final floors = style.floors.clamp(1, 6);
    final cx = w * 0.5;
    final hw = (math.min(w, h) * 0.32).clamp(8.0, w * 0.46);
    final dv = hw * 0.5;

    final gy = h - dv - h * 0.05; // Front-Bodenpunkt sitzt knapp über dem Rand
    final floorH = (h * 0.6 / 6).clamp(7.0, 22.0);
    var wallH = floorH * floors + h * 0.05;
    final maxWallH = gy - dv - h * 0.16;
    wallH = wallH.clamp(h * 0.24, math.max(h * 0.24, maxWallH));

    // Diamant-Eckpunkte am Boden
    final gT = Offset(cx, gy - dv);
    final gR = Offset(cx + hw, gy);
    final gB = Offset(cx, gy + dv);
    final gL = Offset(cx - hw, gy);
    // … und am Dach
    final rT = gT.translate(0, -wallH);
    final rR = gR.translate(0, -wallH);
    final rB = gB.translate(0, -wallH);
    final rL = gL.translate(0, -wallH);

    // ── Iso-Bodenschatten ──────────────────────────────────────────────
    canvas.save();
    canvas.translate(hw * 0.16, dv * 0.45);
    canvas.drawPath(
      Path()
        ..moveTo(gT.dx, gT.dy)
        ..lineTo(gR.dx, gR.dy)
        ..lineTo(gB.dx, gB.dy)
        ..lineTo(gL.dx, gL.dy)
        ..close(),
      Paint()
        ..color = const Color(0x66000000)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 10),
    );
    canvas.restore();

    // ── Boden-Glow (nur Hero) ───────────────────────────────────────────
    if (isPlayer) {
      final glowRect = Rect.fromCircle(center: gB, radius: hw * 2.0);
      canvas.drawCircle(
        gB,
        hw * 2.0,
        Paint()
          ..shader = RadialGradient(
            colors: [style.accentColor.withAlpha(102), style.accentColor.withAlpha(0)],
          ).createShader(glowRect),
      );
    }

    // ── Linke Wandfläche (heller) ──────────────────────────────────────
    final leftFace = Path()
      ..moveTo(gL.dx, gL.dy)
      ..lineTo(gB.dx, gB.dy)
      ..lineTo(rB.dx, rB.dy)
      ..lineTo(rL.dx, rL.dy)
      ..close();
    _fillFaceGradient(canvas, leftFace, const Color(0xFF1C1F25), const Color(0xFF0E1014));

    // ── Rechte Wandfläche (dunkler) ────────────────────────────────────
    final rightFace = Path()
      ..moveTo(gB.dx, gB.dy)
      ..lineTo(gR.dx, gR.dy)
      ..lineTo(rR.dx, rR.dy)
      ..lineTo(rB.dx, rB.dy)
      ..close();
    _fillFaceGradient(canvas, rightFace, const Color(0xFF15181E), const Color(0xFF101216));

    // ── Fenster ────────────────────────────────────────────────────────
    if (style.windowPattern != WindowPattern.none) {
      _windows(canvas, gL, gB, wallH, floors, 1);
      _windows(canvas, gB, gR, wallH, floors, 2);
    }

    // ── Schild über der Tür (Front-Ecke) ──────────────────────────────
    if (style.hasSign) _sign(canvas, gB, gL, gR, wallH, floorH);

    // ── Flachdach ──────────────────────────────────────────────────────
    final roof = Path()
      ..moveTo(rT.dx, rT.dy)
      ..lineTo(rR.dx, rR.dy)
      ..lineTo(rB.dx, rB.dy)
      ..lineTo(rL.dx, rL.dy)
      ..close();
    canvas.drawPath(roof, Paint()..color = const Color(0xFF15171B));
    canvas.drawPath(
      roof,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1
        ..color = const Color(0xFF262A31),
    );
    _acUnits(canvas, rT, rB, rL, rR, hw, wallH);

    // ── Neon-Outline (Hero / Auswahl) ──────────────────────────────────
    if (isPlayer || isSelected) {
      _neon(canvas, gL, gB, gR, rR, rT, rL);
    }

    // ── Döner-Pin (Hero) ───────────────────────────────────────────────
    if (isPlayer) _heroPin(canvas, Offset(cx, rT.dy));
  }

  void _fillFaceGradient(Canvas canvas, Path face, Color top, Color bottom) {
    final b = face.getBounds();
    canvas.save();
    canvas.clipPath(face);
    canvas.drawRect(
      b,
      Paint()
        ..shader = LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [top, bottom],
        ).createShader(b),
    );
    canvas.restore();
  }

  void _windows(Canvas canvas, Offset origin, Offset uEnd, double wallH,
      int floors, int faceSeed) {
    final uVec = uEnd - origin;
    const cols = 3;
    final rows = floors;
    const marginU = 0.13, marginTop = 0.10, marginBottom = 0.05;
    final cellU = (1 - 2 * marginU) / cols;
    final cellV = (1 - marginTop - marginBottom) / rows;

    canvas.save();
    canvas.clipPath(Path()
      ..moveTo(origin.dx, origin.dy)
      ..lineTo(uEnd.dx, uEnd.dy)
      ..lineTo(uEnd.dx, uEnd.dy - wallH)
      ..lineTo(origin.dx, origin.dy - wallH)
      ..close());

    var idx = faceSeed * 997;
    final off = Paint()..color = const Color(0xFF0E1014);
    final warmFill = Paint()..color = const Color(0xFFE8A24B);
    final coolFill = Paint()..color = const Color(0xFF4A6B8A);
    final glow = Paint()
      ..color = const Color(0x44E8A24B)
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 3);
    final frame = Paint()
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1
      ..color = const Color(0xFF22252B);

    for (int row = 0; row < rows; row++) {
      final isGround = row == 0;
      for (int col = 0; col < cols; col++) {
        idx++;
        final r = _rng(idx);
        final bool warm = r < style.windowWarmChance;
        final bool cool = !warm && r < style.windowWarmChance + 0.05;
        final paintFill = warm ? warmFill : (cool ? coolFill : off);

        final wFrac = (isGround ? cellU * 0.8 : cellU * 0.55);
        final hFrac = (isGround ? cellV * 0.78 : cellV * 0.52);
        final uA = marginU + col * cellU + (cellU - wFrac) / 2;
        final uB = uA + wFrac;
        final vB = marginBottom + row * cellV + (cellV - hFrac) / 2;
        final vT = vB + hFrac;

        final p0 = _facePoint(origin, uVec, wallH, uA, vB);
        final p1 = _facePoint(origin, uVec, wallH, uB, vB);
        final p2 = _facePoint(origin, uVec, wallH, uB, vT);
        final p3 = _facePoint(origin, uVec, wallH, uA, vT);
        final win = Path()
          ..moveTo(p0.dx, p0.dy)
          ..lineTo(p1.dx, p1.dy)
          ..lineTo(p2.dx, p2.dy)
          ..lineTo(p3.dx, p3.dy)
          ..close();

        if (warm) canvas.drawPath(win, glow);
        canvas.drawPath(win, paintFill);
        if (warm) canvas.drawPath(win, frame);
      }
    }
    canvas.restore();
  }

  void _sign(Canvas canvas, Offset gB, Offset gL, Offset gR, double wallH,
      double floorH) {
    // Akzent-Band knapp über dem Erdgeschoss auf der rechten (helleren) Front.
    final uVec = gR - gB;
    final v0 = (floorH * 0.9) / wallH;
    final v1 = v0 + (floorH * 0.5) / wallH;
    final p0 = _facePoint(gB, uVec, wallH, 0.12, v0);
    final p1 = _facePoint(gB, uVec, wallH, 0.86, v0);
    final p2 = _facePoint(gB, uVec, wallH, 0.86, v1);
    final p3 = _facePoint(gB, uVec, wallH, 0.12, v1);
    final band = Path()
      ..moveTo(p0.dx, p0.dy)
      ..lineTo(p1.dx, p1.dy)
      ..lineTo(p2.dx, p2.dy)
      ..lineTo(p3.dx, p3.dy)
      ..close();
    canvas.drawPath(
      band,
      Paint()
        ..color = style.accentColor.withAlpha(110)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
    );
    canvas.drawPath(band, Paint()..color = style.accentColor.withAlpha(235));
  }

  void _acUnits(Canvas canvas, Offset rT, Offset rB, Offset rL, Offset rR,
      double hw, double wallH) {
    final center = Offset((rT.dx + rB.dx) / 2, (rT.dy + rB.dy) / 2);
    final mw = (hw * 0.20).clamp(3.0, 14.0);
    final bh = (wallH * 0.05).clamp(3.0, 14.0);
    for (final t in const [0.34, -0.30]) {
      final p = Offset.lerp(center, t > 0 ? rL : rR, t.abs())!;
      _miniBox(canvas, p, mw, bh);
    }
  }

  void _miniBox(Canvas canvas, Offset baseCenter, double mw, double bh) {
    final mdv = mw * 0.5;
    final bT = baseCenter.translate(0, -mdv);
    final bR = baseCenter.translate(mw, 0);
    final bB = baseCenter.translate(0, mdv);
    final bL = baseCenter.translate(-mw, 0);
    final tT = bT.translate(0, -bh);
    final tR = bR.translate(0, -bh);
    final tB = bB.translate(0, -bh);
    final tL = bL.translate(0, -bh);
    canvas.drawPath(
      Path()
        ..moveTo(bL.dx, bL.dy)
        ..lineTo(bB.dx, bB.dy)
        ..lineTo(tB.dx, tB.dy)
        ..lineTo(tL.dx, tL.dy)
        ..close(),
      Paint()..color = const Color(0xFF181B20),
    );
    canvas.drawPath(
      Path()
        ..moveTo(bB.dx, bB.dy)
        ..lineTo(bR.dx, bR.dy)
        ..lineTo(tR.dx, tR.dy)
        ..lineTo(tB.dx, tB.dy)
        ..close(),
      Paint()..color = const Color(0xFF101216),
    );
    canvas.drawPath(
      Path()
        ..moveTo(tT.dx, tT.dy)
        ..lineTo(tR.dx, tR.dy)
        ..lineTo(tB.dx, tB.dy)
        ..lineTo(tL.dx, tL.dy)
        ..close(),
      Paint()..color = const Color(0xFF1C2026),
    );
  }

  void _neon(Canvas canvas, Offset gL, Offset gB, Offset gR, Offset rR,
      Offset rT, Offset rL) {
    final outline = Path()
      ..moveTo(gL.dx, gL.dy)
      ..lineTo(gB.dx, gB.dy)
      ..lineTo(gR.dx, gR.dy)
      ..lineTo(rR.dx, rR.dy)
      ..lineTo(rT.dx, rT.dy)
      ..lineTo(rL.dx, rL.dy)
      ..close();
    final color = isPlayer ? style.accentColor : const Color(0xFFFFFAE6);
    canvas.drawPath(
      outline,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 4
        ..color = color.withAlpha(130)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 9),
    );
    canvas.drawPath(
      outline,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3
        ..color = color,
    );
  }

  void _heroPin(Canvas canvas, Offset anchor) {
    final r = 9.0;
    final tip = anchor.translate(0, -6);
    final center = tip.translate(0, -r - 2);
    // Glow
    canvas.drawCircle(
      center,
      r + 4,
      Paint()
        ..color = style.accentColor.withAlpha(120)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6),
    );
    // Teardrop
    final pin = Path()
      ..moveTo(tip.dx, tip.dy)
      ..lineTo(center.dx - r * 0.7, center.dy + r * 0.3)
      ..arcToPoint(Offset(center.dx + r * 0.7, center.dy + r * 0.3),
          radius: Radius.circular(r), clockwise: true)
      ..close();
    canvas.drawPath(pin, Paint()..color = style.accentColor);
    canvas.drawCircle(center, r * 0.4, Paint()..color = const Color(0xFF1A1206));
  }

  @override
  bool shouldRepaint(covariant StreetBuildingPainter old) {
    return old.style != style ||
        old.isPlayer != isPlayer ||
        old.isSelected != isSelected ||
        old.seed != seed;
  }
}
