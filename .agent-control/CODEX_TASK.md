# CODEX_TASK

## Rolle
Implementation Agent.

## Lies zuerst
- .agent-control/CURRENT_DECISION.md
- .agent-control/MASTER_PLAN.md
- .agent-control/REVIEW_CHECKLIST.md
- .agent-control/REVIEW_QUEUE.md
- AGENTS.md

## Aufgabe
Implementiere nur die Flutter-Spur.

Bevorzugte Produktbereiche:
1. City Map als primaerer Ingame-Flow
2. Standort-Prognose, Risiko und Konkurrenzdruck
3. Shop-Detail als Premium-Management-Konsole
4. Equipment, Personal, Marketing und Expansion
5. Day-End-Loop mit klaren Ursachen und Folgen

## Nicht bauen
- Unity-Code oder Unity-Projektdateien
- Unity Android Build
- Arcade Cooking
- Echtzeit-Kundenbedienung
- manuelle Food-Station
- neue Systeme ohne klare Queue-/User-Richtung

## Validierung
Kleinste sinnvolle Checks laufen lassen:
- `flutter analyze`
- `flutter test`
- `flutter build apk --release` fuer testbare Android-Pakete
- mindestens `git diff --check`

## Vor dem Stoppen
STATUS.md aktualisieren und HANDOFF_LOG.md ergaenzen.
