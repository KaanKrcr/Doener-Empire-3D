# UNITY_CITY_MAP_UX — City Map & Standortauswahl (Vertical Slice)

Stand: 2026-06-02
Rolle: Engineering Planner/Reviewer (Spec für Codex-Implementierung)
Bindende Richtung: [.agent-control/CURRENT_DECISION.md](../.agent-control/CURRENT_DECISION.md)
Vision: [UNITY_PRODUCT_VISION.md](UNITY_PRODUCT_VISION.md) · Theme: [UI_STYLE_GUIDE.md](UI_STYLE_GUIDE.md)
Zielbild: `docs/assets/doener_empire_mobile_premium_ui.png`

> **Management-Spiel, kein Arcade.** Kein Echtzeit-Servieren, kein manuelles
> Zubereiten. Die City Map + Standortauswahl ist die Hauptbühne; alle Aktionen
> sind Management-Entscheidungen (kaufen, upgraden, Tag simulieren, Bericht).

Diese Spec deckt den **Vertical Slice** ab (CURRENT_DECISION 1–7):
Map öffnen → Standort wählen → Bottom-Sheet → kaufen/upgraden → Tag simulieren →
Tagesbericht.

---

## 1. Screen-Übersicht & Navigation

```
[Boot] → [CityMap]  ⇆  [LocationSheet] (Bottom-Sheet-Overlay)
                          │
                          ├─ frei  → [BuyDialog] → zurück zu CityMap (Filiale jetzt „owned")
                          └─ owned → [RestaurantDetail] (Tabs)
[CityMap] → HUD-Button „Tag beenden" → [DaySimOverlay] → [DayReport] → zurück zu CityMap
```

Eine Szene `CityMap.unity` trägt die 3D-Karte + alle UI-Overlays (UI Toolkit).
RestaurantDetail und DayReport sind UI-Panels in derselben Szene (kein Scene-Load).

---

## 2. CityMap-Screen (Hauptbühne)

### 2.1 Layout (nach Referenzbild)
```
┌─────────────────────────────────────────────┐
│ [🥙] Döner Empire            [📊] [🔔] [⚙]  │  ← Top-Bar
│ 💶 1.248.750 €     📅 Tag 47 · Mittwoch      │  ← Status-Strip
├─────────────────────────────────────────────┤
│                                             │
│         (2.5D/3D ISO-STADT)        [◎]      │  ← FAB: Fokus eigene Filiale
│   • eigene Filiale (Neon-Outline)  [🗺]      │  ← FAB: Übersicht/Zoom-Reset
│   • Konkurrenz-Läden (Label + ★)   [📈]      │  ← FAB: Markt/Stats
│   • freie Hotspots (Pin + Mini-KPI)         │
│                                             │
├─────────────────────────────────────────────┤
│  Übersicht  Filialen  Manager  Forschung  Shop │ ← Bottom-Nav
└─────────────────────────────────────────────┘
```

### 2.2 Kamera
- Orthografisch, fester Iso-Winkel (~30° X / 45° Y).
- **Pan** (1-Finger-Drag), **Pinch-Zoom** (Orthographic Size, klemmen), Inertia optional.
- Tap auf Hotspot/Filiale → sanfter Fokus-Tween + LocationSheet öffnet.
- Safe-Area: Top-Bar + Bottom-Sheet überlagern die Karte → Kamera-Fokus mittig-oberhalb des Sheets.

### 2.3 Hotspot-Zustände (klar unterscheidbar — REVIEW_CHECKLIST „locked/available/owned")
| Zustand | Visuell | Tap-Verhalten |
|---|---|---|
| **owned** (eigene Filiale) | Gebäude mit **oranger Neon-Outline**, Modell je `sizeTier`, schwebendes Label „Name ★Rep" | LocationSheet (owned) |
| **available** (frei, in freigeschalteter Stadt) | Pin-Marker (grau/weiß), Mini-KPI (Traffic), dezenter Glow | LocationSheet (frei) |
| **locked** (Stadt/Standort nicht freigeschaltet) | abgedunkelt, Schloss-Icon, kein Glow | Toast „Erst durch Umsatz/Stadtfreischaltung verfügbar" |
| **competitor** | fremdes Lade-Modell, rotes Label „Konkurrenzname ★", optional rote Einflusszone | Info-Popover (read-only): Name, Reputation, Preisniveau, Marktanteil |

