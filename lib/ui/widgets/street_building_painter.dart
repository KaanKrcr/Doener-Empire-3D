import 'dart:math' as math;
import 'dart:ui' show PointMode;

import 'package:flutter/material.dart';

import '../../models/building_style_model.dart';

/// Zeichnet EIN Gebäude im 2.5D-Stil (leichte Iso-Tiefe nach rechts) komplett
/// aus [BuildingStyle]-Parametern — keine Texturen/Bitmaps.
///
/// Das Gebäude wird am unteren Rand der Zeichenfläche verankert; die Höhe
/// skaliert mit [BuildingStyle.floors].
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

  // Deterministische Pseudo-Zufallszahl 0..1 aus einem Integer.
  double _rng(int n) {
    final x = math.sin((n + seed) * 12.9898) * 43758.5453;
    return x - x.floorToDouble();
  }

  @override
  void paint(Canvas canvas, Size size) {
    final w = size.width;
    final h = size.height;
    if (w <= 4 || h <= 4) return;

    final depth = (w * 0.16).clamp(5.0, 36.0);
    final depthDy = depth * 0.55;

    final groundY = h * 0.94;
    final bodyW = w * 0.60;
    final leftX = w * 0.13;
    final rightX = leftX + bodyW;

    final floors = style.floors.clamp(1, 6);
    final floorH = (h * 0.6) / 5.0;
    var bodyH = floorH * floors + h * 0.06;
    final maxBodyH = groundY - (depthDy + h * 0.18);
    bodyH = bodyH.clamp(h * 0.22, math.max(h * 0.22, maxBodyH));
    final topY = groundY - bodyH;

    // ── Boden-Glow (nur eigene Filiale) ──────────────────────────────────
    if (isPlayer) {
      canvas.drawOval(
        Rect.fromCenter(
          center: Offset(leftX + bodyW / 2 + depth / 2, groundY + 3),
          width: bodyW * 1.35,
          height: h * 0.12,
        ),
        Paint()
          ..color = style.accentColor.withAlpha(70)
          ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 10),
      );
    }

    // ── Seitenfläche (rechts, dunkel) ────────────────────────────────────
    final sidePath = Path()
      ..moveTo(rightX, groundY)
      ..lineTo(rightX, topY)
      ..lineTo(rightX + depth, topY - depthDy)
      ..lineTo(rightX + depth, groundY - depthDy)
      ..close();
    canvas.drawPath(sidePath, Paint()..color = style.facadeDarkColor);

    // ── Frontfläche (hell) ───────────────────────────────────────────────
    final frontRect = Rect.fromLTRB(leftX, topY, rightX, groundY);
    canvas.drawRect(frontRect, Paint()..color = style.facadeLightColor);

    _paintFacadeTexture(canvas, frontRect);
    _paintWindows(canvas, frontRect, floors);
    if (style.hasAwning) _paintAwning(canvas, leftX, rightX, groundY, floorH);
    _paintDoor(canvas, leftX, rightX, groundY, floorH);
    if (style.condition == BuildingCondition.worn) _paintWear(canvas, frontRect);
    _paintRoof(canvas, leftX, rightX, topY, bodyH, depth, depthDy);
    if (style.hasSign) _paintSign(canvas, leftX, rightX, topY, floorH);

    if (isPlayer || isSelected) {
      _paintNeon(canvas, leftX, rightX, topY, groundY, depth, depthDy);
    }
  }

  // ── Fassaden-Textur ────────────────────────────────────────────────────
  void _paintFacadeTexture(Canvas canvas, Rect r) {
    canvas.save();
    canvas.clipRect(r);
    final line = Paint()
      ..color = style.facadeDarkColor.withAlpha(120)
      ..strokeWidth = 1;
    switch (style.facadeType) {
      case FacadeType.brick:
        for (double y = r.top + 8; y < r.bottom; y += 9) {
          canvas.drawLine(Offset(r.left, y), Offset(r.right, y), line);
        }
        break;
      case FacadeType.wood:
        for (double x = r.left + 7; x < r.right; x += 8) {
          canvas.drawLine(Offset(x, r.top), Offset(x, r.bottom), line);
        }
        break;
      case FacadeType.concrete:
        canvas.drawLine(Offset(r.center.dx, r.top), Offset(r.center.dx, r.bottom),
            line..color = style.facadeDarkColor.withAlpha(90));
        break;
      case FacadeType.glass:
        canvas.drawRect(
          r,
          Paint()..color = const Color(0xFF6FA8C7).withAlpha(28),
        );
        break;
      case FacadeType.plaster:
        break;
    }
    canvas.restore();
  }

  // ── Fenster ──────────────────────────────────────────────────────────
  void _paintWindows(Canvas canvas, Rect r, int floors) {
    if (style.windowPattern == WindowPattern.none) return;
    final cols = style.windowPattern == WindowPattern.stripe ? 1 : 3;
    final rows = floors;
    final padX = r.width * 0.14;
    final padTop = r.height * 0.10;
    final padBottom = r.height * 0.20; // Erdgeschoss frei für Tür/Markise
    final usableW = r.width - padX * 2;
    final usableH = r.height - padTop - padBottom;
    if (usableW <= 0 || usableH <= 0 || rows <= 0) return;

    final cellW = usableW / cols;
    final cellH = usableH / rows;
    final winW = style.windowPattern == WindowPattern.stripe
        ? usableW
        : cellW * 0.6;
    final winH = cellH * 0.55;

    final dark = Paint()..color = const Color(0xFF0B0D10);
    final warm = Paint()..color = const Color(0xFFFFCB6B);
    final coolGlass = Paint()..color = const Color(0xFF2A4A5C);

    var idx = 0;
    for (int row = 0; row < rows; row++) {
      for (int col = 0; col < cols; col++) {
        idx++;
        if (style.windowPattern == WindowPattern.random && _rng(idx * 7) < 0.25) {
          continue;
        }
        final cx = r.left +
            padX +
            (style.windowPattern == WindowPattern.stripe
                ? 0
                : col * cellW + (cellW - winW) / 2);
        final cy = r.top + padTop + row * cellH + (cellH - winH) / 2;
        final rect = Rect.fromLTWH(cx, cy, winW, winH);
        final lit = _rng(idx * 13) < style.windowWarmChance;
        canvas.drawRRect(
          RRect.fromRectAndRadius(rect, const Radius.circular(1.5)),
          lit
              ? warm
              : (style.facadeType == FacadeType.glass ? coolGlass : dark),
        );
      }
    }
  }

  // ── Markise ──────────────────────────────────────────────────────────
  void _paintAwning(
      Canvas canvas, double leftX, double rightX, double groundY, double floorH) {
    final top = groundY - floorH * 1.05;
    final path = Path()
      ..moveTo(leftX, top)
      ..lineTo(rightX, top)
      ..lineTo(rightX, top + floorH * 0.32)
      ..lineTo(leftX, top + floorH * 0.32)
      ..close();
    canvas.drawPath(path, Paint()..color = style.accentColor.withAlpha(220));
    // Streifen
    final stripe = Paint()..color = Colors.black.withAlpha(40)..strokeWidth = 1;
    final n = 6;
    for (int i = 1; i < n; i++) {
      final x = leftX + (rightX - leftX) * i / n;
      canvas.drawLine(Offset(x, top), Offset(x, top + floorH * 0.32), stripe);
    }
  }

  // ── Tür ──────────────────────────────────────────────────────────────
  void _paintDoor(
      Canvas canvas, double leftX, double rightX, double groundY, double floorH) {
    final doorW = (rightX - leftX) * 0.22;
    final doorH = floorH * 0.7;
    final cx = leftX + (rightX - leftX) * 0.5 - doorW / 2;
    final rect = Rect.fromLTWH(cx, groundY - doorH, doorW, doorH);
    canvas.drawRRect(
      RRect.fromRectAndCorners(rect,
          topLeft: const Radius.circular(2), topRight: const Radius.circular(2)),
      Paint()..color = const Color(0xFF0B0D10),
    );
    canvas.drawRRect(
      RRect.fromRectAndCorners(rect,
          topLeft: const Radius.circular(2), topRight: const Radius.circular(2)),
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1
        ..color = style.accentColor.withAlpha(120),
    );
  }

  // ── Abnutzung (worn) ───────────────────────────────────────────────────
  void _paintWear(Canvas canvas, Rect r) {
    final crack = Paint()
      ..color = Colors.black.withAlpha(70)
      ..strokeWidth = 1.2
      ..style = PaintingStyle.stroke;
    for (int i = 0; i < 3; i++) {
      final sx = r.left + r.width * (0.2 + 0.3 * i);
      final path = Path()..moveTo(sx, r.top + r.height * 0.2);
      var x = sx;
      var y = r.top + r.height * 0.2;
      for (int s = 0; s < 4; s++) {
        x += (_rng(i * 10 + s) - 0.5) * r.width * 0.12;
        y += r.height * 0.15;
        path.lineTo(x, y);
      }
      canvas.drawPath(path, crack);
    }
    // dunkle Verschmutzung unten
    canvas.drawRect(
      Rect.fromLTWH(r.left, r.bottom - r.height * 0.18, r.width, r.height * 0.18),
      Paint()..color = Colors.black.withAlpha(45),
    );
  }

  // ── Dach ───────────────────────────────────────────────────────────────
  void _paintRoof(Canvas canvas, double leftX, double rightX, double topY,
      double bodyH, double depth, double depthDy) {
    final roofPaint = Paint()..color = style.roofColor;
    final roofTopPaint = Paint()
      ..color = Color.lerp(style.roofColor, Colors.white, 0.08)!;

    Path topFace() => Path()
      ..moveTo(leftX, topY)
      ..lineTo(rightX, topY)
      ..lineTo(rightX + depth, topY - depthDy)
      ..lineTo(leftX + depth, topY - depthDy)
      ..close();

    switch (style.roofType) {
      case RoofType.flat:
        canvas.drawPath(topFace(), roofTopPaint);
        // niedrige Attika vorne
        canvas.drawRect(
          Rect.fromLTWH(leftX, topY - 3, rightX - leftX, 3),
          roofPaint,
        );
        break;
      case RoofType.stepped:
        canvas.drawPath(topFace(), roofTopPaint);
        var lx = leftX, rx = rightX, ty = topY;
        for (int i = 0; i < 2; i++) {
          final inset = (rx - lx) * 0.18;
          lx += inset;
          rx -= inset;
          final stepH = bodyH * 0.07;
          canvas.drawRect(
              Rect.fromLTRB(lx, ty - stepH, rx, ty), roofPaint);
          ty -= stepH;
        }
        break;
      case RoofType.pointed:
        final gableH = (bodyH * 0.22).clamp(8.0, 60.0);
        final midX = (leftX + rightX) / 2;
        // Seitliche Dachfläche
        final sideRoof = Path()
          ..moveTo(rightX, topY)
          ..lineTo(midX, topY - gableH)
          ..lineTo(midX + depth, topY - gableH - depthDy)
          ..lineTo(rightX + depth, topY - depthDy)
          ..close();
        canvas.drawPath(sideRoof, roofPaint);
        // Frontgiebel
        final gable = Path()
          ..moveTo(leftX, topY)
          ..lineTo(rightX, topY)
          ..lineTo(midX, topY - gableH)
          ..close();
        canvas.drawPath(gable, roofTopPaint);
        break;
      case RoofType.sawtooth:
        canvas.drawPath(topFace(), roofPaint);
        final n = 3;
        final segW = (rightX - leftX) / n;
        final toothH = (bodyH * 0.12).clamp(5.0, 28.0);
        final teeth = Paint()..color = roofTopPaint.color;
        for (int i = 0; i < n; i++) {
          final x0 = leftX + i * segW;
          final p = Path()
            ..moveTo(x0, topY)
            ..lineTo(x0 + segW, topY)
            ..lineTo(x0 + segW, topY - toothH)
            ..close();
          canvas.drawPath(p, teeth);
        }
        break;
    }
  }

  // ── Leuchtreklame ──────────────────────────────────────────────────────
  void _paintSign(Canvas canvas, double leftX, double rightX, double topY,
      double floorH) {
    final signW = (rightX - leftX) * 0.7;
    final signH = floorH * 0.42;
    final cx = leftX + (rightX - leftX - signW) / 2;
    final rect = Rect.fromLTWH(cx, topY + floorH * 0.16, signW, signH);
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(3)),
      Paint()..color = style.accentColor.withAlpha(235),
    );
    canvas.drawRRect(
      RRect.fromRectAndRadius(rect, const Radius.circular(3)),
      Paint()
        ..color = style.accentColor.withAlpha(120)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 6),
    );
  }

  // ── Neon-Outline (Spieler / Auswahl) ─────────────────────────────────
  void _paintNeon(Canvas canvas, double leftX, double rightX, double topY,
      double groundY, double depth, double depthDy) {
    final outline = Path()
      ..moveTo(leftX, groundY)
      ..lineTo(leftX, topY)
      ..lineTo(rightX, topY)
      ..lineTo(rightX + depth, topY - depthDy)
      ..lineTo(rightX + depth, groundY - depthDy);
    final color = isPlayer ? style.accentColor : const Color(0xFFFFFAE6);
    canvas.drawPath(
      outline,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 3
        ..color = color.withAlpha(120)
        ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 5),
    );
    canvas.drawPath(
      outline,
      Paint()
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.6
        ..color = color,
    );
    // Eckpunkte betonen
    canvas.drawPoints(
      PointMode.points,
      [Offset(leftX, topY), Offset(rightX, topY)],
      Paint()
        ..color = color
        ..strokeWidth = 3
        ..strokeCap = StrokeCap.round,
    );
  }

  @override
  bool shouldRepaint(covariant StreetBuildingPainter old) {
    return old.style != style ||
        old.isPlayer != isPlayer ||
        old.isSelected != isSelected ||
        old.seed != seed;
  }
}
