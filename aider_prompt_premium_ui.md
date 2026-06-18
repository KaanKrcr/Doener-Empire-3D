# Prompt: Premium Dark Mode UI Theme

Ändere die bestehende Datei `lib/core/theme.dart`, um das aktuelle Döner-Rot/Orange-Theme in ein Premium-Dark-Mode-Design mit Gold/Amber-Akzenten zu verwandeln.

## Ziel-Design (aus Referenz-Bildanalyse):
- 1024x1536 Portrait (Mobile)
- Premium Dark Mode ohne helles Theme
- Minimalistisch, edel, card-basiert

## Farbpalette:
- **Hintergrund (scaffold)**: `#231F19` (dunkles Warmbraun) — nicht mehr reines Schwarz
- **Akzentfarbe (Buttons, CTAs, Highlights)**: `#D46816` (Gold/Amber) — die einzige kräftige Farbe
- **Karten/Surface**: `#3D2E22` (warmes Mittelbraun) für `bgCard`
- **Header/Navigation**: `#141010` (fast schwarz) für `bgTab` und `bgSurface`
- **Text primär**: `#FFFAE6` (warmweiß, nicht reines Weiß)
- **Text sekundär**: `#C4B5A0` (Sand)
- **Text muted**: `#7A6A5C` (dunkles Sand)
- **Border**: `#3A2C20` zum `bgCard` abheben
- **Border heller**: `#4D3A28` für Hover/Selected
- **Divider**: genau wie border, sehr dezent

## Anpassungen:

### 1. `AppColors`
- `primary` → `#D46816` (Gold/Amber)
- `primaryDark` → `#B85700` (dunkleres Gold)
- `primaryGlow` → `#26D46816` (Glow 15% opacity)
- `secondary` → `#F59E0B` (wärmeres Goldgelb)
- Entferne `tomato` und `onion` (zu food-themig)
- Ersetze `accent` → bleibt Goldton
- `bg` → `#231F19`
- `bgCard` → `#3D2E22`
- `bgCardHover` → `#4D3A28`
- `bgSurface` → `#1A1815`
- `bgTab` → `#141010`
- `bgGlow` → eine der Gold-Varianten

### 2. CardTheme
- cards sollen einen leichten, warmen Schatten haben (nicht flach)
- Karten-Radius: 16px statt 20px (edler, weniger spielerisch)
- Border: `AppColors.border`
- Padding der Cards: horizontal 20px, vertikal 16px

### 3. Buttons
- **ElevatedButton** → Gold gefüllt (`AppColors.primary`), abgerundet 12px, Hover-Effekt
- **OutlinedButton** → Gold Border, transparenter Hintergrund
- **TextButton** → Gold Text, kein Hintergrund

### 4. BottomNavigationBar
- Hintergrund: `#141010` (fast schwarz)
- Selected: Gold (`AppColors.primary`)
- Unselected: `textMuted`
- Keine Elevation

### 5. AppBar
- Hintergrund: `bg`
- Keine Unterlinie
- Title: goldfarbener displayText

### 6. TextTheme
- Alle Textfarben an neue `AppColors` anpassen
- `displayFont` weiterhin 'Baloo2', body weiterhin 'Inter'
- displayLarge: 30px (etwas kleiner = edler), Farbe warmweiß
- displayMedium: 24px

### 7. Dialog/Modal
- Hintergrund: `bgSurface`
- Border-Radius: 20px (geschlossener)
- Title gold

### Wichtig:
- KEINE Logik ändern, KEINE neuen Dateien erstellen, KEINE Screens ändern
- NUR `lib/core/theme.dart` bearbeiten
- Alle `AppColors`-Konstanten und `AppTheme.dark` anpassen
- `AppGradients` an die neue Gold-Palette anpassen (Flame → Gold-Gradient)
- Nach Änderung: `flutter analyze` und `flutter test` laufen lassen