### 2.4 Daten-Bindings (aus dem portierten C#-State)
- Eigene Filialen: `GameState.Shops` (Position via `LocationTemplate`/Hotspot-Mapping).
- Konkurrenz: `GameState.Competitors` (pro `CityId`).
- Freie Hotspots: `GameCatalog.LocationTemplates[cityTier]` minus belegte.
- Kontostand: `GameState.Cash`; Tag: `GameState.CurrentDay` (+ Wochentag aus Tag % 7).

---

## 3. LocationSheet (Bottom-Sheet) — Herzstück

Öffnet von unten (≤ 80 % Höhe), Backdrop dimmt Karte, Karte bleibt schwenkbar
unter dem Sheet (kein harter Modal-Lock).

### 3.1 Owned-Variante (eigene Filiale) — exakt nach Referenzbild
```
┌───────────────────────────────────────────┐
│  Hauptstrasse 12  ✏        ★★★★½  4.6      │
│  📍 Mitte, Berlin            AKTIVE FILIALE ●│
│ ┌─────────┬─────────┬─────────┬──────────┐ │
│ │MARKTANT.│FUSSGÄNG.│WOCHENM. │PROGNOSE  │ │  ← 4 KPI-Kacheln
│ │  ◔ 34%  │ 1.842   │ 8.750 € │ 15.430 € │ │     + Mini-Sparkline
│ │ Stadt12%│ pro Tag │/Woche   │nächste Wo│ │
│ └─────────┴─────────┴─────────┴──────────┘ │
│ [⚙ OPTIMIEREN]            [＋ FILIALE …]   │
└───────────────────────────────────────────┘
```
- KPI-Kacheln: `PremiumMetricStrip`-Äquivalent (USS `.metric-tile`).
  - **Marktanteil**: Donut (eigener Anteil) + Vergleich „Stadt X %".
  - **Fußgänger/Tag**: `Shop.FootTraffic` (ggf. ×Brand/Konkurrenz).
  - **Wochenmiete**: `Shop.WeeklyRent`.
  - **Prognose Gewinn**: nächste-Woche-Schätzung (aus letztem `DailyRecord`-Trend).
- Sparkline: letzte 7 `DailyRecord`-Werte (oder leer bei <7 Tagen).
- **OPTIMIEREN** (primär, orange) → RestaurantDetail.
- **FILIALE ERÖFFNEN** (sekundär) → zur nächsten freien Lage springen.

### 3.2 Frei-Variante (verfügbarer Hotspot)
```
┌───────────────────────────────────────────┐
│  Uni-Viertel                     FREI       │
│  📍 Köln · Hochschul-Lage                   │
│ ┌─────────┬─────────┬─────────┬──────────┐ │
│ │TRAFFIC  │MIETE/Wo │KAUTION  │KONKURRENZ│ │
│ │ 1.2×    │ 5.200 € │ 4.000 € │ mittel   │ │
│ └─────────┴─────────┴─────────┴──────────┘ │
│ 💡 Empfehlung: preissensible Kundschaft –   │  ← Status-Hint
│    Mittags-/Abendgeschäft stark.            │
│ [＋ FILIALE ERÖFFNEN — 4.000 € Kaution]     │
└───────────────────────────────────────────┘
```
- Werte aus `LocationTemplate` (FootTrafficFactor, RentFactor) × `CityData`.
- Konkurrenz-Level aus Anzahl/Stärke `Competitors` in der Stadt.
- Empfehlung aus `LocationPersonality` (Zeitprofil/Preissensibilität).
- Primär-Button disabled + Hint, wenn `Cash < Kaution`.

