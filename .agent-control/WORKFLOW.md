# Workflow: City Map auf Mockup-Niveau

## Reihenfolge

```
Tag 1: Analyse + Entscheidung
┌─────────────────────────────────────────────────┐
│ 1. Codex: Architektur-Entscheidung + Doku       │
│    (CODEX_ARCHITEKTUR_PROMPT.md)                │
│    → Gibt Empfehlung: Iso-Stadt vs Drill-Down   │
│                                                 │
│ 2. Local LM: Code-Review + Spec-Extraktion      │
│    (LOCAL_LM_REVIEW_PROMPT.md)                  │
│    → Findet Inkonsistenzen                     │
│                                                 │
│ 3. DU: Entscheidung fällen                      │
│    → Iso-Stadt oder Drill-Down-Upgrade          │
└─────────────────────────────────────────────────┘

Tag 2: Implementation
┌─────────────────────────────────────────────────┐
│ 4. Claude Code: Palette wärmer machen           │
│    (CLAUDE_CODE_IMPL_PROMPT.md Schritt 1)       │
│    → flutter analyze + test müssen grün bleiben │
│                                                 │
│ 5. Codex: MAP_DESIGN_SPEC.md aktualisieren      │
│    (CODEX_ARCHITEKTUR_PROMPT.md Aufgabe 1)      │
│    → Doku an aktuellen Code-Stand angleichen    │
│                                                 │
│ 6. Claude Code: Hybrid-Prototyp an Daten binden │
│    (CLAUDE_CODE_IMPL_PROMPT.md Schritt 2)       │
│    → Funktionierender Prototyp mit GameState    │
│                                                 │
│ 7. Local LM: Review nach Implementation         │
│    → Hat Claude Code sauber gearbeitet?         │
└─────────────────────────────────────────────────┘

Tag 3+: Assets + Architektur (je nach Entscheidung)
┌─────────────────────────────────────────────────┐
│ 8. Codex: Blender-Workflow-Dokumentation        │
│    (CODEX_ARCHITEKTUR_PROMPT.md Aufgabe 3)      │
│                                                 │
│ 9. Local LM + Blender: Gebäude-Sprites rendern  │
│    → assets/iso/building_*.png ersetzen         │
│                                                 │
│ 10. Claude Code: Karten-Architektur-Umbau       │
│     → Eine Iso-Stadt (falls Entscheidung dafür) │
│                                                 │
│ 11. Local LM: Finaler Review + Test-Check       │
└─────────────────────────────────────────────────┘
```

## Prompts zum Starten

| Tool | Prompt-Datei |
|---|---|
| **Codex** | `.agent-control/CODEX_ARCHITEKTUR_PROMPT.md` |
| **Claude Code** | `.agent-control/CLAUDE_CODE_IMPL_PROMPT.md` |
| **Local LM** | `.agent-control/LOCAL_LM_REVIEW_PROMPT.md` |
