# Unity-Port — Task Board

Verbindliche Aufgaben- und Verantwortlichkeits-Tabelle für den Übergang
Flutter → Unity. Single Source of Truth für Claude, Codex und Owner.
Stand: **2026-06-06**.

**Lese-Vorgaben vor Arbeit:**
1. `docs/UNITY_PRODUCT_VISION.md` — verbindliche Produktrichtung (Management-Sim, kein Arcade)
2. `docs/UNITY_REWRITE_PLAN.md` — Meilensteine M0–M9
3. `docs/UNITY_SAVE_COMPAT.md` — JSON-Feld- und Enum-Kompatibilität
4. `docs/UNITY_CITY_MAP_UX.md` — UX-Spec für Vertical Slice
5. `docs/UI_STYLE_GUIDE.md` — visuelle Foundation
6. `AGENTS.md` — Agenten-Regeln

## Legende

| Symbol | Bedeutung |
|---|---|
| 🟢 | erledigt |
| 🟡 | in Arbeit / unterwegs |
| 🔵 | bereit zum Start |
| ⚪ | blockiert durch andere Aufgabe |
| 🔴 | blockiert durch externe Ressource (Asset, Lizenz, Hardware) |

| Wer | Bedeutung |
|---|---|
| **O** | Owner (Kaan) — Hardware, Login, GUI, Lizenz |
| **C** | Claude — Architektur, Review, kleine chirurgische Edits, Diagnose |
| **X** | Codex — generative Brot-und-Butter-Arbeit, Engine-Ports, UI-Layouts |

---

## Phase 0 — Toolchain & Bootstrap

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 0.1 | O | 🟢 | Unity Hub installiert |
| 0.2 | O | 🟢 | Unity 6 LTS (6000.4.9f1) Editor installiert |
| 0.3 | O | 🟢 | .NET 8 SDK installiert (8.0.421) |
| 0.4 | O | 🟢 | Git LFS installiert (3.7.1) |
| 0.5 | C | 🟢 | Root `.gitattributes` für LFS-Patterns |
| 0.6 | C | 🟢 | `.editorconfig` für C#/USS/UXML |
| 0.7 | **O** | 🔴 | **Android Build Support + OpenJDK + SDK/NDK via Hub-GUI installieren** |
| 0.8 | C | ⚪ | `Packages/manifest.json` mit URP + Input System (blockiert von 0.7) |
| 0.9 | O | ⚪ | Initiale Editor-Öffnung → erzeugt `ProjectSettings/` (blockiert von 0.7) |
| 0.10 | O | ⚪ | URP-Pipeline-Asset im Editor anlegen (1 Klick, blockiert von 0.9) |
| 0.11 | O | ⚪ | Tablet (Galaxy Tab S9) per USB-Debugging registrieren |

---

## Phase 1 — Logikschicht (UnityEngine-frei, per `dotnet test` verifiziert)

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 1.1 | X | 🟢 | M1 Daten-Layer: Enums, Difficulty, ShopSizeTier, GameData, GameCatalog |
| 1.2 | X | 🟢 | M2 Stateful-Modelle: Shop, GameState, Loan, BrandStats, DailyRecord |
| 1.3 | X | 🟢 | SaveService mit DTO-Pattern (MVP-Felder) |
| 1.4 | C | 🟢 | 5 Assembly Definitions mit `noEngineReferences: true` |
| 1.5 | C | 🟢 | Flutter-Save-Roundtrip-Fixture + 12 Compat-Tests |
| 1.6 | C | 🟢 | Save-Asymmetrien in `UNITY_SAVE_COMPAT.md` dokumentiert |
| 1.7 | C | 🟢 | CI: GitHub Action für `dotnet test` |
| 1.8 | X | 🟡 | M3 GameEngine.SimulateDay — Tagessimulation (Beginn vorhanden) |
| 1.9 | X | 🔵 | **M4a CompetitorEngine-Port** (Spec: `docs/ports/COMPETITOR_ENGINE_PORT.md`) |
| 1.10 | X | 🔵 | M4b CorporateEngine-Port (Buyout, Auto-Hire, Manager) |
| 1.11 | X | 🔵 | M4c HrEngine-Port (Pool, Hiring, Training) |
| 1.12 | X | 🔵 | M4d CampaignEngine + MissionEngine + MarketingEngine |
| 1.13 | C | ⚪ | Save-DTO um nicht-MVP-Felder erweitern, wenn die jeweilige Engine portiert ist (`history`, `missions`, `stocks`, `facilities`, `hr*`, `globalPrices`, `cityPrices`, `activeCityCampaigns`, `activeGlobalCampaigns`, `completedChapterIds`, `activeComboIds`, `productQuality`) |
| 1.14 | C | ⚪ | Code-Review jedes Engine-Ports vor Merge |
| 1.15 | O | 🔵 | Optional: echtes Flutter-Save vom Android-Gerät exportieren (`adb shell run-as de.kaan.doener_empire cat .../shared_prefs/...`) und als zusätzliche Fixture committen |