### 3.3 Zustände
- Lädt Werte beim Öffnen (eingefroren bis Schließen — keine Live-Mutation während offen).
- „✏" (owned) erlaubt Umbenennen (`Shop.CustomName`).

---

## 4. BuyDialog (Filiale eröffnen)

Bestätigungs-Dialog (kompaktes Decision-Sheet):
```
Uni-Viertel, Köln eröffnen?
Kaution:        4.000 €
Wochenmiete:    5.200 €
Empf. Startpreis Döner: 6,50 €
Kapital danach: 11.000 €
[Abbrechen]            [Eröffnen ✓]
```
- Bei Bestätigung: Kaution + erste Wochenmiete abziehen, `Shop` anlegen
  (Default-Menü aus `GameData.AllProducts` isDefault, leeres Equipment/Personal,
  `SizeTier=Klein`), in `GameState.Shops` einfügen, Hotspot → owned, Sheet schließen.
- Re-Use der Sim-Logik aus dem C#-Port (kein neuer Wirtschaftspfad).

---

## 5. RestaurantDetail (Verwaltung) — Tabs

Vollflächiges Premium-Panel (Push über die Karte), Tab-Leiste oben:
```
[Sortiment] [Equipment] [Personal] [Marketing] [Ausbau]
```
| Tab | Inhalt (Bindings) | Aktion |
|---|---|---|
| **Sortiment** | `Shop.Menu` (Produkt, Preis-Slider, Marge, Döner-Index) | Preis setzen → `ShopProduct.Price` |
| **Equipment** | `GameCatalog.AllEquipment` (owned/kaufbar, Boost-KPIs) | kaufen → Cash−, `ShopEquipment` add |
| **Personal** | `Shop.Employees` + Bewerber (`GameState.EmployeePool`) | einstellen/feuern, Cap = `ShopSizing.EmployeeCap(city,tier)` |
| **Marketing** | `GameCatalog`-Kampagnen, aktive (`Shop.ActiveCampaigns`) | buchen → `ActiveCampaign` add |
| **Ausbau** | `ShopSizing` (aktueller→nächster Tier, Cap/Miete/Kosten) | ausbauen → `Shop.SizeTier` next, Cash−, Miete× |

- Jede Karte als `.decision-sheet`, KPIs als `.metric-tile`, Verlustmarge/Cap-Limit
  als `.status-hint` (danger/warning) — Theme aus UI_STYLE_GUIDE.
- **MVP-Minimum:** Sortiment + Ausbau funktionsfähig; Equipment/Personal/Marketing
  dürfen im ersten Slice read-only/Stub sein (klar gekennzeichnet).

---

## 6. Tag simulieren + Tagesbericht

### 6.1 Auslöser
- HUD-Button „Tag beenden" (prominent, im Status-Strip oder als FAB).
- Optional DaySimOverlay: kurze Karten-Animation (Kundendots laufen zu Filialen —
  **rein visuell**, keine Interaktion). Skippbar.

### 6.2 DayReport (nach Simulation)
```
┌─ Tag 47 abgeschlossen ───────────────┐
│  Gewinn  + 15.430 €      (Hero-Wert)  │
│ ┌────────┬────────┬────────┬───────┐ │
│ │UMSATZ  │KOSTEN  │KUNDEN  │Ø RUF  │ │
│ │ 38.900 │ 23.470 │ 1.842  │ 4.6   │ │
│ └────────┴────────┴────────┴───────┘ │
│ TREIBER                               │
│  • Top-Produkt: Dürüm (Marge × Stück) │
│  • Größter Kostenposten: Personal     │
│  • Δ Vortag: Umsatz +6%, Ruf +0.1     │
│ [Verstanden]                          │
└───────────────────────────────────────┘
```
- Werte aus dem neu erzeugten `DailyRecord` (+ Vergleich zum vorherigen).
- Hero = `DailyRecord.Profit` (grün/rot). Bei Verlust rot-betont, gleiche Struktur.
- „Verstanden" schließt → zurück zur Karte (sichtbar: Cash/Tag aktualisiert).

