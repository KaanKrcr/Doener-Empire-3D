# Prompt: Tutorial-Komplettumbau

## Ziel
Das Tutorial muss komplett an die neue UI-Struktur angepasst werden (City Map = Tab 0, Hauptbildschirm).

## Betroffene Dateien

### 1. `lib/models/tutorial_model.dart`
Rewrite the TutorialStep enum and extension to match the new flow:

**Neue Tutorial-Schritte (vereinfacht, an City-Map-Flow angepasst):**
```
1. openFirstShop      → "Erste Filiale eröffnen"
2. changeProductPrice → "Produktpreis anpassen"  
3. hireFirstEmployee  → "Mitarbeiter einstellen"
4. endFirstDay        → "Ersten Tag abschließen"
5. readDayReport      → "Tagesbericht lesen"
6. viewCityMapMetrics → "Stadtkarten-Kennzahlen"
7. finishTutorial     → "Tutorial abschließen"
```

**Entfernte Steps:**
- `understandLocationValues` → Fusionieren mit openFirstShop (Location-Werte sind direkt in der Karte sichtbar)
- `openEmpireMenu` → Vereinfachen, nicht mehr nötig
- `viewDashboardMetrics` → Ersetzt durch viewCityMapMetrics
- `understandHrCompetitionGrowth` → Zu komplex für Tutorial, raus

**Neue targetTabIndex:**
- Alle Steps die auf der City Map stattfinden => 0
- `finishTutorial` => null

**Neue Beschreibungen, Hints, actionLabel, whyItMatters:**
- openFirstShop: "Wähle auf der Stadtkarte einen Standort und eröffne deine erste Filiale."
- changeProductPrice: "Öffne eine Filiale und passe die Preise an."
- hireFirstEmployee: "Stelle in der Filiale einen Mitarbeiter ein."
- endFirstDay: "Beende den Tag über den goldenen Button."
- readDayReport: "Lies den Tagesbericht und bestätige ihn."
- viewCityMapMetrics: "Sieh dir auf der Karte Umsatz und Filialen an."
- finishTutorial: "Tutorial abschließen und frei spielen."

**actionLabel:**
- changeProductPrice: "Preis geändert"
- viewCityMapMetrics: "Verstanden"
- finishTutorial: "Tutorial beenden"
- rest: null

**Tutorial step count auf 7 setzen** (`kTutorialStepCount = 7`)

### 2. `lib/providers/game_provider.dart`
- `onTutorialTabOpened` anpassen: checkt nur noch `openFirstShop` (tabIndex == 0) für Auto-Advance
- Alle veralteten Step-Referenzen entfernen/ersetzen
- `_completeTutorialStep` und `markTutorialDone` beibehalten

### 3. `lib/ui/main_scaffold.dart` (minimal)
- Tutorial Card UI beibehalten (existierende Widgets sind gut)
- Keine großen Änderungen am Card-Layout

### Wichtig:
- `flutter analyze` muss sauber sein
- `flutter test` muss weiterhin 100/100 bestehen
- Keine Logik am GameEngine oder Provider-Architektur ändern
- Nur Tutorial-Step-Definition + Flow anpassen