---

## Phase 2 — Unity-Editor-Verdrahtung (braucht Phase 0)

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 2.1 | C | ⚪ | App-Layer Asmdef (refs Logic + UnityEngine) — sobald App↔UI-Zirkel aufgelöst |
| 2.2 | C | ⚪ | `EventBus` an `GameState` der Logikschicht binden |
| 2.3 | C | ⚪ | Scene-Flow `Boot.unity` → `CityMap.unity` |
| 2.4 | X | ⚪ | Erste 3D-City-Szene mit Iso-Kamera, Pan/Pinch-Zoom (Primitives) |
| 2.5 | X | ⚪ | Hotspot-Marker-Prefab aus `LocationTemplates` instanziieren |
| 2.6 | O | 🔴 | 3D-Asset-Pack lizenzieren (Synty Low-Poly City o.ä.) |
| 2.7 | O | ⚪ | Asset-Pack importieren, in `Assets/Art/Models/` einsortieren |

---

## Phase 3 — UI-Toolkit (parallel zu Phase 2 möglich, sobald GameController steht)

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 3.1 | C | 🟢 | Premium-Theme USS-Foundation (`Assets/UI/Theme/Theme.uss`) |
| 3.2 | O | 🔴 | Fonts Inter + Baloo2 als TTF beistellen, in `Assets/UI/Fonts/` |
| 3.3 | X | ⚪ | `Hud.uxml/.uss` Top-Bar + Status-Strip + Bottom-Nav |
| 3.4 | X | ⚪ | `LocationSheet.uxml/.uss` mit 4 KPI-Kacheln |
| 3.5 | X | ⚪ | Sparkline-Custom-VisualElement |
| 3.6 | X | ⚪ | Donut-Custom-VisualElement (Marktanteil) |
| 3.7 | X | ⚪ | `BuyDialog.uxml`, `RestaurantDetail.uxml`, `DayReport.uxml` |

---

## Phase 4 — Vertical Slice spielbar (MVP-DoD)

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 4.1 | C+O | ⚪ | End-to-End-Playthrough <10 Min: 1. Filiale eröffnen → Preise → Tag simulieren → Report → Upgrade |
| 4.2 | X | ⚪ | Day-End-Visualisierung (GPU-instanced Kundendots) |
| 4.3 | C | ⚪ | Save/Load via `Application.persistentDataPath` |
| 4.4 | O | ⚪ | Tablet-Build + Playtest |

---

## Phase 5 — Polish & Parität

| # | Wer | Status | Aufgabe |
|---|---|---|---|
| 5.1 | C | ⚪ | Balancing-Parität vs. Flutter (10 Tage simulieren, KPIs vergleichen) |
| 5.2 | O+X | ⚪ | Sound (Tap, Day-End, Konfetti) |
| 5.3 | C | ⚪ | Performance-Pass (LODs, Batching, Draw-Call < 100) |
| 5.4 | O | ⚪ | Release-Signing-Keystore (Android) |

---

## Aktuell als Nächstes startbar

**Owner (O):** 0.7 — Android Build Support installieren. Alles in Phase 0/2/3 hängt daran.

**Codex (X):** 1.9 — CompetitorEngine-Port. Spec: `docs/ports/COMPETITOR_ENGINE_PORT.md`. Nicht von 0.7 blockiert (Logikschicht ist UnityEngine-frei).

**Claude (C):** wartet auf Codex-Output (für Review) oder 0.7-Abschluss (für 0.8 manifest.json + 2.x Editor-Verdrahtung).