---

## 7. Daten- & Architektur-Anbindung

- **Presentation hört nur auf den GameController via EventBus** (UNITY_PRODUCT_VISION §7):
  UI mutiert State nie direkt. Intents (Buy/Upgrade/SetPrice/EndDay) → Controller →
  Sim-Port → Events (`MoneyChanged`, `ShopsChanged`, `DayEnded`) → UI aktualisiert.
- Sim-Aufrufe nutzen den **bereits portierten C#-Logik-Layer** (`unity/Assets/Scripts/`,
  79 Tests grün): GameData/Catalog, Shop, ShopSizing, Loan, BrandStats … sowie
  (folgend) GameState + GameEngine.
- Keine Wirtschaftslogik in der UI — nur Darstellung + Intent-Auslösung.

---

## 8. Akzeptanzkriterien (Vertical Slice)

- [ ] CityMap zeigt 1 Startstadt iso-3D, Pan/Zoom flüssig (≥ 60 FPS, Primitives/Low-Poly).
- [ ] Hotspots klar in **locked / available / owned / competitor** unterscheidbar.
- [ ] Tap auf Hotspot öffnet das passende LocationSheet (owned vs. frei) mit echten Werten.
- [ ] Frei → BuyDialog → Filiale wird angelegt (Cash−, Default-Menü), Hotspot wird owned.
- [ ] Owned → OPTIMIEREN → RestaurantDetail; **Preis setzen** wirkt auf `ShopProduct.Price`.
- [ ] **Ausbau** erhöht `SizeTier`, zieht Kosten, erhöht Miete & Cap (min Stadt/Stufe).
- [ ] „Tag beenden" simuliert → DayReport zeigt konsistente Werte (Umsatz−Kosten=Gewinn).
- [ ] Premium-Look: Theme-Farben/Typo, Bottom-Sheet & KPI-Kacheln wie Referenzbild.
- [ ] UI mutiert State nur via Controller/Events; `dotnet test` der Logik bleibt grün.
- [ ] Kein Arcade-Element, keine Echtzeit-Bedienung (rein management-basiert).

---

## 9. Out-of-Scope (dieser Slice)
- Mehrere Städte gleichzeitig auf einer Karte, Stadt-Wechsel-Animation.
- Konkurrenz-Aktionen-Visualisierung über read-only Popover hinaus.
- Marketing/Personal-Vollausbau (Stub erlaubt).
- Sound, Partikel-Polish über den einfachen Kundendot-Effekt hinaus.
- Endgame (Börse/Produktion/Konzern-HR) — erst nach Kern-Loop.

---

## 10. Empfohlene Codex-Reihenfolge (kleinste sinnvolle Slices)
1. CityMap-Szene + Kamera (Pan/Zoom) + 1 Boden + statische Hotspots (Dummy-Daten).
2. Hotspot-Zustände + Tap → LocationSheet (frei + owned) mit Dummy-Werten.
3. GameController/EventBus + Anbindung an portierten State (Cash/Tag/Shops).
4. BuyDialog → Filiale anlegen (echte Sim-Daten).
5. RestaurantDetail: Sortiment (Preis) + Ausbau (SizeTier) funktionsfähig.
6. „Tag beenden" + DayReport (sobald GameEngine-Tagessim portiert ist).
7. Premium-Polish (Theme, KPI-Kacheln, Sparklines, Neon-Outline).

> Hinweis Reviewer: Schritte 4–6 hängen am GameState/GameEngine-Port
> (siehe [UNITY_PRODUCT_VISION.md §3](UNITY_PRODUCT_VISION.md)). Schritte 1–3 können
> sofort gegen Dummy/aktuellen State beginnen.
