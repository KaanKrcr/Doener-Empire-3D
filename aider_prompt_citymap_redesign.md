# Prompt: City Map als Hauptbildschirm + Premium Redesign

## Ziel
Die City Map soll der primäre Bildschirm des Spiels sein (Tab 0 im MainScaffold), nicht mehr die DashboardScreen. Das gesamte Layout soll dem Premium-Dark-Mode-Design entsprechen (siehe aktuelles theme.dart).

## Änderungen

### 1. Main Scaffold (`lib/ui/main_scaffold.dart`)
- Ersetze `DashboardScreen()` als ersten Screen (`kTabDashboard = 0`) durch einen Container, der die City Map anzeigt
- Die City Map soll die erste Stadt aus `gameState.unlockedCityIds` laden (oder `kAllCities.first.id` falls keine freigeschaltet)
- Entferne die `DashboardScreen`-Importe und den Tab-Eintrag (Stats kann rein)
- Bottom Nav: "Imbiss" Label → "Stadtkarte" oder kürzer

### 2. City Map Screen (`lib/ui/screens/city_map_screen.dart`)
Komplett neu aufsetzen als Premium-Dark-Mode-Design:

#### Layout (von oben nach unten, 1024x1536 Portrait)
```
┌──────────────────────┐
│  Status Bar (dunkel)  │
│  Header: Stadtname    │
│  + Cash-Anzeige       │
├──────────────────────┤
│  Summary Strip        │
│  (Premium-Karten)     │
├──────────────────────┤
│                      │
│  2.5D City Map       │
│  (CityMapView)       │
│                      │
├──────────────────────┤
│  Location Panel      │
│  (Gold-CTA Button)   │
├──────────────────────┤
│  Eigene Filialen     │
│  (Shop-Karten)       │
├──────────────────────┤
│  Bottom Nav          │
└──────────────────────┘
```

#### Neuer Header (statt alter AppBar)
- Sehr dunkel (#141010), kein Schatten
- Links: Stadtname in Baloo2 Gold (#D46816)
- Rechts: Cash-Anzeige mit Icon, goldenem Text
- Kompakt, ~50px Höhe

#### Summary Strip
- 2 Premium-Karten nebeneinander
- Karte 1: "Umsatz" mit goldenem Icon + Wert in warmweiß
- Karte 2: "Filialen" goldener Icon + Anzahl
- Kartenhintergrund: #3D2E22, 16px Radius, sanfter Schatten
- Icons der gold-Akzentfarbe

#### Location Panel (wenn Standort ausgewählt)
- Premium-Karte mit Header (Standortname in Gold)
- Details: Miete, Kaution, Traffic, Konkurrenz-Druck
- Jede Detail-Zeile: Label links (sand), Wert rechts (warmweiß)
- Gold-CTA Button "Filiale eröffnen" — vollflächig, abgerundet 12px
- Wenn bereits eigene Filiale: Button anders labeln ("Verwalten")

#### Shop-Karten (eigene Filialen)
- Pro Shop eine Premium-Karte
- Header: Shop-Name in warmweiß (Baloo2)
- Subtitle: Standort + ★ Bewertung + Umsatz
- Antippen → navigiert zu /shop/{id}
- Gold-Akzent-Linie links am Kartenrand (borderLeft)

### 3. Farben & Styling (bereits in theme.dart)
- Hintergrund: #231F19
- Karten: #3D2E22, Border #3A2C20, 16px Radius, Schatten
- Gold-Akzente: #D46816
- Text warmweiß: #FFFAE6
- Text sekundär: #C4B5A0

### Wichtig:
- **KEINE Logik ändern** — GameController, Provider, Engine, Services unangetastet
- **KEINE neuen Abhängigkeiten** einführen
- `flutter analyze` muss sauber sein
- `flutter test` muss weiterhin 100/100 Tests bestehen
- CityMapView (lib/ui/widgets/city_map_view.dart) bleibt unverändert
