# UI Theme — Premium Foundation

Single source of truth for visual tokens (colors, typography, spacing,
elevation) — derived from [`docs/UI_STYLE_GUIDE.md`](../../../../docs/UI_STYLE_GUIDE.md).

## Files

- `Theme.uss` — alle USS Custom Properties + Foundation-Component-Klassen
  (`.decision-sheet`, `.metric-tile`, `.btn--primary`, …)

## Verwendung in einem UXML

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="project://database/Assets/UI/Theme/Theme.uss" />

    <ui:VisualElement class="theme-root">
        <ui:VisualElement class="decision-sheet">
            <ui:Label class="section-label" text="STANDORT" />
            <ui:Label class="text-h1" text="Hauptstrasse 12" />

            <ui:VisualElement class="metric-strip">
                <ui:VisualElement class="metric-tile">
                    <ui:Label class="metric-tile__label" text="MARKTANTEIL" />
                    <ui:Label class="metric-tile__value" text="34%" />
                    <ui:Label class="metric-tile__sub"   text="Stadt 12%" />
                </ui:VisualElement>
                <!-- weitere Tiles … -->
            </ui:VisualElement>

            <ui:Button class="btn btn--primary"   text="OPTIMIEREN" />
            <ui:Button class="btn btn--secondary" text="FILIALE ÖFFNEN" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

## Konventionen

1. **Keine rohen Hex-Werte** in Komponenten-USS. Immer `var(--color-…)`.
2. **Spacing nur über Token** (`--space-sm` … `--space-3xl`).
3. **Tap-Targets ≥ 44 px** (siehe `.btn` Höhe).
4. **Letter-Spacing für Labels** über `--letter-spacing-label`.
5. **State-Modifier** als BEM-Suffix: `.btn--primary`, `.btn--secondary`,
   `.metric-tile__label`, `.metric-tile__value`.

## Was hier NICHT lebt

- Layout-spezifische USS für einzelne Screens → in `Assets/UI/Screens/<Name>/`.
- UXML-Files → in `Assets/UI/Screens/<Name>/`.
- Schriftarten (Inter, Baloo2) → in `Assets/UI/Fonts/` als `TMP_FontAsset`
  (noch nicht importiert).

## Was noch fehlt (Codex/Owner)

- [ ] **Fonts importieren** — Owner: Inter + Baloo2 als TTF in
  `Assets/UI/Fonts/`. Dann `-unity-font-definition` in `Theme.uss` setzen.
- [ ] **Icon-Set** — z.B. Lucide oder Heroicons als SVG → USS-Background-Image.
- [ ] **Sparkline-Custom-Element** — für Mini-Trend in den KPI-Kacheln
  (`metric-tile__sparkline`); braucht Custom-`VisualElement` mit `MeshGenerationContext`.
- [ ] **Donut-Custom-Element** — für Marktanteil-Tile.
